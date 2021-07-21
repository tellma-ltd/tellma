using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Notifications
{
    public class NotificationsQueue
    {
        private readonly NotificationsOptions _options;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly EmailQueue _emailQueue;
        private readonly SmsQueue _smsQueue;
        private readonly PushNotificationQueue _pushQueue;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public NotificationsQueue(
            IApplicationRepositoryFactory repoFactory, 
            IOptions<NotificationsOptions> options, 
            EmailQueue emailQueue, 
            SmsQueue smsQueue, 
            PushNotificationQueue pushQueue,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _options = options.Value;
            _repoFactory = repoFactory;
            _emailQueue = emailQueue;
            _smsQueue = smsQueue;
            _pushQueue = pushQueue;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        public bool EmailEnabled => _emailSender.IsEnabled;
        public bool SmsEnabled => _smsSender.IsEnabled;
        public static bool PushEnabled => false; // TODO

        /// <summary>
        /// Queues the notifications in the database and if the database table was clear it queues them immediately for processing.<br/>
        /// If the database contains stale notifications, then do not queue the new ones immediately. Instead wait for <see cref="SmsPollingJob"/>
        /// to pick them out in order and schedule them, this prevent notifications being dispatched grossly out of order.
        /// </summary>
        public async Task Enqueue(
            int tenantId,
            List<EmailToSend> emails = null,
            List<SmsToSend> smsMessages = null,
            List<PushToSend> pushNotifications = null,
            CancellationToken cancellation = default)
        {
            // (1) Map notifications to Entities and validate them
            // Email
            emails ??= new List<EmailToSend>();
            if (emails.Count > 0 && !EmailEnabled)
            {
                // Developer mistake
                throw new InvalidOperationException("Attempt to Enqueue emails while email is disabled in this installation.");
            }

            var validEmails = new List<EmailToSend>(emails.Count);
            var emailEntities = new List<EmailForSave>(emails.Count);
            foreach (var email in emails)
            {
                var emailEntity = ToEntity(email);
                emailEntities.Add(emailEntity);

                var error = EmailValidation.Validate(email);
                if (error != null)
                {
                    emailEntity.State = EmailState.ValidationFailed;
                    emailEntity.ErrorMessage = error;
                }
                else
                {
                    validEmails.Add(email);
                }
            }

            // SMS
            smsMessages ??= new List<SmsToSend>();
            if (smsMessages.Count > 0 && !SmsEnabled)
            {
                // Developer mistake
                throw new InvalidOperationException("Attempt to Enqueue SMS messages while SMS is disabled in this installation.");
            }
            var validSmsMessages = new List<SmsToSend>(smsMessages.Count);
            var smsEntities = new List<SmsMessageForSave>(smsMessages.Count);
            foreach (var sms in smsMessages)
            {
                var smsEntity = ToEntity(sms);
                smsEntities.Add(smsEntity);

                var error = SmsValidation.Validate(sms);
                if (error != null)
                {
                    smsEntity.State = SmsState.ValidationFailed;
                    smsEntity.ErrorMessage = error;
                }
                else
                {
                    validSmsMessages.Add(sms);
                }
            }

            // Push
            pushNotifications ??= new List<PushToSend>();
            if (pushNotifications.Count > 0 && !PushEnabled)
            {
                // Developer mistake
                throw new InvalidOperationException("Attempt to Enqueue Push notifications while Push is disabled in this installation.");
            }
            var validPushNotifications = new List<PushToSend>(pushNotifications.Count);
            var pushEntities = new List<PushNotificationForSave>(pushNotifications.Count);
            // TODO

            // Start a serializable transaction
            using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

            // If the table already contains notifications that are 90% expired, do not queue the new notifications
            int expiryInSeconds = _options.PendingNotificationExpiryInSeconds * 9 / 10;

            // (2) Call the stored procedure
            // Persist the notifications in the database, the returned booleans will tell us which notifications we can queue immediately
            var repo = _repoFactory.GetRepository(tenantId);
            var (queueEmails, queueSmsMessages, queuePushNotifications) = await repo.Notifications_Enqueue(
                expiryInSeconds, emailEntities, smsEntities, pushEntities, cancellation);

            // (3) Map the Ids back to the DTOs and queue valid notifications
            // Email
            if (queueEmails && validEmails.Any())
            {
                // Map the Ids
                for (int i = 0; i < emailEntities.Count; i++)
                {
                    var entity = emailEntities[i];
                    var dto = emails[i];
                    dto.EmailId = entity.Id;
                    dto.TenantId = tenantId;
                }

                // Queue (Emails are queued in bulk unlike SMS and Push)
                _emailQueue.QueueBackgroundWorkItem(validEmails);
            }

            // SMS
            if (queueSmsMessages && validSmsMessages.Any())
            {
                // Map the Ids
                for (int i = 0; i < smsEntities.Count; i++)
                {
                    var smsEntity = smsEntities[i];
                    var sms = smsMessages[i];
                    sms.MessageId = smsEntity.Id;
                    sms.TenantId = tenantId;
                }

                // Queue
                _smsQueue.QueueAllBackgroundWorkItems(validSmsMessages);
            }

            // Push
            if (queuePushNotifications && validPushNotifications.Any())
            {
                // Map the Ids
                for (int i = 0; i < smsEntities.Count; i++)
                {
                    var entity = pushEntities[i];
                    var dto = pushNotifications[i];
                    dto.PushId = entity.Id;
                    dto.TenantId = tenantId;
                }

                // Queue
                _pushQueue.QueueAllBackgroundWorkItems(validPushNotifications);
            }

            trx.Complete();
        }

        #region Helper Functions

        /// <summary>
        /// Helper function
        /// </summary>
        public static EmailForSave ToEntity(EmailToSend e)
        {
            return new EmailForSave
            {
                ToEmail = e.ToEmail,
                Subject = e.Subject,
                Body = e.Body,
                Id = e.EmailId,
                State = EmailState.Scheduled
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static EmailToSend FromEntity(EmailForSave e, int tenantId)
        {
            return new EmailToSend(e.ToEmail)
            {
                EmailId = e.Id,
                Subject = e.Subject,
                Body = e.Body,
                TenantId = tenantId
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static SmsMessageForSave ToEntity(SmsToSend e)
        {
            return new SmsMessageForSave
            {
                ToPhoneNumber = e.ToPhoneNumber,
                Message = e.Message,
                State = SmsState.Scheduled
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static SmsToSend FromEntity(SmsMessageForSave e, int tenantId)
        {
            return new SmsToSend(e.ToPhoneNumber, e.Message)
            {
                MessageId = e.Id,
                TenantId = tenantId
            };
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static PushNotificationForSave ToEntity(PushToSend e)
        {
            return new PushNotificationForSave
            {
                Auth = e.Auth,
                Endpoint = e.Endpoint,
                P256dh = e.P256dh,

                Title = e.Content?.Title,
                Body = e.Content?.Body,
                Content = JsonConvert.SerializeObject(e.Content),

                State = PushState.Scheduled
            };
        }

        /// <summary>
        ///  Helper function (may return null if the JSON content could not be parsed)
        /// </summary>
        public static PushToSend FromEntity(PushNotificationForSave e, int tenantId)
        {
            PushContent content;
            try
            {
                content = JsonConvert.DeserializeObject<PushContent>(e.Content);
            }
            catch
            {
                return null; // Should not happen in theory but just in case
            }

            return new PushToSend
            {
                Auth = e.Auth,
                Endpoint = e.Endpoint,
                P256dh = e.P256dh,
                Content = content,

                PushId = e.Id,
                TenantId = tenantId
            };
        }

        #endregion
    }
}
