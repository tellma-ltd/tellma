using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Services.Utilities;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "RelationDefinition", Plural = "RelationDefinitions")]
    public class RelationDefinitionForSave : EntityWithKey<int>
    {
        #region Title & Code

        [Display(Name = "Code")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
        [Required]
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

        #endregion

        #region Common with Resources

        [VisibilityDisplay(Name = "Entity_Currency"), VisibilityChoiceList]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Center"), VisibilityChoiceList]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Image"), VisibilityChoiceList]
        public string ImageVisibility { get; set; }

        [VisibilityDisplay(Name = "Description"), VisibilityChoiceList]
        public string DescriptionVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Location"), VisibilityChoiceList]
        public string LocationVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate", Language = Language.Primary)]
        [StringLength(50)]
        public string FromDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate", Language = Language.Secondary)]
        [StringLength(50)]
        public string FromDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate", Language = Language.Ternary)]
        [StringLength(50)]
        public string FromDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_FromDate"), VisibilityChoiceList]
        public string FromDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate", Language = Language.Primary)]
        [StringLength(50)]
        public string ToDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate", Language = Language.Secondary)]
        [StringLength(50)]
        public string ToDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate", Language = Language.Ternary)]
        [StringLength(50)]
        public string ToDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_ToDate"), VisibilityChoiceList]
        public string ToDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1", Language = Language.Primary)]
        [StringLength(50)]
        public string Decimal1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Decimal1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Decimal1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal1"), VisibilityChoiceList]
        public string Decimal1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2", Language = Language.Primary)]
        [StringLength(50)]
        public string Decimal2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2", Language = Language.Secondary)]
        [StringLength(50)]
        public string Decimal2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2", Language = Language.Ternary)]
        [StringLength(50)]
        public string Decimal2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal2"), VisibilityChoiceList]
        public string Decimal2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1", Language = Language.Primary)]
        [StringLength(50)]
        public string Int1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Int1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Int1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int1"), VisibilityChoiceList]
        public string Int1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2", Language = Language.Primary)]
        [StringLength(50)]
        public string Int2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2", Language = Language.Secondary)]
        [StringLength(50)]
        public string Int2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2", Language = Language.Ternary)]
        [StringLength(50)]
        public string Int2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int2"), VisibilityChoiceList]
        public string Int2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup1"), VisibilityChoiceList]
        public string Lookup1Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        public int? Lookup1DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup2", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup2"), VisibilityChoiceList]
        public string Lookup2Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        public int? Lookup2DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup3"), VisibilityChoiceList]
        public string Lookup3Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup3")]
        public int? Lookup3DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup4"), VisibilityChoiceList]
        public string Lookup4Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup4")]
        public int? Lookup4DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1", Language = Language.Primary)]
        [StringLength(50)]
        public string Text1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Text1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Text1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text1"), VisibilityChoiceList]
        public string Text1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2", Language = Language.Primary)]
        [StringLength(50)]
        public string Text2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2", Language = Language.Secondary)]
        [StringLength(50)]
        public string Text2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2", Language = Language.Ternary)]
        [StringLength(50)]
        public string Text2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text2"), VisibilityChoiceList]
        public string Text2Visibility { get; set; }

        [Display(Name = "Definition_Script")]
        public string Script { get; set; }

        #endregion

        #region Relation Only

        [VisibilityDisplay(Name = "Relation_Agent")]
        [VisibilityChoiceList]
        public string AgentVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_TaxIdentificationNumber")]
        [VisibilityChoiceList]
        public string TaxIdentificationNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_Job")]
        [VisibilityChoiceList]
        public string JobVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_BankAccountNumber")]
        [VisibilityChoiceList]
        public string BankAccountNumberVisibility { get; set; }

        [Display(Name = "RelationDefinition_UserCardinality")]
        [ChoiceList(new object[] { "None", "Single", "Multiple" },
            new string[] { "Cardinality_None", "Cardinality_Single", "Cardinality_Multiple" })]
        public string UserCardinality { get; set; }

        #endregion

        #region Main Menu

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

        #endregion
    }

    public class RelationDefinition : RelationDefinitionForSave
    {
        [Display(Name = "Definition_State")]
        [ChoiceList(new object[] { DefStates.Hidden, DefStates.Visible, DefStates.Archived },
            new string[] { "Definition_State_Hidden", "Definition_State_Visible", "Definition_State_Archived" })]
        [AlwaysAccessible]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
