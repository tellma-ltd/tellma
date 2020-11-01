using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Email;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// A background job that awaits the <see cref="EmailQueue"/> and dispatches any Emails added there through <see cref="IEmailSender"/>
    /// </summary>
    public class EmailJob : BackgroundService
    {
        private readonly JobsOptions _options;
        private readonly EmailQueue _queue;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailJob> _logger;
        private readonly ApplicationRepositoryLite _repo;

        public EmailJob(IOptions<JobsOptions> options, EmailQueue queue, IEmailSender emailSender, ILogger<EmailJob> logger, ApplicationRepositoryLite repo)
        {
            _options = options.Value;
            _queue = queue;
            _emailSender = emailSender;
            _logger = logger;
            _repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                try // To make sure the background service keeps running
                {
                    // When the queue is empty, this goes to sleep until an item is enqueued
                    var (emails, scheduledAt) = await _queue.DequeueAsync(cancellation);

                    if (emails == null || !emails.Any())
                    {
                        continue; // Empty emails collection for some reason
                    }

                    // These Emails spent too long in the queue they're considered stale,.
                    // EmailPollingJob will (and might have already) pick them up and send them again, so ignore them
                    if (IsStale(scheduledAt))
                    {
                        var firstEmail = emails.First();
                        _logger.LogWarning($"Stale Email remained in the {nameof(EmailQueue)} for {(DateTimeOffset.Now - scheduledAt).TotalSeconds} seconds. First Email TenantId = {firstEmail.TenantId}, EmailId = {firstEmail.EmailId}.");
                        continue;
                    }

                    foreach (var emailsOfTenant in emails.GroupBy(e => e.TenantId))
                    {
                        var tenantId = emailsOfTenant.Key;
                        var stateUpdates = emailsOfTenant.Select(e => new IdStateError { Id = e.EmailId, State = EmailState.Dispatched });

                        // Begin serializable transaction
                        using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

                        // Update the state first (since this action can be rolled back)
                        await _repo.Notifications_Emails__UpdateState(tenantId, stateUpdates); // actions that modify state should not use cancellationToken

                        try
                        {
                            // Send the emails after you update the state in the DB, since sending emails
                            // is non-transactional and therefore cannot be rolled back
                            await _emailSender.SendBulkAsync(emailsOfTenant);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to Dispatch Emails. TenantId = {tenantId}, First EmailId = {emailsOfTenant.Select(e => e.EmailId).First()}");

                            // If sending the Email fails, update the state to DispatchFailed together with the error message
                            stateUpdates = emailsOfTenant.Select(e => new IdStateError { Id = e.EmailId, State = EmailState.DeliveryFailed, Error = ex.Message });
                            await _repo.Notifications_Emails__UpdateState(tenantId, stateUpdates);
                        }

                        trx.Complete();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(EmailJob)}.");
                }
            }
        }

        private bool IsStale(DateTimeOffset scheduledAt)
        {
            // If it's 90% expired, play it safe and consider it expired
            var tooLongAgo = DateTimeOffset.Now.AddSeconds(_options.PendingNotificationExpiryInSeconds * -9 / 10);
            return scheduledAt < tooLongAgo;
        }
    }
}
