using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;
using Tellma.Entities.Descriptors;
using Tellma.Services;
using Microsoft.Extensions.Logging;

namespace Tellma.Data
{
    /// <summary>
    /// All admin stored procedures that are stateless and independent of
    /// the session context, this service is Thread-Safe and is registered as a singleton
    /// </summary>
    public class AdminRepositoryLite
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly Random _rand = new Random();

        #region Lifecycle

        public AdminRepositoryLite(IOptions<AdminRepositoryOptions> config, ILogger<AdminRepositoryLite> logger)
        {
            _connectionString = config?.Value?.ConnectionString ?? throw new ArgumentException("The admin connection string was not supplied", nameof(config));
            _logger = logger;
        }

        #endregion

        #region Jobs

        public async Task Heartbeat(Guid instanceId, int keepAliveInSeconds, CancellationToken cancellation)
        {
            // Connection
            using var conn = new SqlConnection(_connectionString);

            // Command
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(Heartbeat)}]";

            // Parameters
            cmd.Parameters.Add("@InstanceId", instanceId);
            cmd.Parameters.Add("@KeepAliveInSeconds", keepAliveInSeconds);

            // Execute
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            nameof(Heartbeat));
        }

        public async Task<IEnumerable<int>> AdoptOrphans(Guid instanceId, int keepAliveInSeconds, int orphanCount, CancellationToken cancellation)
        {
            var result = new List<int>();

            // Connection
            using var conn = new SqlConnection(_connectionString);

            // Command
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = $"[dal].[{nameof(AdoptOrphans)}]";

            // Parameters
            cmd.Parameters.Add("@InstanceId", instanceId);
            cmd.Parameters.Add("@KeepAliveInSeconds", keepAliveInSeconds);
            cmd.Parameters.Add("@OrphanCount", orphanCount);

            // Execute and Load
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    result.Add(reader.GetInt32(0));
                }
            },
            nameof(AdoptOrphans));

            return result;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Helper function that executes an async SQL query with exponential backoff
        /// </summary>
        private async Task ExponentialBackoff(CancellationToken cancellation, Func<Task> sqlQuery, string spName)
        {
            // Exponential backoff
            const int maxAttempts = 2;
            const int maxBackoff = 4000; // 4 Seconds
            const int minBackoff = 1000; // 1 Second
            const int deltaBackoff = 1000; // 1 Second

            int attemptsSoFar = 0;
            int backoff = minBackoff;

            while (attemptsSoFar < maxAttempts && !cancellation.IsCancellationRequested)
            {
                attemptsSoFar++;
                try
                {
                    // Load Email Ids
                    await sqlQuery();

                    // Break the cycle if successful
                    break;
                }
                catch (SqlException ex) when (ex.Number == 1205) // 1205 = Deadlock
                {
                    // Exponential backoff in case 
                    if (attemptsSoFar < maxAttempts)
                    {
                        _logger.LogWarning(ex, $"Admin SP or Query {spName} deadlocked after {attemptsSoFar} attempt(s).");

                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        _logger.LogError(ex, $"Admin SP or Query {spName} deadlocked after {attemptsSoFar} attempts.");
                        throw; // Give up
                    }
                }
            }
        }

        #endregion
    }
}
