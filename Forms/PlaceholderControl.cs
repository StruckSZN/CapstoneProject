namespace MealWise.Forms;

public sealed class PlaceholderControl : UserControl
{
    public PlaceholderControl(string heading, string description)
    {
        BackColor = Color.FromArgb(245, 247, 250);

        var title = new Label
        {
            Text = heading,
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var body = new Label
        {
            Text = description,
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 70,
            Padding = new Padding(0, 12, 0, 0),
            ForeColor = Color.FromArgb(75, 85, 99)
        };

        Controls.Add(body);
        Controls.Add(title);
    }
}
