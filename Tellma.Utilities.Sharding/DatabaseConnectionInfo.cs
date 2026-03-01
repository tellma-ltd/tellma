using System;

namespace Tellma.Utilities.Sharding
{
    /// <summary>
    /// Contains all the information needed to build a connection string to a database.
    /// </summary>
    public struct DatabaseConnectionInfo
    {
        public DatabaseConnectionInfo(string serverName, string databaseName, string userName, string password, bool isWindowsAuth, bool trustServerCertificate)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentException($"'{nameof(serverName)}' cannot be null or whitespace.", nameof(serverName));
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            if (!isWindowsAuth)
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    throw new ArgumentException($"'{nameof(userName)}' cannot be null or whitespace.", nameof(userName));
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException($"'{nameof(password)}' cannot be null or whitespace.", nameof(password));
                }
            }

            ServerName = serverName;
            DatabaseName = databaseName;
            UserName = userName;
            Password = password;
            IsWindowsAuth = isWindowsAuth;
            TrustServerCertificate = trustServerCertificate;
        }

        public string ServerName { get; }
        public string DatabaseName { get; }
        public string UserName { get; }
        public string Password { get; }
        public bool IsWindowsAuth { get; }
        public bool TrustServerCertificate { get; }
    }
}
