using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;
using Tellma.Entities;

namespace Tellma.Controllers.Jobs
{
    public class ExternalNotificationsService
    {
        private readonly ApplicationLiteRepository _repo;
        private readonly EmailQueue _emailQueue;
        private readonly SmsQueue _smsQueue;
        private readonly PushNotificationQueue _pushQueue;

        public ExternalNotificationsService(ApplicationLiteRepository repo, EmailQueue emailQueue, SmsQueue smsQueue, PushNotificationQueue pushQueue)
        {
            _repo = repo;
            _emailQueue = emailQueue;
            _smsQueue = smsQueue;
            _pushQueue = pushQueue;
        }

        public async Task Enqueue(int tenantId, List<EmailQueueItem> emailQueueItems = null, List<SmsQueueItem> smsQueueItems = null, List<PushNotificationQueueItem> pushQueueItems = null, CancellationToken cancellation)
        {
            emailQueueItems ??= new List<EmailQueueItem>();
            smsQueueItems ??= new List<SmsQueueItem>();
            pushQueueItems ??= new List<PushNotificationQueueItem>();

            var emails = emailQueueItems.Select(e => Map(e)).ToList();
            var smses = smsQueueItems.Select(e => Map(e)).ToList();
            var pushes = pushQueueItems.Select(e => Map(e)).ToList();

            var trxOptions = new TransactionOptions { IsolationLevel = IsolationLevel.Serializable };
            using var trx = new TransactionScope(TransactionScopeOption.Required, trxOptions, TransactionScopeAsyncFlowOption.Enabled);

            var (emailsRead, smsesReady, pushesReady) = await _repo.Notifications_Enqueu(tenantId, emails, smses, pushes, cancellation);


            _emailQueue.QueueBackgroundWorkItem();
        }

        private static EmailForSave Map(EmailQueueItem e)
        {
            return new EmailForSave
            {
                FromEmail = e.FromEmail,
                Body = e.Body,
                Subject = e.Subject,
                ToEmail = e.ToEmail,
            };
        }

        private static EmailQueueItem Map(EmailForSave e)
        {
            return new EmailQueueItem
            {
                FromEmail = e.FromEmail,
                Body = e.Body,
                Subject = e.Subject,
                ToEmail = e.ToEmail,
            };
        }

        private static SmsMessageForSave Map(SmsQueueItem e)
        {
            return new SmsMessageForSave
            {
                ToPhoneNumber = e.ToPhoneNumber,
                Message = e.Message,
            };
        }

        private static SmsQueueItem Map(SmsMessageForSave e)
        {
            return new SmsQueueItem
            {
                ToPhoneNumber = e.ToPhoneNumber,
                Message = e.Message,
            };
        }

        private static PushNotificationForSave Map(PushNotificationQueueItem e)
        {
            return new PushNotificationForSave
            {
                Auth = e.Auth,
                Endpoint = e.Endpoint,
                P256dh = e.P256dh,

                Title = e.Content?.Title,
                Body = e.Content?.Body,
                Content = JsonConvert.SerializeObject(e.Content),
            };
        }

        private static PushNotificationQueueItem Map(PushNotificationForSave e)
        {
            PushNotificationInfo content;
            try
            {
                content = JsonConvert.DeserializeObject<PushNotificationInfo>(e.Content);
            } 
            catch
            {
                return null; // Should not happen in theory but just in case
            }

            return new PushNotificationQueueItem
            {
                Auth = e.Auth,
                Endpoint = e.Endpoint,
                P256dh = e.P256dh,
                Content = content,
            };
        }
    }
}
