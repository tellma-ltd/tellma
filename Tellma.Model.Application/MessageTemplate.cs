using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "MessageTemplate", GroupName = "MessageTemplates")]
    public class MessageTemplateForSave<TParameter, TSubscriber> : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description3 { get; set; }

        [Display(Name = "NotificationTemplate_Trigger")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { Triggers.Automatic, Triggers.Manual },
            new string[] { "Trigger_Automatic", "Trigger_Manual" })]
        public string Trigger { get; set; }

        [Display(Name = "NotificationTemplate_Cardinality")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { Cardinalities.Single, Cardinalities.Multiple },
            new string[] { "Cardinality_Single", "Cardinality_Multiple" })]
        public string Cardinality { get; set; }

        [Display(Name = "NotificationTemplate_ListExpression")]
        [StringLength(1024 * 255)]
        public string ListExpression { get; set; }

        [Display(Name = "NotificationTemplate_Schedule")]
        [StringLength(1024)]
        public string Schedule { get; set; }

        [Display(Name = "NotificationTemplate_ConditionExpression")]
        [StringLength(1024)]
        public string ConditionExpression { get; set; }

        [Display(Name = "NotificationTemplate_Renotify")]
        [Required]
        public bool? Renotify { get; set; }

        [Display(Name = "NotificationTemplate_Version")]
        [StringLength(1024)]
        public string Version { get; set; }

        [Display(Name = "Template_Usage")]
        [ChoiceList(new object[] {
                TemplateUsages.FromSearchAndDetails,
                TemplateUsages.FromDetails,
            },
        new string[] {
                    TemplateUsageNames.FromSearchAndDetails,
                    TemplateUsageNames.FromDetails,
        })]
        public string Usage { get; set; }

        [Display(Name = "Template_Collection")]
        [StringLength(50)]
        [Required]
        public string Collection { get; set; }

        [Display(Name = "Template_DefinitionId")]
        public int? DefinitionId { get; set; }

        [Display(Name = "NotificationTemplate_PhoneNumber")]
        [StringLength(1024)]
        public string PhoneNumber { get; set; }

        [Display(Name = "NotificationTemplate_Content")]
        [StringLength(1024 * 255)]
        public string Content { get; set; }

        [Display(Name = "NotificationTemplate_Caption")]
        [Required, ValidateRequired]
        [StringLength(1024)]
        public string Caption { get; set; }

        [Display(Name = "Template_IsDeployed")]
        [Required]
        public bool? IsDeployed { get; set; }

        [Display(Name = "Template_Parameters")]
        [ForeignKey(nameof(NotificationTemplateParameter.NotificationTemplateId))]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "NotificationTemplate_Subscribers")]
        [ForeignKey(nameof(NotificationTemplateSubscriber.NotificationTemplateId))]
        public List<TSubscriber> Subscribers { get; set; }
    }

    public class MessageTemplateForSave : MessageTemplateForSave<MessageTemplateParameterForSave, MessageTemplateSubscriberForSave>
    {
    }

    public class MessageTemplate : MessageTemplateForSave<MessageTemplateParameter, MessageTemplateSubscriber>
    {
        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
