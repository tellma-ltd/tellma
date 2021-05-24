using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "MarkupTemplate", GroupName = "MarkupTemplates")]
    public class MarkupTemplateForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required]
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

        [Display(Name = "MarkupTemplate_Usage")]
        [ChoiceList(new object[] { 
                MarkupTemplateConst.QueryByFilter, 
                MarkupTemplateConst.QueryById }, 
            new string[] { 
                "MarkupTemplate_Usage_QueryByFilter", 
                "MarkupTemplate_Usage_QueryById" })]
        [StringLength(50)]
        public string Usage { get; set; }

        [Display(Name = "MarkupTemplate_Collection")]
        [StringLength(50)]
        [Required]
        public string Collection { get; set; }

        [Display(Name = "MarkupTemplate_DefinitionId")]
        public int? DefinitionId { get; set; }

        [Display(Name = "MarkupTemplate_MarkupLanguage")]
        [Required]
        [ChoiceList(new object[] { "text/html" }, new string[] { "HTML" })]
        [StringLength(255)]
        public string MarkupLanguage { get; set; } // HTML, Markdown, XML, etc…

        [Display(Name = "MarkupTemplate_Supports")]
        [Required]
        public bool? SupportsPrimaryLanguage { get; set; }

        [Display(Name = "MarkupTemplate_Supports")]
        [Required]
        public bool? SupportsSecondaryLanguage { get; set; }

        [Display(Name = "MarkupTemplate_Supports")]
        [Required]
        public bool? SupportsTernaryLanguage { get; set; }

        [Display(Name = "MarkupTemplate_DownloadName")]
        [StringLength(1024)]
        public string DownloadName { get; set; }

        [Display(Name = "MarkupTemplate_Body")]
        [StringLength(1024 * 255)]
        public string Body { get; set; }

        [Display(Name = "MarkupTemplate_IsDeployed")]
        [Required]
        public bool? IsDeployed { get; set; }
    }

    public class MarkupTemplate : MarkupTemplateForSave
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

    public static class MarkupTemplateConst
    {
        public const string QueryByFilter = nameof(QueryByFilter);
        public const string QueryById = nameof(QueryById);
    }
}
