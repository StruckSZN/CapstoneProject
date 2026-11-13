using MealWise.Data;

namespace MealWise.Forms;

public sealed class DashboardControl : UserControl
{
    private readonly DatabaseService _database;
    private readonly Label _connectionStatus;
    private readonly Button _testButton;

    public DashboardControl(DatabaseService database)
    {
        _database = database;
        BackColor = Color.FromArgb(245, 247, 250);

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4)
        };
        Controls.Add(layout);

        var welcome = new Label
        {
            Text = "Plan your week. Make mealtime easier.",
            AutoSize = true,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(welcome);

        var description = new Label
        {
            Text = "MealWise brings recipes, weekly planning, and grocery preparation together in one place. Search the recipe API, save meals to your library, and organize your week.",
            AutoSize = true,
            MaximumSize = new Size(880, 0),
            ForeColor = Color.FromArgb(75, 85, 99),
            Margin = new Padding(0, 0, 0, 24)
        };
        layout.Controls.Add(description);

        var databaseCard = new Panel
        {
            Width = 850,
            Height = 190,
            BackColor = Color.White,
            Padding = new Padding(22),
            Margin = new Padding(0, 0, 0, 16)
        };
        layout.Controls.Add(databaseCard);

        databaseCard.Controls.Add(new Label
        {
            Text = "Database connection",
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Location = new Point(22, 20)
        });

        _connectionStatus = new Label
        {
            Text = "Not tested",
            AutoSize = true,
            ForeColor = Color.FromArgb(75, 85, 99),
            Location = new Point(22, 62)
        };
        databaseCard.Controls.Add(_connectionStatus);

        _testButton = new Button
        {
            Text = "Test database connection",
            AutoSize = true,
            Height = 38,
            Location = new Point(22, 105),
            BackColor = Color.FromArgb(22, 128, 75),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _testButton.FlatAppearance.BorderSize = 0;
        _testButton.Click += TestDatabaseConnectionAsync;
        databaseCard.Controls.Add(_testButton);

        var featureCard = new Panel { Width = 850, Height = 130, BackColor = Color.White, Padding = new Padding(22) };
        featureCard.Controls.Add(new Label
        {
            Text = "Current project modules",
            AutoSize = true,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Location = new Point(22, 18)
        });
        featureCard.Controls.Add(new Label
        {
            Text = "Recipe search and import: connected to TheMealDB API\nRecipe library, weekly planner, and grocery list: modules under development",
            AutoSize = true,
            Location = new Point(22, 52),
            ForeColor = Color.FromArgb(75, 85, 99)
        });
        layout.Controls.Add(featureCard);
    }

    private async void TestDatabaseConnectionAsync(object? sender, EventArgs e)
    {
        _testButton.Enabled = false;
        _connectionStatus.ForeColor = Color.FromArgb(75, 85, 99);
        _connectionStatus.Text = "Connecting...";

        try
        {
            var databaseName = await _database.TestConnectionAsync();
            _connectionStatus.ForeColor = Color.FromArgb(21, 128, 61);
            _connectionStatus.Text = $"Connected successfully. Database: {databaseName}";
        }
        catch (Exception ex)
        {
            _connectionStatus.ForeColor = Color.FromArgb(185, 28, 28);
            _connectionStatus.Text = $"Connection failed: {ex.Message}";
        }
        finally
        {
            _testButton.Enabled = true;
        }
    }
}
