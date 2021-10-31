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
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
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
        private readonly IBlobService _blobService;
        private readonly ILogger<EmailPollingJob> _logger;

        public EmailPollingJob(
            IOptions<NotificationsOptions> options,
            EmailQueue queue,
            InstanceInfoProvider instanceInfo,
            IApplicationRepositoryFactory repoFactory,
            IBlobService blobService,
            ILogger<EmailPollingJob> logger)
        {
            _options = options.Value;
            _queue = queue;
            _instanceInfo = instanceInfo;
            _repoFactory = repoFactory;
            _blobService = blobService;
            _logger = logger;
        }

        /// <summary>
        /// If we poll way too many at a time for <see cref="EmailJob"/> to handle, the rest will just expire in the <see cref="EmailQueue"/>.
        /// We estimate at least 200 email per second (emails are sent in batches of 100 per request)
        /// </summary>
        private int PollingBatchSize => _options.PendingNotificationExpiryInSeconds * 200;

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds;
                if (tenantIds.Any())
                {
                    // Match every tenantId to the Task of polling
                    // Then Wait until all adopted tenants have returned
                    await Task.WhenAll(tenantIds.Select(async tenantId =>
                    {
                        try // To make sure the background service keeps running
                        {
                            var repo = _repoFactory.GetRepository(tenantId);

                            // Begin serializable transaction
                            using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

                            // Retrieve NEW or stale PENDING emails, after marking them as fresh PENDING
                            IEnumerable<EmailForSave> emailEntities = await repo.Notifications_Emails__Poll(_options.PendingNotificationExpiryInSeconds, PollingBatchSize, cancellation);
                            var emails = await NotificationsQueue.FromEntities(tenantId, emailEntities, _blobService, cancellation);
                            // IEnumerable<EmailToSend> emails = emailEntities.Select(e => NotificationsQueue.FromEntity(e, tenantId));

                            // Queue the emails for dispatching
                            _queue.QueueBackgroundWorkItem(emails);

                            trx.Complete();

                            // Log a warning, since in theory this job should rarely find anything, if it finds stuff too often it means something is wrong
                            if (emails.Any())
                            {
                                _logger.LogWarning($"{nameof(EmailPollingJob)} found {emailEntities.Count()} emails in database for tenant {tenantId}.");
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
