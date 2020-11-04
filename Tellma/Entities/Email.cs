using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Email", Plural = "Emails")]
    public class EmailForSave : EntityWithKey<int>
    {
        [Display(Name = "Email_ToEmail")]
        public string ToEmail { get; set; }

        [Display(Name = "Email_Subject")]
        public string Subject { get; set; }

        [Display(Name = "Email_Body")]
        public string Body { get; set; }

        [Display(Name = "State")]
        [AlwaysAccessible]
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
    }

    public class EmailForQuery : EmailForSave
    {
        [Display(Name = "StateSince")]
        public DateTimeOffset? StateSince { get; set; }

        [Display(Name = "Email_DeliveredAt")]
        public DateTimeOffset? DeliveredAt { get; set; }

        [Display(Name = "Email_OpenedAt")]
        public DateTimeOffset? OpenedAt { get; set; }

        [Display(Name = "CreatedAt")]
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
