using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public class MessageTemplateSubscriberForSave : EntityWithKey<int>
    {
        [Display(Name = "NotificationTemplate_User")]
        [Required, ValidateRequired]
        public int? UserId { get; set; }
    }

    public class MessageTemplateSubscriber : MessageTemplateSubscriberForSave
    {
        [Required]
        public int? MessageTemplateId { get; set; }

        // For Query

        [Display(Name = "NotificationTemplate_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
