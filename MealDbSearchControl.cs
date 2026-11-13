using MealWise.Data;
using MealWise.Models;
using MealWise.Services;

namespace MealWise.Forms;

/// <summary>Recipe discovery page backed by TheMealDB external API.</summary>
public sealed class MealDbSearchControl : UserControl
{
    private readonly MealDbApiService _api = new();
    private readonly RecipeImportRepository _repository;
    private readonly TextBox _searchBox = new();
    private readonly ListBox _resultsList = new();
    private readonly Label _detailTitle = new();
    private readonly Label _detailMeta = new();
    private readonly ListBox _ingredientsList = new();
    private readonly TextBox _instructionsBox = new();
    private readonly PictureBox _image = new();
    private readonly Button _searchButton = new();
    private readonly Button _saveButton = new();
    private ExternalMealDetails? _selectedDetails;
    private bool _loadingDetails;

    public MealDbSearchControl(DatabaseService database)
    {
        _repository = new RecipeImportRepository(database);
        BackColor = Color.FromArgb(245, 247, 250);
        BuildInterface();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = BackColor,
            Padding = new Padding(2)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var intro = new Label
        {
            Text = "Search TheMealDB for recipes, review their ingredients and instructions, then save them to your recipe library.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(75, 85, 99),
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(intro, 0, 0);

        var searchRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        _searchBox.Dock = DockStyle.Fill;
        _searchBox.PlaceholderText = "Try chicken, pasta, salmon...";
        _searchBox.Margin = new Padding(0, 5, 10, 5);
        _searchBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchAsync();
            }
        };
        _searchButton.Text = "Search recipes";
        _searchButton.Dock = DockStyle.Fill;
        _searchButton.FlatStyle = FlatStyle.Flat;
        _searchButton.BackColor = Color.FromArgb(37, 99, 235);
        _searchButton.ForeColor = Color.White;
        _searchButton.FlatAppearance.BorderSize = 0;
        _searchButton.Click += async (_, _) => await SearchAsync();
        searchRow.Controls.Add(_searchBox, 0, 0);
        searchRow.Controls.Add(_searchButton, 1, 0);
        root.Controls.Add(searchRow, 0, 1);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = BackColor,
            Size = new Size(1000, 600)
        };
        split.SplitterDistance = 260;
        split.Panel1MinSize = 200;
        split.Panel2MinSize = 420;
        root.Controls.Add(split, 0, 2);

        var resultsPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 12, 0) };
        resultsPanel.Controls.Add(_resultsList);
        _resultsList.Dock = DockStyle.Fill;
        _resultsList.Font = new Font("Segoe UI", 10F);
        _resultsList.IntegralHeight = false;
        _resultsList.SelectedIndexChanged += async (_, _) => await LoadSelectedDetailsAsync();
        split.Panel1.Controls.Add(resultsPanel);

        var detailLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(10, 10, 0, 0)
        };
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        detailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        split.Panel2.Controls.Add(detailLayout);

        _image.Dock = DockStyle.Fill;
        _image.SizeMode = PictureBoxSizeMode.Zoom;
        _image.BackColor = Color.White;
        _image.Margin = new Padding(0, 0, 12, 8);
        detailLayout.Controls.Add(_image, 0, 0);
        detailLayout.SetRowSpan(_image, 2);

        var titlePanel = new Panel { Dock = DockStyle.Fill };
        _detailTitle.Text = "Select a recipe";
        _detailTitle.Dock = DockStyle.Top;
        _detailTitle.Height = 30;
        _detailTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        _detailTitle.ForeColor = Color.FromArgb(31, 41, 55);
        _detailMeta.Dock = DockStyle.Top;
        _detailMeta.Height = 28;
        _detailMeta.ForeColor = Color.FromArgb(75, 85, 99);
        _saveButton.Text = "Save to recipe library";
        _saveButton.AutoSize = true;
        _saveButton.Enabled = false;
        _saveButton.FlatStyle = FlatStyle.Flat;
        _saveButton.BackColor = Color.FromArgb(22, 128, 75);
        _saveButton.ForeColor = Color.White;
        _saveButton.FlatAppearance.BorderSize = 0;
        _saveButton.Click += async (_, _) => await SaveSelectedRecipeAsync();
        titlePanel.Controls.Add(_saveButton);
        titlePanel.Controls.Add(_detailMeta);
        titlePanel.Controls.Add(_detailTitle);
        _saveButton.Location = new Point(0, 64);
        detailLayout.Controls.Add(titlePanel, 1, 0);
        detailLayout.SetRowSpan(titlePanel, 2);

        var ingredientsPanel = BuildTitledPanel("Ingredients", _ingredientsList);
        detailLayout.Controls.Add(ingredientsPanel, 0, 2);
        detailLayout.SetColumnSpan(ingredientsPanel, 2);

        var instructionsPanel = BuildTitledPanel("Cooking instructions", _instructionsBox);
        detailLayout.Controls.Add(instructionsPanel, 0, 3);
        detailLayout.SetColumnSpan(instructionsPanel, 2);
        _instructionsBox.Dock = DockStyle.Fill;
        _instructionsBox.Multiline = true;
        _instructionsBox.ReadOnly = true;
        _instructionsBox.ScrollBars = ScrollBars.Vertical;
        _instructionsBox.BorderStyle = BorderStyle.None;
        _instructionsBox.BackColor = Color.White;

        var attribution = new Label
        {
            Text = "Recipe data provided by TheMealDB • For educational use",
            Dock = DockStyle.Bottom,
            Height = 22,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleRight
        };
        Controls.Add(attribution);
        attribution.BringToFront();
    }

    private static Panel BuildTitledPanel(string title, Control content)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
        var heading = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 27,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        };
        panel.Controls.Add(content);
        panel.Controls.Add(heading);
        content.Dock = DockStyle.Fill;
        return panel;
    }

    private async Task SearchAsync()
    {
        var term = _searchBox.Text.Trim();
        if (term.Length < 2)
        {
            MessageBox.Show("Enter at least two characters to search for a recipe.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _searchButton.Enabled = false;
        _searchButton.Text = "Searching...";
        _resultsList.DataSource = null;
        ClearDetails();

        try
        {
            var meals = await _api.SearchMealsAsync(term);
            _resultsList.DataSource = meals.ToList();
            if (meals.Count == 0)
            {
                MessageBox.Show("No recipes were found. Try another search term.", "No results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Recipe search failed. Check your internet connection and try again.\n\n{ex.Message}", "API error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _searchButton.Enabled = true;
            _searchButton.Text = "Search recipes";
        }
    }

    private async Task LoadSelectedDetailsAsync()
    {
        if (_loadingDetails || _resultsList.SelectedItem is not ExternalMealSummary selected)
        {
            return;
        }

        _loadingDetails = true;
        _saveButton.Enabled = false;
        _detailTitle.Text = "Loading recipe...";
        try
        {
            _selectedDetails = await _api.GetMealDetailsAsync(selected.Id);
            _detailTitle.Text = _selectedDetails.Name;
            _detailMeta.Text = string.Join(" • ", new[] { _selectedDetails.Category, _selectedDetails.Area }.Where(x => !string.IsNullOrWhiteSpace(x)));
            _ingredientsList.Items.Clear();
            foreach (var ingredient in _selectedDetails.Ingredients)
            {
                _ingredientsList.Items.Add(string.IsNullOrWhiteSpace(ingredient.MeasureText)
                    ? ingredient.Name
                    : $"{ingredient.MeasureText}  {ingredient.Name}");
            }
            _instructionsBox.Text = _selectedDetails.Instructions ?? "No instructions were supplied.";
            _image.ImageLocation = _selectedDetails.ImageUrl;
            _saveButton.Enabled = true;
        }
        catch (Exception ex)
        {
            _selectedDetails = null;
            _detailTitle.Text = "Could not load recipe";
            MessageBox.Show(ex.Message, "Recipe details", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loadingDetails = false;
        }
    }

    private async Task SaveSelectedRecipeAsync()
    {
        if (_selectedDetails is null)
        {
            return;
        }

        _saveButton.Enabled = false;
        _saveButton.Text = "Saving...";
        try
        {
            var recipeId = await _repository.SaveOrUpdateAsync(_selectedDetails);
            MessageBox.Show($"'{_selectedDetails.Name}' was saved to your recipe library (Recipe ID {recipeId}).", "Recipe saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The recipe could not be saved. Confirm that the database scripts have been run and the connection is configured.\n\n{ex.Message}", "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _saveButton.Enabled = _selectedDetails is not null;
            _saveButton.Text = "Save to recipe library";
        }
    }

    private void ClearDetails()
    {
        _selectedDetails = null;
        _detailTitle.Text = "Select a recipe";
        _detailMeta.Text = string.Empty;
        _ingredientsList.Items.Clear();
        _instructionsBox.Clear();
        _image.ImageLocation = null;
        _saveButton.Enabled = false;
    }
}
