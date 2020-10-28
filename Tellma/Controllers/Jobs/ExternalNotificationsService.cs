using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Email;
using Tellma.Services.Sms;

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

        public async Task Enqueue(int tenantId, List<Email> emails = null, List<SmsMessage> smsMessages = null, List<PushNotification> pushNotifications = null, CancellationToken cancellation = default)
        {
            emails ??= new List<Email>();
            smsMessages ??= new List<SmsMessage>();
            pushNotifications ??= new List<PushNotification>();

            var emailsDic = emails.ToDictionary(e => ToEntity(e));
            var smsesDic = smsMessages.ToDictionary(e => ToEntity(e));
            var pushesDic = pushNotifications.ToDictionary(e => ToEntity(e));

            var trxOptions = new TransactionOptions { IsolationLevel = IsolationLevel.Serializable };
            using var trx = new TransactionScope(TransactionScopeOption.Required, trxOptions, TransactionScopeAsyncFlowOption.Enabled);

            // Persist the notifications in the database
            // The returned notifications are those that we can queue immediately, since their tables contain no previous NEW or stale PENDING notifications
            var (emailsReady, smsesReady, pushesReady) = await _repo.Notifications_Enqueu(
                    tenantId: tenantId,
                    emails: emailsDic.Keys.ToList(),
                    smses: smsesDic.Keys.ToList(),
                    pushes: pushesDic.Keys.ToList(),
                    cancellation: cancellation);

            // Queue emails
            _emailQueue.QueueBackgroundWorkItem(emailsReady.Select(e => emailsDic[e]));

            // Queue SMS messages
            foreach (SmsMessage sms in smsesReady.Select(e => smsesDic[e]))
            {
                _smsQueue.QueueBackgroundWorkItem(sms);
            }

            // Queue web push notifications
            foreach (PushNotification pushNotification in pushesReady.Select(e => pushesDic[e]))
            {
                _pushQueue.QueueBackgroundWorkItem(pushNotification);
            }

            // A set of background jobs will asynchrously dequeue these notifications and dispatch them to the appropriate external services
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static EmailForSave ToEntity(Email e)
        {
            return new EmailForSave
            {
                ToEmail = e.ToEmail,
                Subject = e.Subject,
                Body = e.Body,
                Id = e.EmailId
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static Email FromEntity(EmailForSave e)
        {
            return new Email(e.ToEmail)
            {
                EmailId = e.Id,
                Subject = e.Subject,
                Body = e.Body
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static SmsMessageForSave ToEntity(SmsMessage e)
        {
            return new SmsMessageForSave
            {
                ToPhoneNumber = e.ToPhoneNumber,
                Message = e.Message,
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static SmsMessage FromEntity(SmsMessageForSave e)
        {
            return new SmsMessage(e.ToPhoneNumber, e.Message);
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static PushNotificationForSave ToEntity(PushNotification e)
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

        /// <summary>
        ///  Helper function
        /// </summary>
        public static PushNotification FromEntity(PushNotificationForSave e)
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

            return new PushNotification
            {
                Auth = e.Auth,
                Endpoint = e.Endpoint,
                P256dh = e.P256dh,
                Content = content,
            };
        }
    }
}
