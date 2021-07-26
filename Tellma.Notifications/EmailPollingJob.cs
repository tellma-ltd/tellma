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
using Tellma.Utilities.Email;

namespace Tellma.Notifications
{
    /// <summary>
    /// A background job that periodically polls the adopted tenant databases for NEW or stale PENDING emails,
    /// marks them as fresh PENDING and enqueues them in the <see cref="EmailQueue"/>. <br/>
    /// This job terminates immediately when canceled.
    /// </summary>
    public class EmailPollingJob : BackgroundService
    {
        private readonly NotificationsOptions _options;
        private readonly EmailQueue _queue;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<EmailJob> _logger;

        public EmailPollingJob(IOptions<NotificationsOptions> options, EmailQueue queue, InstanceInfoProvider instanceInfo, IApplicationRepositoryFactory repoFactory, ILogger<EmailJob> logger)
        {
            _options = options.Value;
            _queue = queue;
            _instanceInfo = instanceInfo;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        /// <summary>
        /// If we poll way too many at a time for <see cref="EmailJob"/> to handle, the rest will just expire in the <see cref="EmailQueue"/>.
        /// We estimate at least 200 email per second (emails are sent in batches of 100 per request)
        /// </summary>
        private int PollingBatchSize => _options.PendingNotificationExpiryInSeconds * 200;

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds;
                if (tenantIds.Any())
                {
                    // Match every tenantID to the Task of polling
                    IEnumerable<Task> tasks = tenantIds.Select(async tenantId =>
                    {
                        try // To make sure the background service keeps running
                        {
                            var repo = _repoFactory.GetRepository(tenantId);

                            // Begin serializable transaction
                            using var trx = new TransactionScope(
                                TransactionScopeOption.RequiresNew, 
                                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, 
                                TransactionScopeAsyncFlowOption.Enabled);

                            // Retrieve NEW or stale PENDING emails, after marking them as fresh PENDING
                            IEnumerable<EmailForSave> emailEntities = await repo.Notifications_Emails__Poll(_options.PendingNotificationExpiryInSeconds, PollingBatchSize, cancellation);
                            IEnumerable<EmailToSend> emails = emailEntities.Select(e => NotificationsQueue.FromEntity(e, tenantId));

                            // Queue the emails for dispatching
                            _queue.QueueBackgroundWorkItem(emails);

                            trx.Complete();

                            // Log a warning, since in theory this job should rarely find anything, if it finds stuff too often it means something is wrong
                            if (emails.Any())
                            {
                                _logger.LogWarning($"{nameof(EmailPollingJob)} found {emailEntities.Count()} emails in database for tenant {tenantId}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {nameof(EmailPollingJob)}. TenantId = {tenantId}.");
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
