using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Email", GroupName = "Emails")]
    public class EmailForSave<TAttachment> : EntityWithKey<int>
    {
        [Display(Name = "Email_To")]
        public string To { get; set; }

        [Display(Name = "Email_Cc")]
        public string Cc { get; set; }

        [Display(Name = "Email_Bcc")]
        public string Bcc { get; set; }

        [Display(Name = "Email_Subject")]
        public string Subject { get; set; }

        [Display(Name = "Email_Body")]
        public string BodyBlobId { get; set; }

        [Display(Name = "State")]
        [Required]
        [ChoiceList(new object[] {
            EmailState.Scheduled,
            EmailState.InProgress,
            EmailState.Dispatched,
            EmailState.Delivered,
            EmailState.Opened,
            EmailState.Clicked,
            EmailState.ValidationFailed,
            EmailState.DispatchFailed,
            EmailState.DeliveryFailed,
            EmailState.ReportedSpam,
        }, new string[] {
            EmailStateName.Scheduled,
            EmailStateName.InProgress,
            EmailStateName.Dispatched,
            EmailStateName.Delivered,
            EmailStateName.Opened,
            EmailStateName.Clicked,
            EmailStateName.ValidationFailed,
            EmailStateName.DispatchFailed,
            EmailStateName.DeliveryFailed,
            EmailStateName.ReportedSpam,
        })]
        public short? State { get; set; }

        [Display(Name = "Email_ErrorMessage")]
        public string ErrorMessage { get; set; }


        [Display(Name = "Email_Attachments")]
        [ForeignKey(nameof(EmailAttachment.EmailId))]
        public List<TAttachment> Attachments { get; set; }
    }

    public class EmailForSave : EmailForSave<EmailAttachmentForSave>
    {

    }

    public class EmailForQuery : EmailForSave<EmailAttachment>
    {
        [Display(Name = "StateSince")]
        [Required]
        public DateTimeOffset? StateSince { get; set; }

        [Display(Name = "Email_DeliveredAt")]
        public DateTimeOffset? DeliveredAt { get; set; }

        [Display(Name = "Email_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }
    }

    public static class EmailState
    {
        public static readonly short[] All = new short[] { Scheduled, InProgress, Dispatched, ValidationFailed, DispatchFailed };

        /// <summary>
        /// Initial state of a new email
        /// </summary>
        public const short Scheduled = 0;

        /// <summary>
        /// Currently being sent to the email service (e.g. SendGrid)
        /// </summary>
        public const short InProgress = 1;

        /// <summary>
        /// Sent to email service (e.g. SendGrid)
        /// </summary>
        public const short Dispatched = 2;

        /// <summary>
        /// Sent to the recipient server
        /// </summary>
        public const short Delivered = 3;

        /// <summary>
        /// The user opened the email
        /// </summary>
        public const short Opened = 4;

        /// <summary>
        /// The user clicked a link in the email
        /// </summary>
        public const short Clicked = 5;

        /// <summary>
        /// Failed the validation locally, (e.g. invalid email, or body too long)
        /// </summary>
        public const short ValidationFailed = -1;

        /// <summary>
        /// Failed to send to the email service (e.g. SendGrid)
        /// </summary>
        public const short DispatchFailed = -2;

        /// <summary>
        /// Bounced back from the recipient server
        /// </summary>
        public const short DeliveryFailed = -3;

        /// <summary>
        /// The user reported the email as spam
        /// </summary>
        public const short ReportedSpam = -4;
    }

    public static class EmailStateName
    {
        private const string _generic = "Notification_State_";
        private const string _email = "Email_State_";

        public const string Scheduled = _generic + "0";
        public const string InProgress = _generic + "1";
        public const string Dispatched = _generic + "2";
        public const string Delivered = _email + "3";
        public const string Opened = _email + "4";
        public const string Clicked = _email + "5";

        public const string ValidationFailed = _generic + "minus_1";
        public const string DispatchFailed = _generic + "minus_2";
        public const string DeliveryFailed = _email + "minus_3";
        public const string ReportedSpam = _email + "minus_4";
    }
}
