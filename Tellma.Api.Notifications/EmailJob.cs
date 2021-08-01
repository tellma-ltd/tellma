using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Email;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// A background job that awaits the <see cref="EmailQueue"/> and dispatches any emails added there through <see cref="IEmailSender"/>.
    /// </summary>
    public class EmailJob : BackgroundService
    {
        private readonly NotificationsOptions _options;
        private readonly EmailQueue _queue;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailJob> _logger;
        private readonly IApplicationRepositoryFactory _repoFactory;

        public EmailJob(IOptions<NotificationsOptions> options, EmailQueue queue, IEmailSender emailSender, ILogger<EmailJob> logger, IApplicationRepositoryFactory repoFactory)
        {
            _options = options.Value;
            _queue = queue;
            _emailSender = emailSender;
            _logger = logger;
            _repoFactory = repoFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

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
                        _logger.LogWarning(
                            $"Stale Email remained in the {nameof(EmailQueue)} for {(DateTimeOffset.Now - scheduledAt).TotalSeconds} seconds. First Email TenantId = {firstEmail.TenantId}, EmailId = {firstEmail.EmailId}.");
                        continue;
                    }

                    foreach (var emailsOfTenant in emails.GroupBy(e => e.TenantId))
                    {
                        var tenantId = emailsOfTenant.Key;
                        var repo = _repoFactory.GetRepository(tenantId);

                        var stateUpdates = emailsOfTenant.Select(e => new IdStateErrorTimestamp { Id = e.EmailId, State = EmailState.Dispatched, Timestamp = DateTimeOffset.Now });

                        // Begin serializable transaction
                        using var trx = Transactions.Serializable(TransactionScopeOption.RequiresNew);

                        // Update the state first (since this action can be rolled back)

                        await repo.Notifications_Emails__UpdateState(stateUpdates, cancellation: default); // actions that modify state should not use cancellationToken

                        try
                        {
                            // Send the emails after you update the state in the DB, since sending emails
                            // is non-transactional and therefore cannot be rolled back
                            await _emailSender.SendBulkAsync(emailsOfTenant, null, cancellation: default);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to Dispatch Emails. TenantId = {tenantId}, First EmailId = {emailsOfTenant.Select(e => e.EmailId).First()}");

                            // If sending the Email fails, update the state to DispatchFailed together with the error message
                            stateUpdates = emailsOfTenant
                                .Select(e => new IdStateErrorTimestamp
                                {
                                    Id = e.EmailId,
                                    State = EmailState.DeliveryFailed,
                                    Timestamp = DateTimeOffset.Now,
                                    Error = ex.Message
                                });

                            await repo.Notifications_Emails__UpdateState(stateUpdates, cancellation: default);
                        }

                        trx.Complete();
                    }
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
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
