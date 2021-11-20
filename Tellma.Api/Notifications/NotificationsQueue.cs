using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Notifications
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
        private readonly IBlobService _blobService;

        public NotificationsQueue(
            IApplicationRepositoryFactory repoFactory,
            IOptions<NotificationsOptions> options,
            EmailQueue emailQueue,
            SmsQueue smsQueue,
            PushNotificationQueue pushQueue,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IBlobService blobService)
        {
            _options = options.Value;
            _repoFactory = repoFactory;
            _emailQueue = emailQueue;
            _smsQueue = smsQueue;
            _pushQueue = pushQueue;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _blobService = blobService;
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
            var blobs = new List<(string name, byte[] content)>();
            foreach (var email in emails)
            {
                var (emailEntity, emailBlobs) = ToEntity(email);
                emailEntities.Add(emailEntity);
                blobs.AddRange(emailBlobs);

                var error = EmailValidation.Validate(email);
                if (error != null)
                {
                    emailEntity.State = EmailState.ValidationFailed;
                    emailEntity.ErrorMessage = error;

                    // The following ensures it will fit in the table
                    emailEntity.To = emailEntity.To?.Truncate(EmailValidation.MaximumEmailAddressLength);
                    emailEntity.Cc = emailEntity.Cc?.Truncate(EmailValidation.MaximumEmailAddressLength);
                    emailEntity.Bcc = emailEntity.Bcc?.Truncate(EmailValidation.MaximumEmailAddressLength);
                    emailEntity.Subject = emailEntity.Subject?.Truncate(EmailValidation.MaximumSubjectLength);
                    foreach (var att in emailEntity.Attachments)
                    {
                        if (string.IsNullOrWhiteSpace(att.Name))
                        {
                            att.Name = "(Missing Name)";
                        }
                        else
                        {
                            att.Name = att.Name?.Truncate(EmailValidation.MaximumAttchmentNameLength);
                        }
                    }
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
            using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

            // If the table already contains notifications that are 90% expired, do not queue the new notifications
            int expiryInSeconds = _options.PendingNotificationExpiryInSeconds * 9 / 10;

            // (2) Call the stored procedure
            // Persist the notifications in the database, the returned booleans will tell us which notifications we can queue immediately
            var repo = _repoFactory.GetRepository(tenantId);
            var (queueEmails, queueSmsMessages, queuePushNotifications) = await repo.Notifications_Enqueue(
                expiryInSeconds, emailEntities, smsEntities, pushEntities, cancellation);

            await _blobService.SaveBlobsAsync(tenantId, blobs);

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


        public async Task EnqueueEmails(int tenantId, List<EmailToSend> emails)
        {
            await Enqueue(tenantId: tenantId, emails: emails);
        }

        #region Helper Functions

        /// <summary>
        /// Helper function.
        /// </summary>
        public static (EmailForSave result, IEnumerable<(string name, byte[] content)> blobs) ToEntity(EmailToSend emailToSend)
        {
            var blobCount = 1 + emailToSend.Attachments.Count(); // To make sliiightly faster
            var blobs = new List<(string name, byte[] content)>(blobCount);

            string bodyBlobId = null;
            if (!string.IsNullOrWhiteSpace(emailToSend.Body))
            {
                bodyBlobId = Guid.NewGuid().ToString();
                var bodyBlobName = EmailUtil.EmailBodyBlobName(bodyBlobId);
                var bodyBlobContent = Encoding.UTF8.GetBytes(emailToSend.Body);

                blobs.Add((bodyBlobName, bodyBlobContent));
            }

            var emailForSave = new EmailForSave
            {
                To = string.Join(';', emailToSend.To ?? new List<string>()),
                Cc = string.Join(';', emailToSend.Cc ?? new List<string>()),
                Bcc = string.Join(';', emailToSend.Bcc ?? new List<string>()),
                Subject = emailToSend.Subject,
                BodyBlobId = bodyBlobId,
                Id = emailToSend.EmailId,
                State = EmailState.Scheduled,
                Attachments = new List<EmailAttachmentForSave>()
            };

            if (emailToSend.Attachments != null)
            {
                foreach (var att in emailToSend.Attachments)
                {
                    // If there is no content, then don't add the attachment
                    var contentBlobContent = att.Contents;
                    string contentBlobId = null;

                    if (contentBlobContent != null && contentBlobContent.Length > 0)
                    {
                        contentBlobId = Guid.NewGuid().ToString();
                        var contentBlobName = EmailUtil.EmailAttachmentBlobName(contentBlobId);
                        blobs.Add((contentBlobName, contentBlobContent));
                    }

                    emailForSave.Attachments.Add(new EmailAttachmentForSave
                    {
                        Name = att.Name,
                        ContentBlobId = contentBlobId
                    });
                }
            }

            return (emailForSave, blobs);
        }

        /// <summary>
        /// Helper function
        /// </summary>
        public static async Task<IEnumerable<EmailToSend>> FromEntities(int tenantId, IEnumerable<EmailForSave> emails, IBlobService blobService, CancellationToken cancellation)
        {
            var result = new List<EmailToSend>();

            foreach (var emailForSave in emails)
            {
                string body = null;
                if (!string.IsNullOrWhiteSpace(emailForSave.BodyBlobId))
                {
                    var bodyBlobName = EmailUtil.EmailBodyBlobName(emailForSave.BodyBlobId);
                    var bodyContent = bodyBlobName == null ? null : await blobService.LoadBlobAsync(tenantId, bodyBlobName, cancellation);
                    body = Encoding.UTF8.GetString(bodyContent);
                }

                var attachments = new List<EmailAttachmentToSend>();
                result.Add(new EmailToSend()
                {
                    To = emailForSave.To?.Split(';'),
                    Cc = emailForSave.Cc?.Split(';'),
                    Bcc = emailForSave.Bcc?.Split(';'),
                    EmailId = emailForSave.Id,
                    Subject = emailForSave.Subject,
                    Body = body,
                    Attachments = attachments,
                    TenantId = tenantId
                });

                foreach (var att in emailForSave?.Attachments ?? new List<EmailAttachmentForSave>())
                {
                    if (!string.IsNullOrWhiteSpace(att.ContentBlobId))
                    {
                        var attBlobName = EmailUtil.EmailAttachmentBlobName(att.ContentBlobId);
                        var attContent = await blobService.LoadBlobAsync(tenantId, attBlobName, cancellation);

                        attachments.Add(new EmailAttachmentToSend
                        {
                            Name = att.Name,
                            Contents = attContent
                        });
                    }
                }
            }

            return result;
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
                Content = JsonSerializer.Serialize(e.Content, _serializerOptions),

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
                content = JsonSerializer.Deserialize<PushContent>(e.Content, _serializerOptions);
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

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        #endregion
    }
}
