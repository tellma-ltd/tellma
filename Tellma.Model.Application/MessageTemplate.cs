﻿using System;
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

        [Display(Name = "NotificationTemplate_PreventRenotify")]
        [Required]
        public bool? PreventRenotify { get; set; }

        [Display(Name = "NotificationTemplate_Version")]
        [StringLength(1024)]
        public string Version { get; set; }

        [Display(Name = "Template_Usage")]
        [ChoiceList(new object[] {
                TemplateUsages.FromSearchAndDetails,
                TemplateUsages.FromDetails,
                TemplateUsages.Standalone,
            },
        new string[] {
                    TemplateUsageNames.FromSearchAndDetails,
                    TemplateUsageNames.FromDetails,
                    TemplateUsageNames.Standalone,
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

        [Display(Name = "MainMenuSection")]
        [ChoiceList(new object[] {
                "Mail",
                "Financials",
                "Cash",
                "FixedAssets",
                "Inventory",
                "Production",
                "Purchasing",
                "Marketing",
                "Sales",
                "HumanCapital",
                "Payroll",
                "Investments",
                "Maintenance",
                "Administration",
                "Security",
                "Studio",
                "Help" },
            new string[] {
                "Menu_Mail",
                "Menu_Financials",
                "Menu_Cash",
                "Menu_FixedAssets",
                "Menu_Inventory",
                "Menu_Production",
                "Menu_Purchasing",
                "Menu_Marketing",
                "Menu_Sales",
                "Menu_HumanCapital",
                "Menu_Payroll",
                "Menu_Investments",
                "Menu_Maintenance",
                "Menu_Administration",
                "Menu_Security",
                "Menu_Studio",
                "Menu_Help"
            })]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "Template_Parameters")]
        [ForeignKey(nameof(MessageTemplateParameter.MessageTemplateId))]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "NotificationTemplate_Subscribers")]
        [ForeignKey(nameof(MessageTemplateSubscriber.MessageTemplateId))]
        public List<TSubscriber> Subscribers { get; set; }
    }

    public class MessageTemplateForSave : MessageTemplateForSave<MessageTemplateParameterForSave, MessageTemplateSubscriberForSave>
    {
    }

    public class MessageTemplate : MessageTemplateForSave<MessageTemplateParameter, MessageTemplateSubscriber>
    {
        public DateTimeOffset? LastExecuted { get; set; }
        public bool? IsError { get; set; }

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
