using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Entities;
using Tellma.Services.Sharding;
using System.Linq;

namespace Tellma.Data
{
    /// <summary>
    /// All application stored procedures that are stateless and independent of
    /// the session context, this service is Thread-Safe and is registered as a singleton
    /// </summary>
    public class ApplicationRepositoryLite
    {
        private readonly IShardResolver _shardResolver;
        private readonly ILogger<ApplicationRepositoryLite> _logger;
        private readonly Random _rand = new Random();

        public ApplicationRepositoryLite(IShardResolver shardResolver, ILogger<ApplicationRepositoryLite> logger)
        {
            _shardResolver = shardResolver;
            _logger = logger;
        }

        /// <summary>
        /// Adds the Emails and SMSes to the database queue tables in state PENDING 
        /// IF the respective queue table (email, SMS or push) does not have any NEW or stale PENDING items, return TRUE for that collection, otherwise FALSE
        /// </summary>
        public async Task<(bool queueEmails, bool queueSmsMessages, bool queuePushNotifications)> Notifications_Enqueue(
            int tenantId, int expiryInSeconds, List<EmailForSave> emails, List<SmsMessageForSave> smses, List<PushNotificationForSave> pushes, CancellationToken cancellation)
        {
            bool queueEmails = false;
            bool queueSmsMessages = false;
            bool queuePushNotifications = false;

            //////////////// Prepare the Email TVP
            DataTable emailTable = new DataTable();

            emailTable.Columns.Add(new DataColumn("Index", typeof(int)));
            emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.ToEmail), typeof(string)) { MaxLength = 256 });
            emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.Subject), typeof(string)) { MaxLength = 1024 });
            emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.Body), typeof(string)));
            emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.State), typeof(short)));
            emailTable.Columns.Add(new DataColumn(nameof(EmailForSave.ErrorMessage), typeof(string)) { MaxLength = 2048 });

            int emailIndex = 0;
            foreach (var email in emails)
            {
                DataRow row = emailTable.NewRow();

                row["Index"] = emailIndex++;
                row[nameof(email.ToEmail)] = email.ToEmail;
                row[nameof(email.Subject)] = email.Subject;
                row[nameof(email.Body)] = email.Body;
                row[nameof(email.State)] = email.State;
                row[nameof(email.ErrorMessage)] = email.ErrorMessage;

                emailTable.Rows.Add(row);
            }

            SqlParameter emailTvp = new SqlParameter("@Emails", emailTable)
            {
                TypeName = $"[dbo].[EmailList]",
                SqlDbType = SqlDbType.Structured
            };

            //////////////// Prepare the SMS TVP
            DataTable smsTable = new DataTable(); // We won't use the utility function because we don't want to include Id

            smsTable.Columns.Add(new DataColumn("Index", typeof(int)));
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.ToPhoneNumber), typeof(string)) { MaxLength = 50 });
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.Message), typeof(string)) { MaxLength = 1600 });
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.State), typeof(short)));
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.ErrorMessage), typeof(string)) { MaxLength = 2048 });

            int smsIndex = 0;
            foreach (var sms in smses)
            {
                DataRow row = smsTable.NewRow();

                row["Index"] = smsIndex++;
                row[nameof(sms.ToPhoneNumber)] = sms.ToPhoneNumber;
                row[nameof(sms.Message)] = sms.Message;
                row[nameof(sms.State)] = sms.State;
                row[nameof(sms.ErrorMessage)] = sms.ErrorMessage;

                smsTable.Rows.Add(row);
            }

            SqlParameter smsTvp = new SqlParameter("@SmsMessages", smsTable)
            {
                TypeName = $"[dbo].[SmsMessageList]",
                SqlDbType = SqlDbType.Structured
            };


            //////////////// Prepare the Push Notifications TVP

            // TODO


            //////////////// Output parameters

            var queueEmailsParam = new SqlParameter("@QueueEmails", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var queueSmsMessagesParam = new SqlParameter("@QueueSmsMessages", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var queuePushNotificationsParam = new SqlParameter("@QueuePushNotifications", SqlDbType.Bit) { Direction = ParameterDirection.Output };

            // Prepare connection string and command
            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_Enqueue)}]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };


            // Parameters

            cmd.Parameters.Add(emailTvp);
            cmd.Parameters.Add(smsTvp);
            // cmd.Parameters.Add(pushTvp);
            cmd.Parameters.Add(queueEmailsParam);
            cmd.Parameters.Add(queueSmsMessagesParam);
            cmd.Parameters.Add(queuePushNotificationsParam);
            cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);

            // Open connection and execute
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                // Execute
                using (var reader = await cmd.ExecuteReaderAsync(cancellation))
                {
                    // Load Email Ids
                    while (await reader.ReadAsync(cancellation))
                    {
                        var index = reader.GetInt32(0);
                        var id = reader.GetInt32(1);

                        emails[index].Id = id;
                    }

                    // Load SMS Ids
                    await reader.NextResultAsync(cancellation);
                    while (await reader.ReadAsync(cancellation))
                    {
                        var index = reader.GetInt32(0);
                        var id = reader.GetInt32(1);

                        smses[index].Id = id;
                    }

                    // Load Push Ids
                    // TODO
                }

                // Get the output parameters
                queueEmails = GetValue(queueEmailsParam.Value, false);
                queueSmsMessages = GetValue(queueSmsMessagesParam.Value, false);
                queuePushNotifications = GetValue(queuePushNotificationsParam.Value, false);
            }, 
            nameof(Notifications_Enqueue));

            // Return the result
            return (queueEmails, queueSmsMessages, queuePushNotifications);
        }

        /// <summary>
        /// Takes a list of (Id, State, Error), and updates the state of every email with a given Id to the given state.
        /// It also marks [StateSince] to the current time and persists the given Error in the Error column if the state is negative
        /// </summary>
        public async Task Notifications_Emails__UpdateState(int tenantId, IEnumerable<IdStateError> stateUpdates, IEnumerable<IdStateError> engagementUpdates = null, CancellationToken cancellation = default)
        {
            stateUpdates ??= new List<IdStateError>();
            engagementUpdates ??= new List<IdStateError>();

            if (!stateUpdates.Any() && !engagementUpdates.Any())
            {
                return;
            }

            // Prep connection
            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);

            // Command and parameters
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_Emails__UpdateState)}]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            var stateUpdatesTable = RepositoryUtilities.DataTable(stateUpdates);
            SqlParameter stateUpdatesTvp = new SqlParameter("@StateUpdates", stateUpdatesTable)
            {
                TypeName = $"[dbo].[{nameof(IdStateError)}List]",
                SqlDbType = SqlDbType.Structured
            };

            var engagementUpdatesTable = RepositoryUtilities.DataTable(stateUpdates);
            SqlParameter engagementUpdatesTvp = new SqlParameter("@EngagementUpdates", stateUpdatesTable)
            {
                TypeName = $"[dbo].[{nameof(IdStateError)}List]",
                SqlDbType = SqlDbType.Structured
            };

            // Execute the Query
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                await cmd.ExecuteNonQueryAsync(cancellation);
            },
            nameof(Notifications_Emails__UpdateState));
        }

        /// <summary>
        /// Returns the Top N emails that are either NEW or stale PENDING after marking them as fresh PENDING
        /// </summary>
        /// <param name="tenantId">The database Id to query</param>
        /// <param name="expiryInSeconds">How many seconds should an email remain pending in the table to be considered "stale"</param>
        /// <param name="top">Maximum number of items to return</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        public async Task<IEnumerable<EmailForSave>> Notifications_Emails__Poll(int tenantId, int expiryInSeconds, int top, CancellationToken cancellation)
        {
            var result = new List<EmailForSave>();

            // Prep connection
            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);

            // Command and parameters
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_Emails__Poll)}]", conn)
            {
                CommandType = CommandType.StoredProcedure,
            };

            cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);
            cmd.Parameters.AddWithValue("@Top", top);

            // Execute the Query
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                // Load Entities
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result.Add(new EmailForSave
                    {
                        Id = reader.GetInt32(i++),
                        ToEmail = reader.GetString(i++),
                        Subject = reader.String(i++),
                        Body = reader.String(i++)
                    });
                }
            }, 
            nameof(Notifications_Emails__Poll));

            return result;
        }

        /// <summary>
        /// Returns the Top N SMS messages that are either NEW or stale PENDING after marking them as fresh PENDING
        /// </summary>
        /// <param name="tenantId">The database Id to query</param>
        /// <param name="expiryInSeconds">How many seconds should an SMS remain pending in the table to be considered "stale"</param>
        /// <param name="top">Maximum number of items to return</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        public async Task<IEnumerable<SmsMessageForSave>> Notifications_SmsMessages__Poll(int tenantId, int expiryInSeconds, int top, CancellationToken cancellation)
        {
            var result = new List<SmsMessageForSave>();

            // Prep connection
            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);

            // Command and parameters
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_SmsMessages__Poll)}]", conn)
            {
                CommandType = CommandType.StoredProcedure,
            };

            cmd.Parameters.AddWithValue("@ExpiryInSeconds", expiryInSeconds);
            cmd.Parameters.AddWithValue("@Top", top);

            // Execute the Query
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                // Load Entities
                using var reader = await cmd.ExecuteReaderAsync(cancellation);
                while (await reader.ReadAsync(cancellation))
                {
                    int i = 0;

                    result.Add(new SmsMessageForSave
                    {
                        Id = reader.GetInt32(i++),
                        ToPhoneNumber = reader.GetString(i++),
                        Message = reader.GetString(i++)
                    });
                }
            }, nameof(Notifications_SmsMessages__Poll));

            return result;
        }

        /// <summary>
        /// Updates the SMS message with a given Id to a new state, as long as the current state is not terminal or greater
        /// than the new state. It also marks [StateSince] to the current time and persists the given Error in the Error column if the state is negative
        /// </summary>
        /// <param name="tenantId">The database Id to query</param>
        /// <param name="id">The Id of the SMS to update</param>
        /// <param name="state">The new state</param>
        public async Task Notifications_SmsMessages__UpdateState(int tenantId, int id, short state, string error = null, CancellationToken cancellation = default)
        {
            // Prep connection
            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);

            // Command and parameters
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_SmsMessages__UpdateState)}]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@NewState", state);
            cmd.Parameters.Add("@Error", error);

            // Execute the Query
            await conn.OpenAsync(cancellation);
            await ExponentialBackoff(cancellation, async () =>
            {
                await cmd.ExecuteNonQueryAsync(cancellation);
            }, nameof(Notifications_SmsMessages__UpdateState));
        }

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
                        _logger.LogWarning(ex, $"SP or Query {spName} deadlocked after {attemptsSoFar} attempt(s).");

                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        _logger.LogError(ex, $"SP or Query {spName} deadlocked after {attemptsSoFar} attempts.");
                        throw; // Give up
                    }
                }
            }
        }

        /// <summary>
        /// Utility function: if obj is <see cref="DBNull.Value"/>, returns the default value of the type, else returns cast value
        /// </summary>
        private T GetValue<T>(object obj, T defaultValue = default)
        {
            if (obj == DBNull.Value)
            {
                return defaultValue;
            }
            else
            {
                return (T)obj;
            }
        }

        #endregion
    }
}
