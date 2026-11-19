/*
    MealWise database smoke test
    Run after 00_CreateDatabase.sql, 01_CreateTables.sql, and 02_InsertSampleData.sql.
    This script performs a disposable CRUD/relationship test inside one transaction
    and rolls back every inserted row. It does not alter existing application data.
*/
USE MealPlannerDb;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Token nvarchar(36) = CONVERT(nvarchar(36), NEWID());
    DECLARE @Email nvarchar(254) = N'smoke-' + @Token + N'@mealwise.local';
    DECLARE @UserId int;
    DECLARE @RecipeId int;
    DECLARE @IngredientId int;
    DECLARE @MealTypeId int;
    DECLARE @UnitId int;
    DECLARE @MealPlanId int;
    DECLARE @EntryId int;
    DECLARE @GroceryListId int;
    DECLARE @GroceryItemId int;
    DECLARE @WeekStart date = DATEADD(day, -(DATEDIFF(day, CONVERT(date, '19000101'), CONVERT(date, GETDATE())) % 7), CONVERT(date, GETDATE()));

    SELECT TOP (1) @MealTypeId = MealTypeId
    FROM dbo.MealTypes
    WHERE TypeName = N'Dinner';

    SELECT TOP (1) @UnitId = UnitId
    FROM dbo.MeasurementUnits
    WHERE Abbreviation = N'pc';

    IF @MealTypeId IS NULL OR @UnitId IS NULL
        THROW 51100, 'Smoke test requires seeded Dinner meal type and pc measurement unit. Run 02_InsertSampleData.sql.', 1;

    INSERT dbo.AppUsers (DisplayName, Email)
    VALUES (N'Smoke Test User', @Email);
    SET @UserId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.Ingredients (IngredientName)
    VALUES (N'Smoke Test Ingredient ' + @Token);
    SET @IngredientId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.Recipes (RecipeName, DefaultServings)
    VALUES (N'Smoke Test Recipe ' + @Token, 2);
    SET @RecipeId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.RecipeIngredients (RecipeId, IngredientId, Quantity, UnitId)
    VALUES (@RecipeId, @IngredientId, 2.00, @UnitId);

    INSERT dbo.MealPlans (UserId, PlanName, WeekStartDate)
    VALUES (@UserId, N'Smoke Test Plan', @WeekStart);
    SET @MealPlanId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.MealPlanEntries (MealPlanId, RecipeId, MealTypeId, PlannedDate, Servings)
    VALUES (@MealPlanId, @RecipeId, @MealTypeId, @WeekStart, 2);
    SET @EntryId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.GroceryLists (MealPlanId, ListName)
    VALUES (@MealPlanId, N'Smoke Test Grocery List');
    SET @GroceryListId = CONVERT(int, SCOPE_IDENTITY());

    INSERT dbo.GroceryListItems (GroceryListId, IngredientId, Quantity, UnitId)
    VALUES (@GroceryListId, @IngredientId, 2.00, @UnitId);
    SET @GroceryItemId = CONVERT(int, SCOPE_IDENTITY());

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.MealPlanEntries e
        JOIN dbo.MealPlans p ON p.MealPlanId = e.MealPlanId
        JOIN dbo.Recipes r ON r.RecipeId = e.RecipeId
        JOIN dbo.MealTypes mt ON mt.MealTypeId = e.MealTypeId
        WHERE e.MealPlanEntryId = @EntryId
          AND p.UserId = @UserId
          AND r.RecipeId = @RecipeId
          AND mt.MealTypeId = @MealTypeId
    )
        THROW 51101, 'Smoke test failed: meal-plan relationships could not be read back.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.GroceryListItems gi
        JOIN dbo.GroceryLists gl ON gl.GroceryListId = gi.GroceryListId
        JOIN dbo.Ingredients i ON i.IngredientId = gi.IngredientId
        WHERE gi.GroceryListItemId = @GroceryItemId
          AND gl.MealPlanId = @MealPlanId
          AND i.IngredientId = @IngredientId
    )
        THROW 51102, 'Smoke test failed: grocery-list relationships could not be read back.', 1;

    UPDATE dbo.GroceryListItems
    SET IsPurchased = 1
    WHERE GroceryListItemId = @GroceryItemId;

    IF NOT EXISTS (SELECT 1 FROM dbo.GroceryListItems WHERE GroceryListItemId = @GroceryItemId AND IsPurchased = 1)
        THROW 51103, 'Smoke test failed: grocery item update did not persist.', 1;

    DELETE dbo.MealPlans WHERE MealPlanId = @MealPlanId;

    IF EXISTS (SELECT 1 FROM dbo.MealPlanEntries WHERE MealPlanId = @MealPlanId)
       OR EXISTS (SELECT 1 FROM dbo.GroceryLists WHERE MealPlanId = @MealPlanId)
       OR EXISTS (SELECT 1 FROM dbo.GroceryListItems WHERE GroceryListId = @GroceryListId)
        THROW 51104, 'Smoke test failed: deleting a plan did not cascade to its dependent entries and grocery list.', 1;

    ROLLBACK TRANSACTION;

    SELECT N'PASS' AS SmokeTestStatus,
           N'Create/read/update/delete and cascade relationships succeeded; all test data was rolled back.' AS Details;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
