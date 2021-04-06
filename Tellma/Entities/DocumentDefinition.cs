using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Services.Utilities;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentDefinition", Plural = "DocumentDefinitions")]
    public class DocumentDefinitionForSave<TLineDefinition> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "DocumentDefinition_IsOriginalDocument")]
        public bool? IsOriginalDocument { get; set; }

        [Display(Name = "DocumentDefinition_DocumentType")]
        [Required]
        [NotNull]
        [ChoiceList(new object[] { (byte)0, (byte)1, (byte)2, (byte)3 },
            new string[] { "DocumentDefinition_DocumentType_0", "DocumentDefinition_DocumentType_1", "DocumentDefinition_DocumentType_2", "DocumentDefinition_DocumentType_3" })]
        public byte? DocumentType { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [Required]
        [NotNull]
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

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitleSingular { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitleSingular2 { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitleSingular3 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitlePlural { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitlePlural2 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string TitlePlural3 { get; set; }

        [Display(Name = "DocumentDefinition_Prefix")]
        [Required]
        [NotNull]
        [StringLength(5)]
        public string Prefix { get; set; }

        [Display(Name = "DocumentDefinition_CodeWidth")]
        [NotNull]
        public byte? CodeWidth { get; set; }

        [VisibilityDisplay(Name = "Document_PostingDate"), VisibilityChoiceList]
        [Required]
        [NotNull]
        public string PostingDateVisibility { get; set; }

        [VisibilityDisplay(Name = "Document_Center"), VisibilityChoiceList]
        [Required]
        [NotNull]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Document_Clearance"), VisibilityChoiceList]
        [Required]
        [NotNull]
        public string ClearanceVisibility { get; set; }

        [VisibilityDisplay(Name = "Memo"), VisibilityChoiceList]
        [Required]
        [NotNull]
        public string MemoVisibility { get; set; }

        [Display(Name = "Definition_HasAttachments")]
        [Required]
        [NotNull]
        public bool? HasAttachments { get; set; }

        [Display(Name = "DocumentDefinition_HasBookkeeping")]
        [Required]
        [NotNull]
        public bool? HasBookkeeping { get; set; }

        [Display(Name = "MainMenuIcon")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "DocumentDefinition_LineDefinitions")]
        [ForeignKey(nameof(DocumentDefinitionLineDefinition.DocumentDefinitionId))]
        public List<TLineDefinition> LineDefinitions { get; set; }
    }

    public class DocumentDefinitionForSave : DocumentDefinitionForSave<DocumentDefinitionLineDefinitionForSave>
    {
    }

    public class DocumentDefinition : DocumentDefinitionForSave<DocumentDefinitionLineDefinition>
    {
        [Display(Name = "Definition_State")]
        [NotNull]
        [ChoiceList(new object[] { DefStates.Hidden, DefStates.Visible, DefStates.Archived },
            new string[] { "Definition_State_Hidden", "Definition_State_Visible", "Definition_State_Archived" })]
        [AlwaysAccessible]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        [NotNull]
        public bool? CanReachState1 { get; set; }

        [NotNull]
        public bool? CanReachState2 { get; set; }

        [NotNull]
        public bool? CanReachState3 { get; set; }

        [NotNull]
        public bool? HasWorkflow { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
