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
using Tellma.Services.Email;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// A background job that periodically polls the adopted tenant databases for NEW or stale PENDING emails,
    /// marks them as fresh PENDING and enqueues them in the <see cref="EmailQueue"/>. 
    /// This job terminates immediately when canceled
    /// </summary>
    public class EmailPollingJob : BackgroundService
    {
        private readonly JobsOptions _options;
        private readonly EmailQueue _queue;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly ApplicationRepositoryLite _repo;
        private readonly ILogger<EmailJob> _logger;

        public EmailPollingJob(IOptions<JobsOptions> options, EmailQueue queue, InstanceInfoProvider instanceInfo, ApplicationRepositoryLite repo, ILogger<EmailJob> logger)
        {
            _options = options.Value;
            _queue = queue;
            _instanceInfo = instanceInfo;
            _repo = repo;
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

                            // Retrieve NEW or stale PENDING emails, after marking them as fresh PENDING
                            IEnumerable<EmailForSave> emailEntities = await _repo.Notifications_Emails__Poll(tenantId, _options.PendingNotificationExpiryInSeconds, PollingBatchSize, cancellation);
                            IEnumerable<Email> emails = emailEntities.Select(e => ExternalNotificationsService.FromEntity(e, tenantId));

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
                            _logger.LogError(ex, $"Error in {nameof(EmailPollingJob)}. TenantId = {tenantId}");
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
