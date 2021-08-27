using Microsoft.Extensions.Options;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Sharding;

namespace Tellma.Repository.Admin
{
    /// <summary>
    /// The implementation of <see cref="IConnectionInfoLoader"/> that retrieves the shard database info from <see cref="AdminRepository"/>.
    /// </summary>
    public class AdminRepositoryConnectionResolver : IConnectionInfoLoader
    {
        private const string AdminServerPlaceholder = "<AdminServer>";

        private readonly AdminRepository _repo;
        private readonly SqlConnectionStringBuilder _adminConnBuilder;

        public AdminRepositoryConnectionResolver(AdminRepository repo, IOptions<AdminRepositoryOptions> options)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));

            var adminConn = options?.Value?.ConnectionString; // Null check not needed cause it's already done in AdminRepository
            _adminConnBuilder = new SqlConnectionStringBuilder(adminConn);
        }

        public async Task<DatabaseConnectionInfo> Load(int databaseId, CancellationToken cancellation)
        {
            using var trx = TransactionFactory.Suppress();

            var (serverName, dbName, userName, passwordKey) = await _repo.GetDatabaseConnectionInfo(databaseId, cancellation);
            
            trx.Complete();
            return ToConnectionInfo(serverName, dbName, userName, passwordKey, _adminConnBuilder);
        }

        public static DatabaseConnectionInfo ToConnectionInfo(
            string serverName, 
            string dbName, 
            string userName, 
            string _, SqlConnectionStringBuilder adminConnBuilder)
        {
            string password = null;
            bool isWindowsAuth = false;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                return default; // Nothing else matters
            }

            // This is the same SQL Server where the admin database resides
            else if (serverName == AdminServerPlaceholder)
            {
                // Everything comes from the Admin connection string except the database name
                serverName = adminConnBuilder.DataSource;
                userName = adminConnBuilder.UserID;
                password = adminConnBuilder.Password;

                isWindowsAuth = adminConnBuilder.IntegratedSecurity;
            }

            // ELSE: this is a different SQL Server use the information in ConnectionInfo
            else
            {
                // The password defaults to that of the Admin DB's SQL Server
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
