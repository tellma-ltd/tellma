using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class EmailTemplateSubscriberForSave : EntityWithKey<int>
    {
        [Display(Name = "NotificationTemplate_User")]
        [Required, ValidateRequired]
        public int? UserId { get; set; }
    }

    public class EmailTemplateSubscriber : EmailTemplateSubscriberForSave
    {
        [Required]
        public int? EmailTemplateId { get; set; }

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
