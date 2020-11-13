using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Services.Utilities;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "RelationDefinition", Plural = "RelationDefinitions")]
    public class RelationDefinitionForSave<TReportDefinition> : EntityWithKey<int>
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

        [VisibilityDisplay(Name = "Relation_DateOfBirth"), VisibilityChoiceList]
        public string DateOfBirthVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactEmail"), VisibilityChoiceList]
        public string ContactEmailVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactMobile"), VisibilityChoiceList]
        public string ContactMobileVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_ContactAddress"), VisibilityChoiceList]
        public string ContactAddressVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date1", Language = Language.Primary)]
        [StringLength(50)]
        public string Date1Label { get; set; }
        
        [DefinitionLabelDisplay(Name = "Entity_Date1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Date1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Date1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date1"), VisibilityChoiceList]
        public string Date1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2", Language = Language.Primary)]
        [StringLength(50)]
        public string Date2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2", Language = Language.Secondary)]
        [StringLength(50)]
        public string Date2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date2", Language = Language.Ternary)]
        [StringLength(50)]
        public string Date2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date2"), VisibilityChoiceList]
        public string Date2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3", Language = Language.Primary)]
        [StringLength(50)]
        public string Date3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3", Language = Language.Secondary)]
        [StringLength(50)]
        public string Date3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date3", Language = Language.Ternary)]
        [StringLength(50)]
        public string Date3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date3"), VisibilityChoiceList]
        public string Date3Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4", Language = Language.Primary)]
        [StringLength(50)]
        public string Date4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4", Language = Language.Secondary)]
        [StringLength(50)]
        public string Date4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Date4", Language = Language.Ternary)]
        [StringLength(50)]
        public string Date4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Date4"), VisibilityChoiceList]
        public string Date4Visibility { get; set; }

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

        [DefinitionLabelDisplay(Name = "Entity_Lookup5", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup5Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup5", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup5Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup5", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup5Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup5"), VisibilityChoiceList]
        public string Lookup5Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup5")]
        public int? Lookup5DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup6Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup6Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup6", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup6Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup6"), VisibilityChoiceList]
        public string Lookup6Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup6")]
        public int? Lookup6DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup7Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup7Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup7", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup7Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup7"), VisibilityChoiceList]
        public string Lookup7Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup7")]
        public int? Lookup7DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8", Language = Language.Primary)]
        [StringLength(50)]
        public string Lookup8Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8", Language = Language.Secondary)]
        [StringLength(50)]
        public string Lookup8Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup8", Language = Language.Ternary)]
        [StringLength(50)]
        public string Lookup8Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup8"), VisibilityChoiceList]
        public string Lookup8Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup8")]
        public int? Lookup8DefinitionId { get; set; }

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

        [DefinitionLabelDisplay(Name = "Entity_Text3", Language = Language.Primary)]
        [StringLength(50)]
        public string Text3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text3", Language = Language.Secondary)]
        [StringLength(50)]
        public string Text3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text3", Language = Language.Ternary)]
        [StringLength(50)]
        public string Text3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text3"), VisibilityChoiceList]
        public string Text3Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4", Language = Language.Primary)]
        [StringLength(50)]
        public string Text4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4", Language = Language.Secondary)]
        [StringLength(50)]
        public string Text4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text4", Language = Language.Ternary)]
        [StringLength(50)]
        public string Text4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text4"), VisibilityChoiceList]
        public string Text4Visibility { get; set; }

        [Display(Name = "Definition_Script")]
        public string Script { get; set; }

        #endregion

        #region Relation Only

        [DefinitionLabelDisplay(Name = "Entity_Relation1", Language = Language.Primary)]
        [StringLength(50)]
        public string Relation1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Relation1", Language = Language.Secondary)]
        [StringLength(50)]
        public string Relation1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Relation1", Language = Language.Ternary)]
        [StringLength(50)]
        public string Relation1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Relation1"), VisibilityChoiceList]
        public string Relation1Visibility { get; set; }

        [NotMapped]
        public int? Relation1DefinitionIndex { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Relation1")]
        [SelfReferencing(nameof(Relation1DefinitionIndex))]
        public int? Relation1DefinitionId { get; set; }

        [VisibilityDisplay(Name = "Relation_Agent"), VisibilityChoiceList]
        public string AgentVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_TaxIdentificationNumber"), VisibilityChoiceList]
        public string TaxIdentificationNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_Job"), VisibilityChoiceList]
        public string JobVisibility { get; set; }

        [VisibilityDisplay(Name = "Relation_BankAccountNumber"), VisibilityChoiceList]
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

        [Display(Name = "Definition_ReportDefinitions")]
        [ForeignKey(nameof(RelationDefinitionReportDefinition.RelationDefinitionId))]
        public List<TReportDefinition> ReportDefinitions { get; set; }
    }

    public class RelationDefinitionForSave : RelationDefinitionForSave<RelationDefinitionReportDefinitionForSave>
    {
    }

    public class RelationDefinition : RelationDefinitionForSave<RelationDefinitionReportDefinition>
    {
        [Display(Name = "Definition_State")]
        [ChoiceList(new object[] { DefStates.Hidden, DefStates.Visible, DefStates.Archived },
            new string[] { "Definition_State_Hidden", "Definition_State_Visible", "Definition_State_Archived" })]
        [AlwaysAccessible]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup1")]
        [ForeignKey(nameof(Lookup1DefinitionId))]
        public LookupDefinition Lookup1Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup2")]
        [ForeignKey(nameof(Lookup2DefinitionId))]
        public LookupDefinition Lookup2Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup3")]
        [ForeignKey(nameof(Lookup3DefinitionId))]
        public LookupDefinition Lookup3Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup4")]
        [ForeignKey(nameof(Lookup4DefinitionId))]
        public LookupDefinition Lookup4Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup5")]
        [ForeignKey(nameof(Lookup5DefinitionId))]
        public LookupDefinition Lookup5Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup6")]
        [ForeignKey(nameof(Lookup6DefinitionId))]
        public LookupDefinition Lookup6Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup7")]
        [ForeignKey(nameof(Lookup7DefinitionId))]
        public LookupDefinition Lookup7Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup8")]
        [ForeignKey(nameof(Lookup8DefinitionId))]
        public LookupDefinition Lookup8Definition { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Relation1")]
        [ForeignKey(nameof(Relation1DefinitionId))]
        public LookupDefinition Relation1Definition { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
