using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "PrintingTemplate", GroupName = "PrintingTemplates")]
    public class PrintingTemplateForSave<TParameter, TRole> : EntityWithKey<int>
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

        [Display(Name = "PrintingTemplate_Context")]
        [StringLength(1024)]
        public string Context { get; set; }

        [Display(Name = "Template_Usage")]
        [ChoiceList(new object[] { 
                TemplateUsages.FromSearchAndDetails, 
                TemplateUsages.FromDetails,
                TemplateUsages.FromReport,
                TemplateUsages.Standalone,
            }, 
            new string[] {
                TemplateUsageNames.FromSearchAndDetails,
                TemplateUsageNames.FromDetails,
                TemplateUsageNames.FromReport,
                TemplateUsageNames.Standalone 
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

        [Display(Name = "PrintingTemplate_Supports")]
        [Required]
        public bool? SupportsPrimaryLanguage { get; set; }

        [Display(Name = "PrintingTemplate_Supports")]
        [Required]
        public bool? SupportsSecondaryLanguage { get; set; }

        [Display(Name = "PrintingTemplate_Supports")]
        [Required]
        public bool? SupportsTernaryLanguage { get; set; }

        [Display(Name = "PrintingTemplate_DownloadName")]
        [StringLength(1024)]
        public string DownloadName { get; set; }

        [Display(Name = "Template_Body")]
        [StringLength(1024 * 255)]
        public string Body { get; set; }

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
        [ForeignKey(nameof(PrintingTemplateParameter.PrintingTemplateId))]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "Definition_Roles")]
        [ForeignKey(nameof(PrintingTemplateRole.PrintingTemplateId))]
        public List<TRole> Roles { get; set; }
    }

    public class PrintingTemplateForSave : PrintingTemplateForSave<PrintingTemplateParameterForSave, PrintingTemplateRoleForSave>
    {
    }

    public class PrintingTemplate : PrintingTemplateForSave<PrintingTemplateParameter, PrintingTemplateRole>
    {
        [Display(Name = "Definition_ShowInMainMenu")]
        public bool? ShowInMainMenu { get; set; }

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

    public static class TemplateUsages
    {
        public const string FromSearchAndDetails = nameof(FromSearchAndDetails);
        public const string FromDetails = nameof(FromDetails);
        public const string FromReport = nameof(FromReport);
        public const string Standalone = nameof(Standalone);
    }

    public static class TemplateUsageNames
    {
        private const string _prefix = "Template_Usage_";

        public const string FromSearchAndDetails = _prefix + nameof(FromSearchAndDetails);
        public const string FromDetails = _prefix + nameof(FromDetails);
        public const string FromReport = _prefix + nameof(FromReport);
        public const string Standalone = _prefix + nameof(Standalone);
    }
}
