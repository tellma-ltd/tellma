using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "PrintingTemplate", GroupName = "PrintingTemplates")]
    public class PrintingTemplateForSave<TParameter> : EntityWithKey<int>
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

        [Display(Name = "PrintingTemplate_Usage")]
        [ChoiceList(new object[] { 
                TemplateUsages.FromMasterAndDetails, 
                TemplateUsages.FromDetails,
                TemplateUsages.FromReport,
                TemplateUsages.Standalone,
            }, 
            new string[] {
                TemplateUsageNames.FromMasterAndDetails,
                TemplateUsageNames.FromDetails,
                TemplateUsageNames.FromReport,
                TemplateUsageNames.Standalone 
            })]
        [StringLength(50)]
        public string Usage { get; set; }

        [Display(Name = "Template_Collection")]
        [StringLength(50)]
        [Required]
        public string Collection { get; set; }

        [Display(Name = "Template_DefinitionId")]
        public int? DefinitionId { get; set; }

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

        [Display(Name = "Template_Parameters")]
        [ForeignKey(nameof(PrintingTemplateParameter.PrintingTemplateId))]
        public List<TParameter> Parameters { get; set; }
    }

    public class PrintingTemplateForSave : PrintingTemplateForSave<PrintingTemplateParameterForSave>
    {
    }

    public class PrintingTemplate : PrintingTemplateForSave<PrintingTemplateParameter>
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

    public static class TemplateUsages
    {
        public const string FromMasterAndDetails = nameof(FromMasterAndDetails);
        public const string FromDetails = nameof(FromDetails);
        public const string FromReport = nameof(FromReport);
        public const string Standalone = nameof(Standalone);
    }

    public static class TemplateUsageNames
    {
        private const string _prefix = "Template_Usage_";

        public const string FromMasterAndDetails = _prefix + nameof(FromMasterAndDetails);
        public const string FromDetails = _prefix + nameof(FromDetails);
        public const string FromReport = _prefix + nameof(FromReport);
        public const string Standalone = _prefix + nameof(Standalone);
    }
}
