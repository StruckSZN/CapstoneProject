using Microsoft.Data.SqlClient;

namespace CapstoneWinFormsStarter.Data;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _connectionString = settings.GetConnectionString();
    }

    /// <summary>Opens the configured SQL Server database and returns its name.</summary>
    public async Task<string> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("SELECT DB_NAME();", connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToString(result) ?? "Connected (database name unavailable)";
    }

    /// <summary>Creates a new unopened connection for feature-specific queries.</summary>
    public SqlConnection CreateConnection() => new(_connectionString);
}
