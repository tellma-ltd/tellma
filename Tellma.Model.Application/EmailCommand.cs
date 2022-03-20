using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "EmailCommand", GroupName = "EmailCommands")]
    public class EmailCommand : EntityWithKey<int>
    {
        public int TemplateId { get; set; }

        public int? EntityId { get; set; }

        [Display(Name = "NotificationCommand_Caption")]
        public string Caption { get; set; }

        [Display(Name = "NotificationCommand_Successes")]
        public int? Successes { get; set; }

        [Display(Name = "NotificationCommand_Errors")]
        public int? Errors { get; set; }

        [Display(Name = "NotificationCommand_Total")]
        public int? Total { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "NotificationCommand_Template")]
        [ForeignKey(nameof(TemplateId))]
        public EmailTemplate Template { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }
    }
}
