using System.Data;
using Microsoft.Data.SqlClient;
using MealWise.Data;

namespace MealWise.Forms;

public sealed class WeeklyMealPlannerControl : UserControl
{
    private readonly DatabaseService _database;
    private readonly ComboBox _plans = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
    private readonly ComboBox _recipes = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _mealTypes = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
    private readonly DateTimePicker _date = new() { Width = 130, Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown _servings = new() { Minimum = 1, Maximum = 255, Value = 2, Width = 65 };
    private readonly DataGridView _grid = new();

    public WeeklyMealPlannerControl(DatabaseService database)
    {
        _database = database; BackColor = Color.FromArgb(245, 247, 250);
        BuildUi();
        Load += async (_, _) => { await LoadLookupsAsync(); await LoadPlansAsync(); };
    }

    private void BuildUi()
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, Padding = new Padding(0, 4, 0, 4) };
        top.Controls.Add(new Label { Text = "Meal plan", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(_plans);
        top.Controls.Add(ModuleUi.Button("New plan", async () => await CreatePlanAsync()));
        top.Controls.Add(ModuleUi.Button("Rename plan", async () => await RenamePlanAsync()));
        top.Controls.Add(ModuleUi.Button("Delete plan", async () => await DeletePlanAsync()));
        _plans.SelectedIndexChanged += async (_, _) => await LoadEntriesAsync();
        Controls.Add(top);

        var add = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, Padding = new Padding(0, 4, 0, 4) };
        add.Controls.Add(new Label { Text = "Recipe", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        add.Controls.Add(_recipes);
        add.Controls.Add(_date);
        add.Controls.Add(_mealTypes);
        add.Controls.Add(new Label { Text = "Servings", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        add.Controls.Add(_servings);
        add.Controls.Add(ModuleUi.Button("Add meal", async () => await AddEntryAsync()));
        add.Controls.Add(ModuleUi.Button("Update selected", async () => await UpdateEntryAsync()));
        add.Controls.Add(ModuleUi.Button("Remove selected", async () => await DeleteEntryAsync()));
        Controls.Add(add);

        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White; _grid.RowHeadersVisible = false;
        Controls.Add(_grid);
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            var recipes = new List<PlannerOption>();
            await using (var cmd = new SqlCommand("SELECT RecipeId, RecipeName FROM dbo.Recipes ORDER BY RecipeName;", connection))
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync()) recipes.Add(new PlannerOption(reader.GetInt32(0), reader.GetString(1)));
            _recipes.DataSource = recipes; _recipes.DisplayMember = nameof(PlannerOption.Name); _recipes.ValueMember = nameof(PlannerOption.Id);
            var meals = new List<PlannerOption>();
            await using (var cmd = new SqlCommand("SELECT MealTypeId, TypeName FROM dbo.MealTypes ORDER BY SortOrder;", connection))
            await using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync()) meals.Add(new PlannerOption(reader.GetInt32(0), reader.GetString(1)));
            _mealTypes.DataSource = meals; _mealTypes.DisplayMember = nameof(PlannerOption.Name); _mealTypes.ValueMember = nameof(PlannerOption.Id);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not load recipes and meal types", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task<int?> GetUserIdAsync(SqlConnection connection)
    {
        await using var cmd = new SqlCommand("SELECT TOP (1) UserId FROM dbo.AppUsers ORDER BY CASE WHEN Email=N'demo@mealwise.local' THEN 0 ELSE 1 END, UserId;", connection);
        var value = await cmd.ExecuteScalarAsync();
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    private async Task LoadPlansAsync()
    {
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            var userId = await GetUserIdAsync(connection);
            if (userId is null) { MessageBox.Show("No application user exists. Run 02_InsertSampleData.sql first."); return; }
            var plans = new List<PlannerOption>();
            await using var cmd = new SqlCommand("SELECT MealPlanId, PlanName FROM dbo.MealPlans WHERE UserId=@UserId ORDER BY WeekStartDate DESC;", connection);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) plans.Add(new PlannerOption(reader.GetInt32(0), reader.GetString(1)));
            _plans.DataSource = plans; _plans.DisplayMember = nameof(PlannerOption.Name); _plans.ValueMember = nameof(PlannerOption.Id);
            if (plans.Count == 0) _grid.DataSource = null;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not load meal plans", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task CreatePlanAsync()
    {
        var name = ModuleUi.Prompt("New meal plan name:", "Create meal plan", $"Week of {DateTime.Today:MMM d}");
        if (string.IsNullOrWhiteSpace(name)) return;
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            var userId = await GetUserIdAsync(connection);
            if (userId is null) { MessageBox.Show("No application user exists. Run 02_InsertSampleData.sql first."); return; }
            await using var cmd = new SqlCommand("INSERT dbo.MealPlans (UserId, PlanName, WeekStartDate) VALUES (@UserId,@Name,@WeekStart); SELECT CAST(SCOPE_IDENTITY() AS int);", connection);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId.Value;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name.Trim();
            cmd.Parameters.Add("@WeekStart", SqlDbType.Date).Value = _date.Value.Date.AddDays(-((int)_date.Value.DayOfWeek + 6) % 7);
            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await LoadPlansAsync(); _plans.SelectedValue = id;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not create plan", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task DeletePlanAsync()
    {
        if (_plans.SelectedValue is not int id) return;
        if (MessageBox.Show("Delete this plan and its grocery list?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("DELETE dbo.MealPlans WHERE MealPlanId=@Id;", connection);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id; await cmd.ExecuteNonQueryAsync();
            await LoadPlansAsync(); await LoadEntriesAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not delete plan", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task LoadEntriesAsync()
    {
        if (_plans.SelectedValue is not int id) { _grid.DataSource = null; return; }
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("""
                SELECT e.MealPlanEntryId, e.PlannedDate, mt.TypeName AS Meal, r.RecipeName AS Recipe,
                       e.Servings, COALESCE(e.Notes,'') AS Notes
                FROM dbo.MealPlanEntries e
                JOIN dbo.MealTypes mt ON mt.MealTypeId=e.MealTypeId
                JOIN dbo.Recipes r ON r.RecipeId=e.RecipeId
                WHERE e.MealPlanId=@PlanId ORDER BY e.PlannedDate, mt.SortOrder;
                """, connection);
            cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = id;
            var table = new DataTable(); await using var reader = await cmd.ExecuteReaderAsync(); table.Load(reader);
            _grid.DataSource = table;
            if (_grid.Columns["MealPlanEntryId"] is not null) _grid.Columns["MealPlanEntryId"].Visible = false;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not load plan entries", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task AddEntryAsync()
    {
        if (_plans.SelectedValue is not int planId || _recipes.SelectedValue is not int recipeId || _mealTypes.SelectedValue is not int mealId)
        { MessageBox.Show("Create/select a plan and ensure recipes and meal types exist."); return; }
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("""
                INSERT dbo.MealPlanEntries (MealPlanId, RecipeId, MealTypeId, PlannedDate, Servings)
                VALUES (@PlanId,@RecipeId,@MealId,@Date,@Servings);
                """, connection);
            cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = planId;
            cmd.Parameters.Add("@RecipeId", SqlDbType.Int).Value = recipeId;
            cmd.Parameters.Add("@MealId", SqlDbType.Int).Value = mealId;
            cmd.Parameters.Add("@Date", SqlDbType.Date).Value = _date.Value.Date;
            cmd.Parameters.Add("@Servings", SqlDbType.TinyInt).Value = (int)_servings.Value;
            await cmd.ExecuteNonQueryAsync(); await LoadEntriesAsync();
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        { MessageBox.Show("That plan already has a meal in this date and meal-type slot. Remove or change the existing entry first.", "Slot already used", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not add meal", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task DeleteEntryAsync()
    {
        if (_grid.CurrentRow?.Cells["MealPlanEntryId"].Value is not object value) return;
        try
        {
            await using var connection = _database.CreateConnection(); await connection.OpenAsync();
            await using var cmd = new SqlCommand("DELETE dbo.MealPlanEntries WHERE MealPlanEntryId=@Id;", connection);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = Convert.ToInt32(value);
            await cmd.ExecuteNonQueryAsync(); await LoadEntriesAsync();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Could not remove meal", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

internal sealed record PlannerOption(int Id, string Name)
{
    public override string ToString() => Name;
}
