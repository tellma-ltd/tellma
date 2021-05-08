using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Utilities.Sharding
{
    public struct DatabaseInfo
    {
        public DatabaseInfo(string sqlServerName, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(sqlServerName))
            {
                throw new ArgumentException($"'{nameof(sqlServerName)}' cannot be null or whitespace.", nameof(sqlServerName));
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException($"'{nameof(databaseName)}' cannot be null or whitespace.", nameof(databaseName));
            }

            SqlServerName = sqlServerName;
            DatabaseName = databaseName;
        }

        public string SqlServerName { get; }
        public string DatabaseName { get; }
    }
}
