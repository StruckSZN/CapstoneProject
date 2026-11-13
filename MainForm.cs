using MealWise.Data;

namespace MealWise.Forms;

public sealed class MainForm : Form
{
    private readonly DatabaseService _database;
    private readonly Panel _contentPanel;
    private readonly Label _pageTitle;

    public MainForm(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));

        Text = "MealWise | Meal Planner";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1040, 680);
        Size = new Size(1320, 850);
        Font = new Font("Segoe UI", 10F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        root.Controls.Add(BuildSidebar(), 0, 0);

        var mainArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(26) };
        root.Controls.Add(mainArea, 1, 0);

        _pageTitle = new Label
        {
            Text = "Dashboard",
            Dock = DockStyle.Top,
            Height = 48,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            TextAlign = ContentAlignment.MiddleLeft
        };
        mainArea.Controls.Add(_pageTitle);

        _contentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 18, 0, 0) };
        mainArea.Controls.Add(_contentPanel);
        _contentPanel.BringToFront();
        _pageTitle.BringToFront();

        ShowPage("Dashboard", new DashboardControl(_database));
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 55, 47),
            Padding = new Padding(16)
        };

        var brand = new Label
        {
            Text = "MEALWISE",
            Dock = DockStyle.Top,
            Height = 68,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(brand);

        var nav = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            BackColor = Color.Transparent
        };
        sidebar.Controls.Add(nav);

        AddNavButton(nav, "Dashboard", () => ShowPage("Dashboard", new DashboardControl(_database)));
        AddNavButton(nav, "Find Recipes (API)", () => ShowPage("Find Recipes", new MealDbSearchControl(_database)));
        AddNavButton(nav, "Recipe Library", () => ShowPage("Recipe Library", new PlaceholderControl(
            "Recipe Library",
            "Classmate module: browse, search, sort, edit, and delete recipes saved from the recipe API.")));
        AddNavButton(nav, "Weekly Meal Planner", () => ShowPage("Weekly Meal Planner", new PlaceholderControl(
            "Weekly Meal Planner",
            "Classmate module: create meal plans and assign recipes to dates, meal types, and serving counts.")));
        AddNavButton(nav, "Grocery List", () => ShowPage("Grocery List", new PlaceholderControl(
            "Grocery List",
            "Classmate module: create grocery lists from planned meals and mark items as purchased.")));
        AddNavButton(nav, "Database", () => ShowPage("Database", new PlaceholderControl(
            "Database",
            "Database diagnostics, schema notes, and sample reporting queries will be added here.")));

        var footer = new Label
        {
            Text = ".NET 8 • Windows Forms",
            Dock = DockStyle.Bottom,
            Height = 34,
            ForeColor = Color.FromArgb(205, 220, 213),
            TextAlign = ContentAlignment.MiddleLeft
        };
        sidebar.Controls.Add(footer);
        return sidebar;
    }

    private static void AddNavButton(FlowLayoutPanel parent, string text, Action onClick)
    {
        var button = new Button
        {
            Text = text,
            Width = 198,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = Color.FromArgb(25, 55, 47),
            ForeColor = Color.White,
            Margin = new Padding(0, 3, 0, 3),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 79, 68);
        button.Click += (_, _) => onClick();
        parent.Controls.Add(button);
    }

    private void ShowPage(string title, Control page)
    {
        _pageTitle.Text = title;
        _contentPanel.SuspendLayout();
        _contentPanel.Controls.Clear();
        page.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(page);
        _contentPanel.ResumeLayout();
    }
}
