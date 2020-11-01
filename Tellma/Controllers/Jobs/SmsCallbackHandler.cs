using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Sms;

namespace Tellma.Controllers.Jobs
{
    public class SmsCallbackHandler : ISmsCallbackHandler
    {
        private readonly ApplicationRepositoryLite _repo;

        public SmsCallbackHandler(ApplicationRepositoryLite repo)
        {
            _repo = repo;
        }

        public async Task HandleCallback(SmsEventNotification smsEvent, CancellationToken cancellation)
        {
            // Nothing to do
            if (smsEvent == null || smsEvent.TenantId == null)
            {
                return;
            }

            // Map the event to the database representation
            var state = smsEvent.Event switch
            {
                SmsEvent.Sent => SmsState.Sent,
                SmsEvent.Failed => SmsState.SendingFailed,
                SmsEvent.Delivered => SmsState.Delivered,
                SmsEvent.Undelivered => SmsState.DeliveryFailed,
                _ => throw new InvalidOperationException($"[Bug] Unknown {nameof(SmsEvent)} = {smsEvent.Event}"), // Future proofing
            };

            // Update the state in the database (should we make it serializable?)
            await _repo.Notifications_SmsMessages__UpdateState(smsEvent.TenantId.Value, smsEvent.MessageId, state, smsEvent.Error, cancellation);
        }
    }
}
