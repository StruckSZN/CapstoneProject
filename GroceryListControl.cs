using System.Data;
using Microsoft.Data.SqlClient;
using MealWise.Data;

namespace MealWise.Forms;

public sealed class GroceryListControl : UserControl
{
    private readonly DatabaseService _database;
    private readonly ComboBox _plans = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly DataGridView _grid = new();
    private int? _listId;
    private bool _loading;

    public GroceryListControl(DatabaseService database)
    {
        _database = database; BackColor = Color.FromArgb(245, 247, 250);
        BuildUi();
        Load += async (_, _) => await LoadPlansAsync();
    }

    private void BuildUi()
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, WrapContents = false, Padding = new Padding(0, 5, 0, 5) };
        top.Controls.Add(new Label { Text = "Meal plan", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(_plans);
        top.Controls.Add(ModuleUi.Button("Generate / update items", async () => await GenerateAsync()));
        top.Controls.Add(ModuleUi.Button("Add item", async () => await AddItemAsync()));\n        top.Controls.Add(ModuleUi.Button("Edit selected", async () => await EditItemAsync()));
        top.Controls.Add(ModuleUi.Button("Delete selected", async () => await DeleteItemAsync()));
        _plans.SelectedIndexChanged += async (_, _) => await LoadListAsync();
        Controls.Add(top);

        _grid.Dock = DockStyle.Fill; _grid.AllowUserToAddRows = false; _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; _grid.BackgroundColor = Color.White; _grid.RowHeadersVisible = false;
        _grid.CellValueChanged += async (_, e) =>
        {
            if (!_loading && e.RowIndex >= 0 && _grid.Columns[e.ColumnIndex].Name == "IsPurchased")
                await SavePurchasedAsync(e.RowIndex);
        };
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty && _grid.CurrentCell is DataGridViewCheckBoxCell) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        Controls.Add(_grid);
    }

    private async Task LoadPlansAsync()
    {
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("""
                SELECT p.MealPlanId, p.PlanName
                FROM dbo.MealPlans p
                ORDER BY p.WeekStartDate DESC, p.PlanName;
                """, connection);
            var options = new List<PlannerOption>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) options.Add(new PlannerOption(reader.GetInt32(0), reader.GetString(1)));
            _plans.DataSource = options; _plans.DisplayMember = nameof(PlannerOption.Name); _plans.ValueMember = nameof(PlannerOption.Id);
            if (options.Count == 0) MessageBox.Show("Create a weekly meal plan first.", "No meal plans", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not load plans", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task EnsureListAsync(SqlConnection connection, SqlTransaction? transaction, int planId)
    {
        await using var cmd = new SqlCommand("""
            IF NOT EXISTS (SELECT 1 FROM dbo.GroceryLists WHERE MealPlanId=@PlanId)
                INSERT dbo.GroceryLists (MealPlanId, ListName) VALUES (@PlanId, N'Groceries');
            SELECT GroceryListId FROM dbo.GroceryLists WHERE MealPlanId=@PlanId;
            """, connection, transaction);
        cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
        _listId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private async Task LoadListAsync()
    {
        _listId = null;
        if (_plans.SelectedValue is not int planId) { _grid.DataSource = null; return; }
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await EnsureListAsync(connection, null, planId);
            await using var cmd = new SqlCommand("""
                SELECT i.GroceryListItemId, i.IsPurchased, ing.IngredientName AS Ingredient,
                       CASE WHEN i.Quantity IS NULL THEN '' ELSE CONVERT(nvarchar(30), i.Quantity) END AS Quantity,
                       COALESCE(mu.Abbreviation,'') AS Unit, COALESCE(i.Notes,'') AS Notes
                FROM dbo.GroceryListItems i
                JOIN dbo.Ingredients ing ON ing.IngredientId=i.IngredientId
                LEFT JOIN dbo.MeasurementUnits mu ON mu.UnitId=i.UnitId
                WHERE i.GroceryListId=@ListId
                ORDER BY i.IsPurchased, ing.IngredientName;
                """, connection);
            cmd.Parameters.Add("@ListId", SqlDbType.Int).Value = _listId.Value;
            var table = new DataTable(); await using var reader = await cmd.ExecuteReaderAsync(); table.Load(reader);
            _loading = true; _grid.DataSource = table; _loading = false;
            if (_grid.Columns["GroceryListItemId"] is not null) _grid.Columns["GroceryListItemId"].Visible = false;
            if (_grid.Columns["IsPurchased"] is not null) _grid.Columns["IsPurchased"].ReadOnly = false;
            foreach (DataGridViewColumn col in _grid.Columns)
                if (col.Name != "IsPurchased") col.ReadOnly = true;
        }
        catch (Exception ex) { _loading = false; MessageBox.Show(ex.Message, "Could not load grocery list", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task GenerateAsync()
    {
        if (_plans.SelectedValue is not int planId) return;
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            await EnsureListAsync(connection, transaction, planId);
            const string numericSql = """
                ;WITH Totals AS
                (
                    SELECT ri.IngredientId, ri.UnitId,
                           SUM(ri.Quantity * CAST(e.Servings AS decimal(10,2)) / NULLIF(r.DefaultServings,0)) AS TotalQuantity
                    FROM dbo.MealPlanEntries e
                    JOIN dbo.Recipes r ON r.RecipeId=e.RecipeId
                    JOIN dbo.RecipeIngredients ri ON ri.RecipeId=r.RecipeId
                    WHERE e.MealPlanId=@PlanId AND ri.Quantity IS NOT NULL AND ri.UnitId IS NOT NULL
                    GROUP BY ri.IngredientId, ri.UnitId
                )
                MERGE dbo.GroceryListItems AS target
                USING Totals AS source
                ON target.GroceryListId=@ListId AND target.IngredientId=source.IngredientId
                   AND target.UnitId=source.UnitId AND target.Notes=N'Generated from meal plan'
                WHEN MATCHED THEN UPDATE SET Quantity=source.TotalQuantity
                WHEN NOT MATCHED THEN INSERT (GroceryListId, IngredientId, Quantity, UnitId, Notes)
                    VALUES (@ListId, source.IngredientId, source.TotalQuantity, source.UnitId, N'Generated from meal plan');
                """;
            await using (var cmd = new SqlCommand(numericSql, connection, transaction))
            {
                cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
                cmd.Parameters.Add("@ListId", SqlDbType.Int).Value = _listId!.Value;
                await cmd.ExecuteNonQueryAsync();
            }

            const string textSql = """
                INSERT dbo.GroceryListItems (GroceryListId, IngredientId, Quantity, UnitId, Notes)
                SELECT @ListId, ri.IngredientId, NULL, NULL,
                       LEFT(CONCAT(N'From ', r.RecipeName, N': ', ri.MeasureText), 200)
                FROM dbo.MealPlanEntries e
                JOIN dbo.Recipes r ON r.RecipeId=e.RecipeId
                JOIN dbo.RecipeIngredients ri ON ri.RecipeId=r.RecipeId
                WHERE e.MealPlanId=@PlanId AND ri.MeasureText IS NOT NULL AND LTRIM(RTRIM(ri.MeasureText)) <> N''
                  AND NOT EXISTS
                  (
                    SELECT 1 FROM dbo.GroceryListItems existing
                    WHERE existing.GroceryListId=@ListId AND existing.IngredientId=ri.IngredientId
                      AND existing.Notes=LEFT(CONCAT(N'From ', r.RecipeName, N': ', ri.MeasureText), 200)
                  );
                """;
            await using (var cmd = new SqlCommand(textSql, connection, transaction))
            {
                cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
                cmd.Parameters.Add("@ListId", SqlDbType.Int).Value = _listId!.Value;
                await cmd.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            MessageBox.Show("Grocery items generated. Numeric quantities were combined only when ingredient and unit match; provider text measures remain separate.", "Grocery list updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadListAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not generate grocery items", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task AddItemAsync()
    {
        if (_listId is null) { MessageBox.Show("Select a meal plan first."); return; }
        var name = ModuleUi.Prompt("Ingredient name:", "Add grocery item");
        if (string.IsNullOrWhiteSpace(name)) return;
        var notes = ModuleUi.Prompt("Optional notes or measurement:", "Add grocery item");
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
            await using var ingredientCmd = new SqlCommand("""
                DECLARE @Id int;
                SELECT @Id=IngredientId FROM dbo.Ingredients WHERE IngredientName=@Name;
                IF @Id IS NULL BEGIN INSERT dbo.Ingredients (IngredientName) VALUES (@Name); SET @Id=CAST(SCOPE_IDENTITY() AS int); END;
                SELECT @Id;
                """, connection, transaction);
            ingredientCmd.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = name.Trim();
            var ingredientId = Convert.ToInt32(await ingredientCmd.ExecuteScalarAsync());
            await using var itemCmd = new SqlCommand("INSERT dbo.GroceryListItems (GroceryListId,IngredientId,Notes) VALUES (@ListId,@IngredientId,@Notes);", connection, transaction);
            itemCmd.Parameters.Add("@ListId", SqlDbType.Int).Value = _listId.Value;
            itemCmd.Parameters.Add("@IngredientId", SqlDbType.Int).Value = ingredientId;
            itemCmd.Parameters.Add("@Notes", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes.Trim();
            await itemCmd.ExecuteNonQueryAsync(); await transaction.CommitAsync(); await LoadListAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not add item", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task DeleteItemAsync()
    {
        if (_grid.CurrentRow?.Cells["GroceryListItemId"].Value is not object value) return;
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("DELETE dbo.GroceryListItems WHERE GroceryListItemId=@Id;", connection);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Convert.ToInt32(value);
            await cmd.ExecuteNonQueryAsync(); await LoadListAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not delete item", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task SavePurchasedAsync(int rowIndex)
    {
        if (_grid.Rows[rowIndex].Cells["GroceryListItemId"].Value is not object idValue) return;
        var purchased = Convert.ToBoolean(_grid.Rows[rowIndex].Cells["IsPurchased"].Value);
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("UPDATE dbo.GroceryListItems SET IsPurchased=@Purchased WHERE GroceryListItemId=@Id;", connection);
            cmd.Parameters.Add("@Purchased", SqlDbType.Bit).Value = purchased;
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Convert.ToInt32(idValue);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not update item", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

internal static class ModuleUi
{
    public static Button Button(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 34, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, Cursor = Cursors.Hand };
        button.FlatAppearance.BorderSize = 0;
        button.Click += async (_, _) => { button.Enabled = false; try { await action(); } finally { button.Enabled = true; } };
        return button;
    }

    public static string? Prompt(string label, string title, string defaultValue = "")
    {
        using var form = new Form { Text = title, Width = 440, Height = 170, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false };
        var text = new TextBox { Left = 16, Top = 18, Width = 390, Text = defaultValue };
        var caption = new Label { Left = 16, Top = 0, Width = 390, Text = label };
        var ok = new Button { Text = "OK", Left = 230, Top = 58, Width = 80, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", Left = 320, Top = 58, Width = 80, DialogResult = DialogResult.Cancel };
        form.Controls.AddRange([caption, text, ok, cancel]); form.AcceptButton = ok; form.CancelButton = cancel;
        return form.ShowDialog() == DialogResult.OK ? text.Text.Trim() : null;
    }
}
