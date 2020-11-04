using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly JobsOptions _options;
        private readonly ApplicationRepositoryLite _repo;
        private readonly EmailQueue _emailQueue;
        private readonly SmsQueue _smsQueue;
        private readonly PushNotificationQueue _pushQueue;

        public ExternalNotificationsService(ApplicationRepositoryLite repo, IOptions<JobsOptions> options, EmailQueue emailQueue, SmsQueue smsQueue, PushNotificationQueue pushQueue)
        {
            _repo = repo;
            _options = options.Value;
            _emailQueue = emailQueue;
            _smsQueue = smsQueue;
            _pushQueue = pushQueue;
        }

        /// <summary>
        /// Queues the notifications in the database and if the database table is clearn queues them immediately for processing.
        /// If the database contains stale notifications, then do not queue the new ones immediately. Instead wait for <see cref="SmsPollingJob"/>
        /// to pick them out in order and schedule them, this prevent notifications being dispatched grossly out of order
        /// </summary>
        public async Task Enqueue(
            int tenantId,
            List<Email> emails = null,
            List<SmsMessage> smsMessages = null,
            List<PushNotification> pushNotifications = null,
            CancellationToken cancellation = default)
        {
            // (1) Map notifications to Entities and validate them
            // Email
            emails ??= new List<Email>();
            var validEmails = new List<Email>(emails.Count);
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
            smsMessages ??= new List<SmsMessage>();
            var validSmsMessages = new List<SmsMessage>(smsMessages.Count);
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
            pushNotifications ??= new List<PushNotification>();
            var validPushNotifications = new List<PushNotification>(pushNotifications.Count);
            var pushEntities = new List<PushNotificationForSave>(pushNotifications.Count);
            // TODO

            // Start a serializable transaction
            using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

            // If the table already contains notifications that are 90% expired, do not queue the new notifications
            int expiryInSeconds = _options.PendingNotificationExpiryInSeconds * 9 / 10;

            // (2) Call the stored procedure
            // Persist the notifications in the database, the returned booleans will tell us which notifications we can queue immediately
            var (queueEmails, queueSmsMessages, queuePushNotifications) = await _repo.Notifications_Enqueue(
                tenantId, expiryInSeconds, emailEntities, smsEntities, pushEntities, cancellation);

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
        public static EmailForSave ToEntity(Email e)
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
        public static Email FromEntity(EmailForSave e, int tenantId)
        {
            return new Email(e.ToEmail)
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
        public static SmsMessageForSave ToEntity(SmsMessage e)
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
        public static SmsMessage FromEntity(SmsMessageForSave e, int tenantId)
        {
            return new SmsMessage(e.ToPhoneNumber, e.Message)
            {
                MessageId = e.Id,
                TenantId = tenantId
            };
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

                State = PushState.Scheduled
            };
        }

        /// <summary>
        ///  Helper function (may return null if the JSON content could not be parsed)
        /// </summary>
        public static PushNotification FromEntity(PushNotificationForSave e, int tenantId)
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

                PushId = e.Id,
                TenantId = tenantId
            };
        }

        #endregion
    }
}
