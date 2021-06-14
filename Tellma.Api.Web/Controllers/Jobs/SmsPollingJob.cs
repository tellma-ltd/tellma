using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;
using Tellma.Model.Application;
using Tellma.Services.Sms;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// A background job that periodically polls the adopted tenant databases for NEW or stale PENDING SMSes,
    /// marks them as fresh PENDING and enqueues them in the <see cref="SmsQueue"/>. 
    /// This job terminates immediately when canceled
    /// </summary>
    public class SmsPollingJob : BackgroundService
    {
        private readonly JobsOptions _options;
        private readonly SmsQueue _queue;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly ApplicationRepositoryLite _repo;
        private readonly ILogger<SmsJob> _logger;

        public SmsPollingJob(IOptions<JobsOptions> options, SmsQueue queue, InstanceInfoProvider instanceInfo, ApplicationRepositoryLite repo, ILogger<SmsJob> logger)
        {
            _options = options.Value;
            _queue = queue;
            _instanceInfo = instanceInfo;
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// If we poll way too many at a time for <see cref="SmsJob"/> to handle, the rest will just expire in the <see cref="SmsQueue"/>.
        /// We estimate 2 SMS per second
        /// </summary>
        private int PollingBatchSize => _options.PendingNotificationExpiryInSeconds * 2; // Assuming 2 SMS per second

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds.ToList();
                if (tenantIds.Any())
                {
                    // Match every tenantID to the Task of polling
                    IEnumerable<Task> tasks = tenantIds.Select(async tenantId =>
                    {
                        try // To make sure the background service keeps running
                        {
                            // Begin serializable transaction
                            using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

                            // Retrieve NEW or stale PENDING SMS messages, after marking them as fresh PENDING
                            IEnumerable<SmsMessageForSave> smsesReady = await _repo.Notifications_SmsMessages__Poll(tenantId, _options.PendingNotificationExpiryInSeconds, PollingBatchSize, cancellation);

                            // Queue the SMS messages for dispatching
                            foreach (SmsMessage sms in smsesReady.Select(e => ExternalNotificationsService.FromEntity(e, tenantId)))
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
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {nameof(SmsPollingJob)}. TenantId = {tenantId}");
                        }
                    });

                    // Wait until all adopted tenants have returned
                    await Task.WhenAll(tasks);
                }

                // Go to sleep until the next round
                await Task.Delay(_options.NotificationCheckFrequencyInSeconds * 1000, cancellation);
            }
        }
    }
}
