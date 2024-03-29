﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Notifications
{
    /// <summary>
    /// Implementation of <see cref="ISmsCallbackHandler"/> which updates the email states 
    /// in the <see cref="ApplicationRepository"/>.
    /// </summary>
    public class SmsCallbackHandler : ISmsCallbackHandler
    {
        private readonly IApplicationRepositoryFactory _repoFactory;

        public SmsCallbackHandler(IApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        public async Task HandleCallback(SmsEventNotification smsEvent, CancellationToken cancellation)
        {
            // Nothing to do
            if (smsEvent == null || smsEvent.TenantId == null || smsEvent.TenantId == 0) // Right now we do not handle null tenant Ids, those were probably sent from identity or admin servers
            {
                return;
            }

            // Map the event to the database representation
            var state = smsEvent.Event switch
            {
                SmsEvent.Sent => MessageState.Sent,
                SmsEvent.Failed => MessageState.SendingFailed,
                SmsEvent.Delivered => MessageState.Delivered,
                SmsEvent.Undelivered => MessageState.DeliveryFailed,
                _ => throw new InvalidOperationException($"[Bug] Unknown {nameof(SmsEvent)} = {smsEvent.Event}"), // Future proofing
            };

            // Update the state in the database (should we make it serializable?)
            var repo = _repoFactory.GetRepository(tenantId: smsEvent.TenantId.Value);

            // Begin serializable transaction
            using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

            await repo.Notifications_Messages__UpdateState(
                id: smsEvent.MessageId, 
                state: state, 
                timestamp: smsEvent.Timestamp, 
                error: smsEvent.Error, 
                cancellation: cancellation); ;

            trx.Complete();
        }
    }
}
