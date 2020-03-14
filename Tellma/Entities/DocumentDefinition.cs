using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class DocumentDefinitionForSave<TDocumentDefinitionLineDefinition> : EntityWithKey<string>
    {
        public bool? IsOriginalDocument { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitleSingular { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitleSingular2 { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitleSingular3 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitlePlural { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitlePlural2 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string TitlePlural3 { get; set; }


        public string Prefix { get; set; }
        public byte? CodeWidth { get; set; }
        public string AgentDefinitionId { get; set; }



        [Display(Name = "MainMenuIcon")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }

        [ForeignKey(nameof(DocumentDefinitionLineDefinition.DocumentDefinitionId))]
        public List<TDocumentDefinitionLineDefinition> LineDefinitions { get; set; }
    }

    public class DocumentDefinitionForSave : DocumentDefinitionForSave<DocumentDefinitionLineDefinitionForSave>
    {

    }

    public class DocumentDefinition : DocumentDefinitionForSave<DocumentDefinitionLineDefinition>
    {
        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
