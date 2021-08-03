using System;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "PushNotification", GroupName = "PushNotifications")]
    public class PushNotificationForSave : EntityWithKey<int>
    {
        [Required]
        public string Endpoint { get; set; }
        
        [Required]
        public string P256dh { get; set; }

        [Required]
        public string Auth { get; set; }

        [Display(Name = "PushNotification_Title")]
        public string Title { get; set; } // Also contained in Content JSON

        [Display(Name = "PushNotification_Body")]
        public string Body { get; set; } // Also contained in Content JSON

        [Required]
        public string Content { get; set; } // JSON

        [Display(Name = "State")]
        [Required]
        [ChoiceList(new object[] {
            PushState.Scheduled,
            PushState.InProgress,
            PushState.Dispatched,
            PushState.ValidationFailed,
            PushState.DispatchFailed,
        }, new string[] {
            PushStateName.Scheduled,
            PushStateName.InProgress,
            PushStateName.Dispatched,
            PushStateName.ValidationFailed,
            PushStateName.DispatchFailed
        })]
        public short? State { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class PushNotificationForQuery : PushNotificationForSave
    {
        public DateTimeOffset? StateSince { get; set; }
    }

    public static class PushState
    {
        public static readonly short[] All = new short[] { Scheduled, InProgress, Dispatched, ValidationFailed, DispatchFailed };

        /// <summary>
        /// Initial state of a new Push notification
        /// </summary>
        public const short Scheduled = 0;

        /// <summary>
        /// Currently being sent to the user
        /// </summary>
        public const short InProgress = 1;

        /// <summary>
        /// Sent to the user
        /// </summary>
        public const short Dispatched = 2;

        /// <summary>
        /// Failed the validation locally, (e.g. invalid title too long)
        /// </summary>
        public const short ValidationFailed = -1;

        /// <summary>
        /// Failed to send to the Push notification service
        /// </summary>
        public const short DispatchFailed = -2;
    }

    public static class PushStateName
    {
        private const string _generic = "Notification_State_";

        public const string Scheduled = _generic + "0";

        public const string InProgress = _generic + "1";
        public const string Dispatched = _generic + "2";

        public const string ValidationFailed = _generic + "minus_1";
        public const string DispatchFailed = _generic + "minus_2";
    }
}
