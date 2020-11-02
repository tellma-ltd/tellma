using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Sms;

namespace Tellma.Controllers.Jobs
{
    /// <summary>
    /// A background job that awaits the <see cref="SmsQueue"/> and dispatches any SMSes added there through <see cref="ISmsSender"/>
    /// </summary>
    public class SmsJob : BackgroundService
    {
        private readonly JobsOptions _options;
        private readonly SmsQueue _queue;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<SmsJob> _logger;
        private readonly ApplicationRepositoryLite _repo;

        public SmsJob(IOptions<JobsOptions> options, SmsQueue queue, ISmsSender smsSender, ILogger<SmsJob> logger, ApplicationRepositoryLite repo)
        {
            _options = options.Value;
            _queue = queue;
            _smsSender = smsSender;
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
                    var (sms, scheduledAt) = await _queue.DequeueAsync(cancellation);

                    // This SMS spent too long in the queue it is considered stale, SmsPollingJob will
                    // (and might have already) pick it up and send it again, so ignore it
                    if (IsStale(scheduledAt))
                    {
                        _logger.LogWarning($"Stale SMS remained in the {nameof(SmsQueue)} for {(DateTimeOffset.Now - scheduledAt).TotalSeconds} seconds. TenantId = {sms.TenantId}, MessageId = {sms.MessageId}.");
                        continue;
                    }

                    // Begin serializable transaction
                    using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

                    // Update the state first (since this action can be rolled back)
                    await _repo.Notifications_SmsMessages__UpdateState(sms.TenantId, sms.MessageId, SmsState.Dispatched, DateTimeOffset.Now); // actions that modify state should not use cancellationToken

                    try
                    {
                        // Send the SMS after you update the state in the DB, since sending SMS 
                        // is non-transactional and therefore cannot be rolled back
                        await _smsSender.SendAsync(sms);
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to Dispatch SMS. TenantId = {sms.TenantId}, MessageId = {sms.MessageId}.");

                        // If sending the SMS fails, update the state to DispatchFailed together with the error message
                        await _repo.Notifications_SmsMessages__UpdateState(sms.TenantId, sms.MessageId, SmsState.DispatchFailed, DateTimeOffset.Now, ex.Message);
                    }

                    trx.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(SmsJob)}.");
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
