namespace MealWise;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var settings = new AppSettings();
            var database = new Data.DatabaseService(settings);
            Application.Run(new Forms.MainForm(database));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"The application could not start.\n\n{ex.Message}",
                "Meal Planner Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
