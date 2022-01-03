using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// A background job that awaits the <see cref="SmsQueue"/> and dispatches any SMSes added there through <see cref="ISmsSender"/>.
    /// </summary>
    public class SmsJob : BackgroundService
    {
        private readonly NotificationsOptions _options;
        private readonly SmsQueue _queue;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<SmsJob> _logger;
        private readonly IApplicationRepositoryFactory _repoFactory;

        public SmsJob(IOptions<NotificationsOptions> options, SmsQueue queue, ISmsSender smsSender, ILogger<SmsJob> logger, IApplicationRepositoryFactory repoFactory)
        {
            _options = options.Value;
            _queue = queue;
            _smsSender = smsSender;
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
                    var (sms, scheduledAt) = await _queue.DequeueAsync(cancellation);

                    // This SMS spent too long in the queue it is considered stale, SmsPollingJob will
                    // (and might have already) pick it up and send it again, so ignore it
                    if (IsStale(scheduledAt))
                    {
                        _logger.LogWarning($"Stale SMS remained in the {nameof(SmsQueue)} for {(DateTimeOffset.Now - scheduledAt).TotalSeconds} seconds. TenantId = {sms.TenantId}, MessageId = {sms.MessageId}.");
                        continue;
                    }

                    // Begin serializable transaction
                    using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

                    // Update the state first (since this action can be rolled back)
                    var repo = _repoFactory.GetRepository(tenantId: sms.TenantId);
                    await repo.Notifications_Messages__UpdateState(sms.MessageId, MessageState.Dispatched, DateTimeOffset.Now, cancellation: default); // actions that modify state should not use cancellationToken

                    try
                    {
                        // Send the SMS after you update the state in the DB, since sending SMS 
                        // is non-transactional and therefore cannot be rolled back
                        await _smsSender.SendAsync(sms, cancellation: default);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to Dispatch SMS. TenantId = {sms.TenantId}, MessageId = {sms.MessageId}.");

                        // If sending the SMS fails, update the state to DispatchFailed together with the error message
                        await repo.Notifications_Messages__UpdateState(sms.MessageId, MessageState.DispatchFailed, DateTimeOffset.Now, ex.Message, cancellation: default);
                    }

                    trx.Complete();
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
