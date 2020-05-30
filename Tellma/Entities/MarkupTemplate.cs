using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Controllers.Utilities;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "MarkupTemplate", Plural = "MarkupTemplates")]
    public class MarkupTemplateForSave : EntityWithKey<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "MarkupTemplate_Usage")]
        [ChoiceList(new object[] { MarkupTemplateConst.QueryByFilter, MarkupTemplateConst.QueryById }, 
            new string[] { "MarkupTemplate_Usage_QueryByFilter", "MarkupTemplate_Usage_QueryById" })]
        [StringLength(50)]
        public string Usage { get; set; }

        [Display(Name = "MarkupTemplate_Collection")]
        [StringLength(50)]
        public string Collection { get; set; }

        [Display(Name = "MarkupTemplate_DefinitionId")]
        [StringLength(50)]
        public string DefinitionId { get; set; }

        [Display(Name = "MarkupTemplate_MarkupLanguage")]
        [Required]
        [ChoiceList(new object[] { MimeTypes.Html }, new string[] { "HTML" })]
        [StringLength(255)]
        public string MarkupLanguage { get; set; } // HTML, Markdown, XML, etc…

        [MultilingualDisplay(Name = "MarkupTemplate_Supports", Language = Language.Primary)]
        public bool? SupportsPrimaryLanguage { get; set; }

        [MultilingualDisplay(Name = "MarkupTemplate_Supports", Language = Language.Secondary)]
        public bool? SupportsSecondaryLanguage { get; set; }

        [MultilingualDisplay(Name = "MarkupTemplate_Supports", Language = Language.Ternary)]
        public bool? SupportsTernaryLanguage { get; set; }

        [Display(Name = "MarkupTemplate_DownloadName")]
        [StringLength(1024)]
        public string DownloadName { get; set; }

        [Display(Name = "MarkupTemplate_Body")]
        [StringLength(1024 * 255)]
        public string Body { get; set; }
    }

    public class MarkupTemplate : MarkupTemplateForSave
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
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
