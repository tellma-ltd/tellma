using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DocumentDefinition", GroupName = "DocumentDefinitions")]
    public class DocumentDefinitionForSave<TLineDefinition> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [StringLength(50)]
        [ValidateRequired]
        public string Code { get; set; }

        [Display(Name = "DocumentDefinition_IsOriginalDocument")]
        public bool? IsOriginalDocument { get; set; }

        [Display(Name = "Description")]
        [Required, ValidateRequired]
        [StringLength(1024)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description3 { get; set; }

        [Display(Name = "TitleSingular")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string TitleSingular { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(50)]
        public string TitleSingular2 { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(50)]
        public string TitleSingular3 { get; set; }

        [Display(Name = "TitlePlural")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string TitlePlural { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(50)]
        public string TitlePlural2 { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(50)]
        public string TitlePlural3 { get; set; }

        [Display(Name = "DocumentDefinition_Prefix")]
        [Required, ValidateRequired]
        [StringLength(5)]
        public string Prefix { get; set; }

        [Display(Name = "DocumentDefinition_CodeWidth")]
        [Required]
        public byte? CodeWidth { get; set; }

        [VisibilityDisplay(Name = "Document_PostingDate"), VisibilityChoiceList]
        [Required, ValidateRequired]
        public string PostingDateVisibility { get; set; }

        [VisibilityDisplay(Name = "Document_Center"), VisibilityChoiceList]
        [Required, ValidateRequired]
        public string CenterVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1")]
        [StringLength(50)]
        public string Lookup1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup1"), VisibilityChoiceList]
        [Required]
        public string Lookup1Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        public int? Lookup1DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2")]
        [StringLength(50)]
        public string Lookup2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup2"), VisibilityChoiceList]
        [Required]
        public string Lookup2Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        public int? Lookup2DefinitionId { get; set; }

        [ChoiceList(new object[] {
               "381",
               "383",
               "388",
               "386"
            },
            new string[] {
               "DocumentDefinition_ZatcaDocumentType_381",
               "DocumentDefinition_ZatcaDocumentType_383",
               "DocumentDefinition_ZatcaDocumentType_388",
               "DocumentDefinition_ZatcaDocumentType_386"
            })]
        [Display(Name = "DocumentDefinition_ZatcaDocumentType")]
        [StringLength(3)]
        public string ZatcaDocumentType { get; set; }

        [VisibilityDisplay(Name = "Document_Clearance"), VisibilityChoiceList]
        [Required, ValidateRequired]
        public string ClearanceVisibility { get; set; }

        [VisibilityDisplay(Name = "Memo"), VisibilityChoiceList]
        [Required, ValidateRequired]
        public string MemoVisibility { get; set; }

        [VisibilityDisplay(Name = "Document_Attachments"), VisibilityChoiceList]
        [Required, ValidateRequired]
        public string AttachmentVisibility { get; set; }

        [Display(Name = "DocumentDefinition_HasBookkeeping")]
        [Required]
        public bool? HasBookkeeping { get; set; }

        [Display(Name = "DocumentDefinition_CloseValidateScript")]
        public string CloseValidateScript { get; set; }

        [Display(Name = "MainMenuIcon")]
        [StringLength(50)]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(50)]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
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
        [Required]
        [ChoiceList(new object[] { 
                DefStates.Hidden, 
                DefStates.Testing,
                DefStates.Visible, 
                DefStates.Archived },
            new string[] { 
                DefStateNames.Hidden, 
                DefStateNames.Testing,
                DefStateNames.Visible, 
                DefStateNames.Archived })]
        public string State { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        [ForeignKey(nameof(Lookup1DefinitionId))]
        public LookupDefinition Lookup1Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        [ForeignKey(nameof(Lookup2DefinitionId))]
        public LookupDefinition Lookup2Definition { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? SavedAt { get; set; }

        [Required]
        public bool? CanReachState1 { get; set; }

        [Required]
        public bool? CanReachState2 { get; set; }

        [Required]
        public bool? CanReachState3 { get; set; }

        [Required]
        public bool? HasWorkflow { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
