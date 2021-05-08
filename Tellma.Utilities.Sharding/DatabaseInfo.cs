using System;

namespace Tellma.Utilities.Sharding
{
    public struct DatabaseInfo
    {
        public DatabaseInfo(string sqlServerName, string databaseName, string username, string passwordKey, bool isWindowsAuth)
        {
            if (string.IsNullOrWhiteSpace(sqlServerName))
            {
                throw new ArgumentException($"'{nameof(sqlServerName)}' cannot be null or whitespace.", nameof(sqlServerName));
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            if (!isWindowsAuth)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException($"'{nameof(username)}' cannot be null or whitespace.", nameof(username));
                }

                if (string.IsNullOrWhiteSpace(passwordKey))
                {
                    throw new ArgumentException($"'{nameof(passwordKey)}' cannot be null or whitespace.", nameof(passwordKey));
                }
            }

            SqlServerName = sqlServerName;
            SqlDatabaseName = databaseName;
            UserName = username;
            PasswordKey = passwordKey;
            IsWindowsAuth = isWindowsAuth;
        }

        public string SqlServerName { get; }
        public string SqlDatabaseName { get; }
        public string UserName { get; set; }
        public string PasswordKey { get; set; }
        public bool IsWindowsAuth { get; set; }
    }
}
