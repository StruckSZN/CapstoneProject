using CapstoneWinFormsStarter.Data;

namespace CapstoneWinFormsStarter.Forms;

public sealed class MainForm : Form
{
    private readonly DatabaseService _database;
    private readonly Panel _contentPanel;
    private readonly Label _pageTitle;

    public MainForm(DatabaseService database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));

        Text = "Capstone Application";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1180, 760);
        Font = new Font("Segoe UI", 10F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var sidebar = BuildSidebar();
        root.Controls.Add(sidebar, 0, 0);

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
            BackColor = Color.FromArgb(25, 39, 59),
            Padding = new Padding(16)
        };

        var brand = new Label
        {
            Text = "CAPSTONE",
            Dock = DockStyle.Top,
            Height = 68,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
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
        AddNavButton(nav, "Database", () => ShowPage("Database", new PlaceholderControl(
            "Database",
            "Database diagnostics and schema information will be added here.")));
        AddNavButton(nav, "Feature One", () => ShowPage("Feature One", new PlaceholderControl(
            "Feature One",
            "Replace this placeholder with the first project-specific module after the requirements are confirmed.")));
        AddNavButton(nav, "Feature Two", () => ShowPage("Feature Two", new PlaceholderControl(
            "Feature Two",
            "Replace this placeholder with the second project-specific module after the requirements are confirmed.")));
        AddNavButton(nav, "Reports", () => ShowPage("Reports", new PlaceholderControl(
            "Reports",
            "Add reports that are required by the capstone rubric here.")));

        var footer = new Label
        {
            Text = ".NET 8 • Windows Forms",
            Dock = DockStyle.Bottom,
            Height = 34,
            ForeColor = Color.FromArgb(190, 200, 215),
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
            Width = 186,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = Color.FromArgb(25, 39, 59),
            ForeColor = Color.White,
            Margin = new Padding(0, 3, 0, 3),
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 61, 86);
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
