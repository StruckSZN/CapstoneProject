using System.Text.Json;

namespace MealWise;

/// <summary>
/// Reads the database connection string from an environment variable first,
/// then from appsettings.json. This keeps credentials out of source code.
/// </summary>
public sealed class AppSettings
{
    public string GetConnectionString()
    {
        var environmentValue = Environment.GetEnvironmentVariable("CAPSTONE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException(
                "appsettings.json was not found. Copy appsettings.example.json to appsettings.json and configure the database.",
                settingsPath);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));
        if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
            && connectionStrings.TryGetProperty("DefaultConnection", out var connectionString))
        {
            var value = connectionString.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is missing or empty in appsettings.json.");
    }
}
