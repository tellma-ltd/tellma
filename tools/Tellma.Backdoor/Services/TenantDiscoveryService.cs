using Microsoft.Data.SqlClient;
using Tellma.Backdoor.Models;

namespace Tellma.Backdoor.Services;

public class TenantDiscoveryService
{
    private const string AdminServerPlaceholder = "<AdminServer>";

    private const string TenantQuery = """
        SELECT d.Id, d.DatabaseName, d.Description,
               s.ServerName, s.UserName, s.PasswordKey
        FROM dbo.SqlDatabases d
        INNER JOIN dbo.SqlServers s ON d.ServerId = s.Id
        ORDER BY d.Id
        """;

    public async Task<List<TenantDatabase>> DiscoverAsync(string adminConnectionString, CancellationToken ct = default)
    {
        var tenants = new List<TenantDatabase>();

        await using var conn = new SqlConnection(adminConnectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(TenantQuery, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            tenants.Add(new TenantDatabase
            {
                DatabaseId = reader.GetInt32(0),
                DatabaseName = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                ServerName = reader.GetString(3),
                UserName = reader.IsDBNull(4) ? null : reader.GetString(4),
                PasswordKey = reader.IsDBNull(5) ? null : reader.GetString(5),
            });
        }

        return tenants;
    }

    /// <summary>
    /// Builds a connection string for a tenant database, replicating the logic from
    /// AdminRepositoryConnectionResolver.ToConnectionInfo.
    /// </summary>
    public static string BuildTenantConnectionString(TenantDatabase tenant, SqlConnectionStringBuilder adminConnBuilder)
    {
        var builder = new SqlConnectionStringBuilder
        {
            PersistSecurityInfo = false,
            TrustServerCertificate = true,
            ConnectTimeout = 120,
            InitialCatalog = tenant.DatabaseName,
        };

        if (tenant.ServerName == AdminServerPlaceholder)
        {
            // Same SQL Server as admin DB — inherit credentials
            builder.DataSource = adminConnBuilder.DataSource;
            builder.UserID = adminConnBuilder.UserID;
            builder.Password = adminConnBuilder.Password;
            builder.IntegratedSecurity = adminConnBuilder.IntegratedSecurity;
        }
        else
        {
            // Different SQL Server
            builder.DataSource = tenant.ServerName;
            builder.UserID = tenant.UserName ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(adminConnBuilder.Password))
            {
                builder.Password = adminConnBuilder.Password;
            }
            else
            {
                builder.IntegratedSecurity = adminConnBuilder.IntegratedSecurity;
            }
        }

        return builder.ConnectionString;
    }

    public static async Task TestConnectionAsync(string connectionString, CancellationToken ct = default)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand("SELECT 1", conn);
        await cmd.ExecuteScalarAsync(ct);
    }
}
