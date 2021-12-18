using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "NotificationCommand", GroupName = "NotificationCommands")]
    public class NotificationCommand : EntityWithKey<int>
    {
        public int TemplateId { get; set; }

        public int? EntityId { get; set; }

        [Display(Name = "NotificationCommand_Caption")]
        public string Caption { get; set; }

        [Display(Name = "NotificationCommand_EmailSuccesses")]
        public int? EmailSuccesses { get; set; }

        [Display(Name = "NotificationCommand_EmailErrors")]
        public int? EmailErrors { get; set; }

        [Display(Name = "NotificationCommand_EmailTotal")]
        public int? EmailTotal { get; set; }

        [Display(Name = "NotificationCommand_SmsSuccesses")]
        public int? SmsSuccesses { get; set; }

        [Display(Name = "NotificationCommand_SmsErrors")]
        public int? SmsErrors { get; set; }

        [Display(Name = "NotificationCommand_SmsTotal")]
        public int? SmsTotal { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "NotificationCommand_Template")]
        [ForeignKey(nameof(TemplateId))]
        public NotificationTemplate Template { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}
