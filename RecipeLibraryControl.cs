using System.Data;
using Microsoft.Data.SqlClient;
using MealWise.Data;

namespace MealWise.Forms;

public sealed class RecipeLibraryControl : UserControl
{
    private readonly DatabaseService _database;
    private readonly DataGridView _grid = new();
    private readonly TextBox _search = new();
    private readonly Label _status = new();

    public RecipeLibraryControl(DatabaseService database)
    {
        _database = database;
        BackColor = Color.FromArgb(245, 247, 250);
        BuildUi();
        Load += async (_, _) => await RefreshRecipesAsync();
    }

    private void BuildUi()
    {
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, WrapContents = false, Padding = new Padding(0, 5, 0, 5) };
        _search.Width = 280;
        _search.PlaceholderText = "Search recipe name...";
        _search.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await RefreshRecipesAsync(); } };
        top.Controls.Add(_search);
        top.Controls.Add(ModuleUi.Button("Search", async () => await RefreshRecipesAsync()));
        top.Controls.Add(ModuleUi.Button("Add recipe", async () => await EditRecipeAsync(null)));
        top.Controls.Add(ModuleUi.Button("Edit selected", async () => await EditRecipeAsync(SelectedRecipeId())));
        top.Controls.Add(ModuleUi.Button("Delete selected", async () => await DeleteRecipeAsync()));
        Controls.Add(top);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.RowHeadersVisible = false;
        Controls.Add(_grid);

        _status.Dock = DockStyle.Bottom;
        _status.Height = 28;
        _status.ForeColor = Color.FromArgb(75, 85, 99);
        Controls.Add(_status);
    }

    private int? SelectedRecipeId()
    {
        if (_grid.CurrentRow?.Cells["RecipeId"].Value is null) return null;
        return Convert.ToInt32(_grid.CurrentRow.Cells["RecipeId"].Value);
    }

    private async Task RefreshRecipesAsync()
    {
        try
        {
            await using var connection = _database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("""
                SELECT r.RecipeId, r.RecipeName, COALESCE(c.CategoryName, '') AS Category,
                       COALESCE(u.CuisineName, '') AS Cuisine, r.DefaultServings,
                       COALESCE(r.PrepMinutes, 0) AS PrepMinutes, COALESCE(r.CookMinutes, 0) AS CookMinutes,
                       r.Instructions, r.SourceName
                FROM dbo.Recipes r
                LEFT JOIN dbo.RecipeCategories c ON c.CategoryId = r.CategoryId
                LEFT JOIN dbo.Cuisines u ON u.CuisineId = r.CuisineId
                WHERE (@Search = N'' OR r.RecipeName LIKE N'%' + @Search + N'%')
                ORDER BY r.RecipeName;
                """, connection);
            command.Parameters.Add("@Search", SqlDbType.NVarChar, 160).Value = _search.Text.Trim();
            var table = new DataTable();
            await using var reader = await command.ExecuteReaderAsync();
            table.Load(reader);
            _grid.DataSource = table;
            if (_grid.Columns["RecipeId"] is not null) _grid.Columns["RecipeId"].Visible = false;
            if (_grid.Columns["Instructions"] is not null) _grid.Columns["Instructions"].Visible = false;
            _status.Text = $"{table.Rows.Count} recipe(s) found. Imported API recipes and locally created recipes appear here.";
        }
        catch (Exception ex) { ShowError("Could not load recipes", ex); }
    }

    private async Task EditRecipeAsync(int? recipeId)
    {
        RecipeEditDialog dialog;
        if (recipeId is null) dialog = new RecipeEditDialog();
        else
        {
            var row = _grid.CurrentRow;
            if (row is null) { MessageBox.Show("Select a recipe first."); return; }
            dialog = new RecipeEditDialog(
                Convert.ToString(row.Cells["RecipeName"].Value) ?? "",
                Convert.ToString(row.Cells["Instructions"].Value) ?? "",
                Convert.ToInt32(row.Cells["DefaultServings"].Value));
        }

        using (dialog)
        {
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                await using var connection = _database.CreateConnection();
                await connection.OpenAsync();
                if (recipeId is null)
                {
                    await using var command = new SqlCommand("""
                        INSERT dbo.Recipes (RecipeName, Instructions, DefaultServings)
                        VALUES (@Name, @Instructions, @Servings);
                        """, connection);
                    AddRecipeParameters(command, dialog);
                    await command.ExecuteNonQueryAsync();
                }
                else
                {
                    await using var command = new SqlCommand("""
                        UPDATE dbo.Recipes SET RecipeName=@Name, Instructions=@Instructions,
                            DefaultServings=@Servings, UpdatedAt=SYSUTCDATETIME()
                        WHERE RecipeId=@Id;
                        """, connection);
                    AddRecipeParameters(command, dialog);
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = recipeId.Value;
                    await command.ExecuteNonQueryAsync();
                }
                await RefreshRecipesAsync();
            }
            catch (Exception ex) { ShowError("Could not save recipe", ex); }
        }
    }

    private static void AddRecipeParameters(SqlCommand command, RecipeEditDialog dialog)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 160).Value = dialog.RecipeName;
        command.Parameters.Add("@Instructions", SqlDbType.NVarChar).Value =
            string.IsNullOrWhiteSpace(dialog.Instructions) ? DBNull.Value : dialog.Instructions;
        command.Parameters.Add("@Servings", SqlDbType.TinyInt).Value = dialog.Servings;
    }

    private async Task DeleteRecipeAsync()
    {
        var id = SelectedRecipeId();
        if (id is null) { MessageBox.Show("Select a recipe first."); return; }
        if (MessageBox.Show("Delete this recipe? Recipes used by a meal plan cannot be deleted until those plan entries are removed.",
            "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            await using var connection = _database.CreateConnection();
            await connection.OpenAsync();
            await using var command = new SqlCommand("DELETE dbo.Recipes WHERE RecipeId=@Id;", connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id.Value;
            await command.ExecuteNonQueryAsync();
            await RefreshRecipesAsync();
        }
        catch (SqlException ex) when (ex.Number == 547)
        {
            MessageBox.Show("This recipe is used by a meal plan. Remove its planned entries before deleting it.", "Recipe in use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("Could not delete recipe", ex); }
    }

    private static void ShowError(string title, Exception ex) =>
        MessageBox.Show($"{ex.Message}\n\nConfirm the database is configured and scripts 00–02 have been run.", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
}

internal sealed class RecipeEditDialog : Form
{
    private readonly TextBox _name = new() { Dock = DockStyle.Fill };
    private readonly TextBox _instructions = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly NumericUpDown _servings = new() { Minimum = 1, Maximum = 255, Value = 4, Width = 90 };

    public string RecipeName => _name.Text.Trim();
    public string Instructions => _instructions.Text.Trim();
    public int Servings => (int)_servings.Value;

    public RecipeEditDialog(string name = "", string instructions = "", int servings = 4)
    {
        Text = string.IsNullOrWhiteSpace(name) ? "Add recipe" : "Edit recipe";
        Width = 560; Height = 430; StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog; MinimizeBox = false; MaximizeBox = false;
        _name.Text = name; _instructions.Text = instructions; _servings.Value = Math.Clamp(servings, 1, 255);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 2, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.Controls.Add(new Label { Text = "Recipe name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        layout.Controls.Add(_name, 1, 0);
        layout.Controls.Add(new Label { Text = "Servings", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_servings, 1, 1);
        layout.Controls.Add(new Label { Text = "Instructions", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 2);
        layout.Controls.Add(_instructions, 1, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var save = new Button { Text = "Save", DialogResult = DialogResult.OK, Width = 90 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
        buttons.Controls.Add(save); buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 0, 3); layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout); AcceptButton = save; CancelButton = cancel;
        save.Click += (_, e) => { if (string.IsNullOrWhiteSpace(_name.Text)) { e = null!; MessageBox.Show("Recipe name is required."); DialogResult = DialogResult.None; } };
    }
}
