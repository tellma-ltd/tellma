using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Message", GroupName = "Messages")]
    public class MessageForSave : EntityWithKey<int>
    {
        [Display(Name = "Message_PhoneNumber")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Message_Content")]
        public string Content { get; set; }

        [Display(Name = "State")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] {
            MessageState.Scheduled,
            MessageState.InProgress,
            MessageState.Dispatched,
            MessageState.Sent,
            MessageState.Delivered,
            MessageState.ValidationFailed,
            MessageState.DispatchFailed,
            MessageState.SendingFailed,
            MessageState.DeliveryFailed,
        }, new string[] {
            MessageStateName.Scheduled,
            MessageStateName.InProgress,
            MessageStateName.Dispatched,
            MessageStateName.Sent,
            MessageStateName.Delivered,
            MessageStateName.ValidationFailed,
            MessageStateName.DispatchFailed,
            MessageStateName.SendingFailed,
            MessageStateName.DeliveryFailed,
        })]
        public short? State { get; set; }

        [Display(Name = "Message_ErrorMessage")]
        public string ErrorMessage { get; set; }
    }

    public class MessageForQuery : MessageForSave
    {
        [Display(Name = "Notification_Command")]
        public int? CommandId { get; set; }

        [Display(Name = "Notification_Command")]
        [ForeignKey(nameof(CommandId))]
        public NotificationCommand Command { get; set; }

        [Display(Name = "StateSince")]
        [Required]
        public DateTimeOffset? StateSince { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }
    }

    public static class MessageState
    {
        public static readonly short[] All = new short[] { Scheduled, InProgress, Dispatched, Sent, Delivered, ValidationFailed, DispatchFailed, SendingFailed, DeliveryFailed };

        /// <summary>
        /// Initial state of a new Message.
        /// </summary>
        public const short Scheduled = 0;

        /// <summary>
        /// Currently being sent to the Messaging service (e.g. Twilio).
        /// </summary>
        public const short InProgress = 1;

        /// <summary>
        /// Sent to Messaging service (e.g. Twilio), pending delivery to the carrier network.
        /// </summary>
        public const short Dispatched = 2;

        /// <summary>
        /// Sent to carrier network, pending delivery to end device
        /// (Due to the nature of the carrier networks, even if the Message gets delivered to the
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
        /// Failed to send to the Messaging service (e.g. Twilio).
        /// </summary>
        public const short DispatchFailed = -2;

        /// <summary>
        /// Sent to Messaging service (e.g. Twilio), but failed to send to the carrier network.
        /// </summary>
        public const short SendingFailed = -3;

        /// <summary>
        /// Sent to the carrier network, but could not be delivered to the end device.
        /// </summary>
        public const short DeliveryFailed = -4;
    }

    public static class MessageStateName
    {
        private const string _generic = "Notification_State_";
        private const string _prefix = "Message_State_";

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
