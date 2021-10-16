using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class NotificationTemplateSubscriberForSave : EntityWithKey<int>
    {
        [Display(Name = "NotificationTemplate_AddressType")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] {
                AddressTypes.User, AddressTypes.Text,
            },
        new string[] {
                "AddressType_User", "AddressType_Text"
        })]
        public string Usage { get; set; }

        [Display(Name = "NotificationTemplate_User")]
        public int? UserId { get; set; }

        [Display(Name = "NotificationTemplate_Phone")]
        [StringLength(1024)]
        public string Email { get; set; }

        [Display(Name = "NotificationTemplate_Email")]
        [StringLength(1024)]
        public string Phone { get; set; }
    }

    public class NotificationTemplateSubscriber : NotificationTemplateSubscriberForSave
    {
        [Required]
        public int? NotificationTemplateId { get; set; }

        [Required]
        public int LastNotificationCount { get; set; }

        public int LastNotificationHash { get; set; }

        // For Query

        [Display(Name = "NotificationTemplate_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }

    public static class AddressTypes
    {
        public const string User = nameof(User);
        public const string Text = nameof(Text);
    }
}
