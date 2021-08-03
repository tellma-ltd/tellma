using System;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "SmsMessage", GroupName = "SmsMessages")]
    public class SmsMessageForSave : EntityWithKey<int>
    {
        [Display(Name = "SmsMessage_ToPhoneNumber")]
        public string ToPhoneNumber { get; set; }

        [Display(Name = "SmsMessage_Message")]
        public string Message { get; set; }

        [Display(Name = "State")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] {
            SmsState.Scheduled,
            SmsState.InProgress,
            SmsState.Dispatched,
            SmsState.Sent,
            SmsState.Delivered,
            SmsState.ValidationFailed,
            SmsState.DispatchFailed,
            SmsState.SendingFailed,
            SmsState.DeliveryFailed,
        }, new string[] {
            SmsStateName.Scheduled,
            SmsStateName.InProgress,
            SmsStateName.Dispatched,
            SmsStateName.Sent,
            SmsStateName.Delivered,
            SmsStateName.ValidationFailed,
            SmsStateName.DispatchFailed,
            SmsStateName.SendingFailed,
            SmsStateName.DeliveryFailed,
        })]
        public short? State { get; set; }

        [Display(Name = "SmsMessage_ErrorMessage")]
        public string ErrorMessage { get; set; }
    }

    public class SmsMessageForQuery : SmsMessageForSave
    {
        [Display(Name = "StateSince")]
        [Required]
        public DateTimeOffset? StateSince { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }
    }

    public static class SmsState
    {
        public static readonly short[] All = new short[] { Scheduled, InProgress, Dispatched, Sent, Delivered, ValidationFailed, DispatchFailed, SendingFailed, DeliveryFailed };

        /// <summary>
        /// Initial state of a new SMS.
        /// </summary>
        public const short Scheduled = 0;

        /// <summary>
        /// Currently being sent to the SMS service (e.g. Twilio).
        /// </summary>
        public const short InProgress = 1;

        /// <summary>
        /// Sent to SMS service (e.g. Twilio), pending delivery to the carrier network.
        /// </summary>
        public const short Dispatched = 2;

        /// <summary>
        /// Sent to carrier network, pending delivery to end device
        /// (Due to the nature of the carrier networks, even if the SMS gets delivered to the
        /// end device, we may never be notified and the state will remain stuck here forever,
        /// therefore it advised that a Sent state older than 4 hours to be treated as final).
        /// </summary>
        public const short Sent = 3;

        /// <summary>
        /// Delivered to the end device
        /// </summary>
        public const short Delivered = 4;

        /// <summary>
        /// Failed the validation locally, (e.g. invalid phone number, or text too long).
        /// </summary>
        public const short ValidationFailed = -1;

        /// <summary>
        /// Failed to send to the SMS service (e.g. Twilio).
        /// </summary>
        public const short DispatchFailed = -2;

        /// <summary>
        /// Sent to SMS service (e.g. Twilio), but failed to send to the carrier network.
        /// </summary>
        public const short SendingFailed = -3;

        /// <summary>
        /// Sent to the carrier network, but could not be delivered to the end device.
        /// </summary>
        public const short DeliveryFailed = -4;
    }

    public static class SmsStateName
    {
        private const string _generic = "Notification_State_";
        private const string _prefix = "SmsMessage_State_";

        public const string Scheduled = _generic + "0";
        
        public const string InProgress = _generic + "1";
        public const string Dispatched = _generic + "2";
        public const string Sent = _prefix + "3";
        public const string Delivered = _prefix + "4";

        public const string ValidationFailed = _generic + "minus_1";
        public const string DispatchFailed = _generic + "minus_2";
        public const string SendingFailed = _prefix + "minus_3";
        public const string DeliveryFailed = _prefix + "minus_4";
    }
}
