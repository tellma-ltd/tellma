using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Entities;
using Tellma.Services.Sharding;

namespace Tellma.Data
{
    /// <summary>
    /// All application stored procedures that are stateless and independent of
    /// the session context, this service is registered as a singleton
    /// </summary>
    public class ApplicationLiteRepository
    {
        private readonly IShardResolver _shardResolver;
        private readonly Random _rand = new Random();

        public ApplicationLiteRepository(IShardResolver shardResolver)
        {
            _shardResolver = shardResolver;
        }

        /// <summary>
        /// Adds the Emails and SMSes to the database queues in state PENDING 
        /// IF the database queues do not have any NEW or stale PENDING items, return the new IDs immediately for processing.
        /// If the database queues contain NEW or stale PENDING items, don't return anything
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="emails"></param>
        /// <param name="smses"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<EmailForSave> emails, IEnumerable<SmsMessageForSave> smses, IEnumerable<PushNotificationForSave> pushes)>
            Notifications_Enqueu(int tenantId, List<EmailForSave> emails, List<SmsMessageForSave> smses, List<PushNotificationForSave> pushes, CancellationToken cancellation)
        {
            List<EmailForSave> emailsReadyToSend = new List<EmailForSave>(emails.Count);
            List<SmsMessageForSave> smsesReadyToSend = new List<SmsMessageForSave>(smses.Count);
            List<PushNotificationForSave> pushesReadyToSend = new List<PushNotificationForSave>(pushes.Count);

            // Prepare the Email Table
            //DataTable emailTable = new DataTable();

            //emailTable.Columns.Add(new DataColumn("Index", typeof(int)));

            // Prepare the SMS Table
            DataTable smsTable = new DataTable();

            smsTable.Columns.Add(new DataColumn("Index", typeof(int)));
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.ToPhoneNumber), typeof(string)) { MaxLength = 50 });
            smsTable.Columns.Add(new DataColumn(nameof(SmsMessageForSave.Message), typeof(string)) { MaxLength = 1024 });

            int smsIndex = 0;
            foreach (var sms in smses)
            {
                DataRow row = smsTable.NewRow();

                row["Index"] = smsIndex++;
                row[nameof(sms.ToPhoneNumber)] = sms.ToPhoneNumber;
                row[nameof(sms.Message)] = sms.Message;

                smsTable.Rows.Add(row);
            }

            SqlParameter smsTvp = new SqlParameter("@Smses", smsTable)
            {
                TypeName = $"[dbo].[NotificationSmsList]",
                SqlDbType = SqlDbType.Structured
            };


            // TODO: Prepare the Push Notifications TVP

            string connString = await _shardResolver.GetConnectionString(tenantId, cancellation);
            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand($"[dal].[{nameof(Notifications_Enqueu)}]", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add Email and SMS parameters

            // cmd.Parameters.Add(emailsTvp);
            cmd.Parameters.Add(smsTvp);
            // cmd.Parameters.Add(pushTvp);


            // Execute the Query

            await conn.OpenAsync(cancellation);
            while (!cancellation.IsCancellationRequested)
            {
                // Exponential backoff
                const int maxAttempts = 2;
                const int maxBackoff = 4000; // 4 Seconds
                const int minBackoff = 1000; // 1 Second
                const int deltaBackoff = 1000; // 1 Second

                int attemptsSoFar = 0;
                int backoff = minBackoff;
                try
                {
                    attemptsSoFar++;

                    // Load Email Ids
                    using var reader = await cmd.ExecuteReaderAsync(cancellation);
                    while (await reader.ReadAsync(cancellation))
                    {
                        int i = 0;
                        int index = reader.GetInt32(i++);

                        var email = emails[index];
                        email.Id = reader.GetInt32(i++);
                        emailsReadyToSend.Add(email);
                    }

                    // Load SMS Ids
                    await reader.NextResultAsync(cancellation);
                    while (await reader.ReadAsync(cancellation))
                    {
                        int i = 0;
                        int index = reader.GetInt32(i++);

                        var sms = smses[index];
                        sms.Id = reader.GetInt32(i++);
                        smsesReadyToSend.Add(sms);
                    }

                    // Load Push Notification Ids
                    await reader.NextResultAsync(cancellation);
                    while (await reader.ReadAsync(cancellation))
                    {
                        int i = 0;
                        int index = reader.GetInt32(i++);

                        var push = pushes[index];
                        push.Id = reader.GetInt32(i++);
                        pushesReadyToSend.Add(push);
                    }
                }
                catch (SqlException ex) when (ex.Number == 1205)
                {
                    // Exponential backoff in case 
                    if (attemptsSoFar < maxAttempts)
                    {
                        var randomOffset = _rand.Next(0, deltaBackoff);
                        await Task.Delay(backoff + randomOffset, cancellation);

                        // Double the backoff for next attempt
                        backoff = Math.Min(backoff * 2, maxBackoff);
                    }
                    else
                    {
                        throw; // Give up
                    }
                }
            }

            return (emailsReadyToSend, smsesReadyToSend, pushesReadyToSend);
        }
    }
}
