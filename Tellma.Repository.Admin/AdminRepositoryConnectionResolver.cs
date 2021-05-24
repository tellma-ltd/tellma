using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Admin
{
    /// <summary>
    /// The implementation of <see cref="IConnectionResolver"/> that retrieves the shard database info from <see cref="AdminRepository"/>.
    /// </summary>
    public class AdminRepositoryConnectionResolver : IConnectionResolver
    {
        private const string ADMIN_SERVER_PLACEHOLDER = "<AdminServer>";

        private readonly AdminRepository _repo;
        private readonly string _adminConnectionString;

        public AdminRepositoryConnectionResolver(AdminRepository repo, IOptions<AdminRepositoryOptions> options)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _adminConnectionString = options?.Value?.ConnectionString; // Null check not needed cause it's already done in AdminRepository
        }

        public async Task<DatabaseConnectionInfo> Resolve(int databaseId, CancellationToken cancellation)
        {
            var (serverName, dbName, userName, _) = await _repo.GetDatabaseConnectionInfo(databaseId, cancellation);
            string password = null;
            bool isWindowsAuth = false;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return default; // Nothing else matters
            }

            // This is the same SQL Server where the admin database resides
            else if (serverName == ADMIN_SERVER_PLACEHOLDER)
            {
                // Get the connection string of the manager
                var adminConnBuilder = new SqlConnectionStringBuilder(_adminConnectionString);

                // Everything comes from the Admin connection string except the database name
                serverName = adminConnBuilder.DataSource;
                userName = adminConnBuilder.UserID;
                password = adminConnBuilder.Password;

                isWindowsAuth = adminConnBuilder.IntegratedSecurity;
            }

            // ELSE: this is a different SQL Server use the information in ConnectionInfo
            else
            {
                // Use the password defaults to that of the Admin DB's SQL Server
                var adminConnBuilder = new SqlConnectionStringBuilder(_adminConnectionString);
                if (!string.IsNullOrWhiteSpace(adminConnBuilder.Password))
                {
                    // The admin SQL Server has a password = use it
                    password = adminConnBuilder.Password;
                }
                else
                {
                    // ELSE we hope that this is windows authentication on a development machine, or else the connection to the shard will sadly fail.
                    isWindowsAuth = adminConnBuilder.IntegratedSecurity;
                }
            }

            // Return the final result
            return new DatabaseConnectionInfo(
                serverName: serverName,
                databaseName: dbName,
                userName: userName,
                password: password,
                isWindowsAuth: isWindowsAuth);
        }
    }
}
