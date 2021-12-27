using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Instances;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// A background job that periodically polls the adopted tenant databases for NEW or stale PENDING SMSes,
    /// marks them as fresh PENDING and enqueues them in the <see cref="SmsQueue"/>. <br/>
    /// This job terminates immediately when canceled.
    /// </summary>
    public class SmsPollingJob : BackgroundService
    {
        private readonly NotificationsOptions _options;
        private readonly SmsQueue _queue;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<SmsPollingJob> _logger;

        public SmsPollingJob(
            IOptions<NotificationsOptions> options,
            SmsQueue queue,
            InstanceInfoProvider instanceInfo,
            IApplicationRepositoryFactory repoFactory,
            ILogger<SmsPollingJob> logger)
        {
            _options = options.Value;
            _queue = queue;
            _instanceInfo = instanceInfo;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        /// <summary>
        /// If we poll way too many at a time for <see cref="SmsJob"/> to handle, the rest will just expire in the <see cref="SmsQueue"/>. <br/>
        /// We estimate 2 SMS per second.
        /// </summary>
        private int PollingBatchSize => _options.PendingNotificationExpiryInSeconds * 2; // Assuming 2 SMS per second

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds;
                if (tenantIds.Any())
                {
                    // Match every tenantID to the Task of polling
                    // Wait until all adopted tenants have returned
                    await Task.WhenAll(tenantIds.Select(async tenantId =>
                    {
                        try // To make sure the background service keeps running
                        {
                            // Begin serializable transaction
                            using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

                            // Retrieve NEW or stale PENDING SMS messages, after marking them as fresh PENDING
                            var repo = _repoFactory.GetRepository(tenantId);
                            IEnumerable<MessageForSave> smsesReady = await repo.Notifications_Messages__Poll(
                                _options.PendingNotificationExpiryInSeconds, PollingBatchSize, cancellation);

                            // Queue the SMS messages for dispatching
                            foreach (SmsToSend sms in smsesReady.Select(e => NotificationsQueue.FromEntity(e, tenantId)))
                            {
                                _queue.QueueBackgroundWorkItem(sms);
                            }

                            trx.Complete();

                            // Log a warning, since in theory this job should rarely find anything, if it finds stuff too often it means something is wrong
                            if (smsesReady.Any())
                            {
                                _logger.LogWarning($"{nameof(SmsPollingJob)} found {smsesReady.Count()} SMSes in database for tenant {tenantId}.");
                            }
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {GetType().Name}.");
                        }
                    }));
                }

                // Go to sleep until the next round
                await Task.Delay(_options.NotificationCheckFrequencyInSeconds * 1000, cancellation);
            }
        }
    }
}
