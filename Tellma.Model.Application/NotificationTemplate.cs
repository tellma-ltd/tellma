using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;
namespace Tellma.Model.Application
{
    [Display(Name = "NotificationTemplate", GroupName = "NotificationTemplates")]
    public class NotificationTemplateForSave<TParameter, TAttachment, TSubscriber> : EntityWithKey<int>
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

        [Display(Name = "NotificationTemplate_Channel")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { Channels.Email, Channels.Sms },
            new string[] { "Channel_Email", "Channel_Sms" })]
        public string Channel { get; set; }

        [Display(Name = "NotificationTemplate_Trigger")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { Triggers.Automatic, Triggers.Manual },
            new string[] { "Trigger_Automatic", "Trigger_Manual" })]
        public string Trigger { get; set; }

        [Display(Name = "NotificationTemplate_Cardinality")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { Cardinalities.Single, Cardinalities.Bulk},
            new string[] { "Cardinality_Single", "Cardinality_Bulk" })]
        public string Cardinality { get; set; }

        [Display(Name = "NotificationTemplate_ListExpression")]
        [StringLength(1024)]
        public string ListExpression { get; set; }

        [Display(Name = "NotificationTemplate_Schedule")]
        [StringLength(1024)]
        public string Schedule { get; set; }

        [Display(Name = "NotificationTemplate_ConditionExpression")]
        [StringLength(1024)]
        public string ConditionExpression { get; set; }

        [Display(Name = "NotificationTemplate_MaximumRenotify")]
        public int? MaximumRenotify { get; set; }

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

        [Display(Name = "Template_ReportDefinitionId")]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "NotificationTemplate_Subject")]
        [StringLength(1024)]
        public string Subject { get; set; }

        [Display(Name = "Template_Body")]
        [StringLength(1024 * 255)]
        public string Body { get; set; }

        [Display(Name = "NotificationTemplate_AddressExpression")]
        [StringLength(1024)]
        public string AddressExpression { get; set; }

        [Display(Name = "NotificationTemplate_Caption")]
        [StringLength(1024)]
        public string Caption { get; set; }

        [Display(Name = "Template_IsDeployed")]
        [Required]
        public bool? IsDeployed { get; set; }

        [Display(Name = "Template_Parameters")]
        [ForeignKey(nameof(NotificationTemplateParameter.NotificationTemplateId))]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "NotificationTemplate_Attachments")]
        [ForeignKey(nameof(NotificationTemplateAttachment.NotificationTemplateId))]
        public List<TAttachment> Attachments { get; set; }

        [Display(Name = "NotificationTemplate_Subscribers")]
        [ForeignKey(nameof(NotificationTemplateSubscriber.NotificationTemplateId))]
        public List<TSubscriber> Subscribers { get; set; }
    }


    public class NotificationTemplateForSave : NotificationTemplateForSave<NotificationTemplateParameterForSave, NotificationTemplateAttachmentForSave, NotificationTemplateSubscriberForSave>
    {
    }

    public class NotificationTemplate : NotificationTemplateForSave<NotificationTemplateParameter, NotificationTemplateAttachment, NotificationTemplateSubscriber>
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

        [Display(Name = "Template_ReportDefinitionId")]
        [ForeignKey(nameof(ReportDefinitionId))]
        public ReportDefinition ReportDefinition { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }

    public static class Channels
    {
        public const string Email = nameof(Email);
        public const string Sms = nameof(Sms);
    }

    public static class Triggers
    {
        public const string Automatic = nameof(Automatic);
        public const string Manual = nameof(Manual);
    }

    public static class Cardinalities
    {
        public const string Single = nameof(Single);
        public const string Bulk = nameof(Bulk);
    }
}
