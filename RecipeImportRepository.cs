using Microsoft.Data.SqlClient;
using MealWise.Models;

namespace MealWise.Data;

/// <summary>
/// Saves recipes returned by TheMealDB into the local normalized recipe tables.
/// A transaction ensures a recipe and its ingredients are imported together.
/// </summary>
public sealed class RecipeImportRepository
{
    private readonly DatabaseService _database;

    public RecipeImportRepository(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<int> SaveOrUpdateAsync(
        ExternalMealDetails meal,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(meal);

        await using var connection = _database.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var categoryId = await GetOrCreateLookupAsync(
                connection, transaction, "dbo.RecipeCategories", "CategoryName", "CategoryId",
                meal.Category, cancellationToken);
            var cuisineId = await GetOrCreateLookupAsync(
                connection, transaction, "dbo.Cuisines", "CuisineName", "CuisineId",
                meal.Area, cancellationToken);

            const string upsertRecipeSql = """
                IF EXISTS (SELECT 1 FROM dbo.Recipes WHERE SourceName = N'TheMealDB' AND SourceRecipeId = @ExternalId)
                BEGIN
                    UPDATE dbo.Recipes
                    SET RecipeName = @RecipeName,
                        CategoryId = @CategoryId,
                        CuisineId = @CuisineId,
                        Instructions = @Instructions,
                        ImageUrl = @ImageUrl,
                        SourceUrl = @SourceUrl,
                        UpdatedAt = SYSUTCDATETIME()
                    WHERE SourceName = N'TheMealDB' AND SourceRecipeId = @ExternalId;

                    SELECT RecipeId FROM dbo.Recipes
                    WHERE SourceName = N'TheMealDB' AND SourceRecipeId = @ExternalId;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.Recipes
                        (RecipeName, CategoryId, CuisineId, Instructions, DefaultServings,
                         ImageUrl, SourceName, SourceRecipeId, SourceUrl)
                    VALUES
                        (@RecipeName, @CategoryId, @CuisineId, @Instructions, 4,
                         @ImageUrl, N'TheMealDB', @ExternalId, @SourceUrl);

                    SELECT CAST(SCOPE_IDENTITY() AS int);
                END;
                """;

            int recipeId;
            await using (var command = new SqlCommand(upsertRecipeSql, connection, transaction))
            {
                command.Parameters.AddWithValue("@ExternalId", meal.Id);
                command.Parameters.AddWithValue("@RecipeName", meal.Name);
                command.Parameters.Add("@CategoryId", System.Data.SqlDbType.Int).Value = (object?)categoryId ?? DBNull.Value;
                command.Parameters.Add("@CuisineId", System.Data.SqlDbType.Int).Value = (object?)cuisineId ?? DBNull.Value;
                command.Parameters.AddWithValue("@Instructions", (object?)meal.Instructions ?? DBNull.Value);
                command.Parameters.AddWithValue("@ImageUrl", (object?)meal.ImageUrl ?? DBNull.Value);
                command.Parameters.AddWithValue("@SourceUrl", (object?)meal.SourceUrl ?? DBNull.Value);
                recipeId = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            }

            await using (var deleteIngredients = new SqlCommand(
                "DELETE FROM dbo.RecipeIngredients WHERE RecipeId = @RecipeId;",
                connection, transaction))
            {
                deleteIngredients.Parameters.AddWithValue("@RecipeId", recipeId);
                await deleteIngredients.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var ingredient in meal.Ingredients)
            {
                var ingredientId = await GetOrCreateLookupAsync(
                    connection, transaction, "dbo.Ingredients", "IngredientName", "IngredientId",
                    ingredient.Name, cancellationToken);

                if (ingredientId is null)
                {
                    continue;
                }

                const string insertIngredientSql = """
                    INSERT INTO dbo.RecipeIngredients
                        (RecipeId, IngredientId, MeasureText, IsOptional)
                    VALUES (@RecipeId, @IngredientId, @MeasureText, 0);
                    """;

                await using var ingredientCommand = new SqlCommand(insertIngredientSql, connection, transaction);
                ingredientCommand.Parameters.AddWithValue("@RecipeId", recipeId);
                ingredientCommand.Parameters.AddWithValue("@IngredientId", ingredientId.Value);
                ingredientCommand.Parameters.AddWithValue("@MeasureText", (object?)ingredient.MeasureText ?? DBNull.Value);
                await ingredientCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return recipeId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int?> GetOrCreateLookupAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string tableName,
        string nameColumn,
        string idColumn,
        string? value,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Table and column names are internal constants supplied by this class, never user input.
        var sql = $"""
            DECLARE @FoundId int;
            SELECT @FoundId = {idColumn} FROM {tableName} WHERE {nameColumn} = @Value;
            IF @FoundId IS NULL
            BEGIN
                INSERT INTO {tableName} ({nameColumn}) VALUES (@Value);
                SET @FoundId = CAST(SCOPE_IDENTITY() AS int);
            END;
            SELECT @FoundId;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@Value", value.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }
}
