using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Services.Utilities;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "ResourceDefinition", Plural = "ResourceDefinitions")]
    public class ResourceDefinitionForSave<TReportDefinition> : EntityWithKey<int>
    {
        #region Title & Code

        [Display(Name = "Code")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
        [Required]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitleSingular { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Secondary)]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitleSingular2 { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Ternary)]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitleSingular3 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Primary)]
        [Required]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitlePlural { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Secondary)]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitlePlural2 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Ternary)]
        [StringLength(100)]
        [AlwaysAccessible]
        public string TitlePlural3 { get; set; }

        [Display(Name = "ResourceDefinition_ResourceDefinitionType")]
        [Required]
        [NotNull]
        [ChoiceList(new object[] { 
            "PropertyPlantAndEquipment", 
            "InvestmentProperty", 
            "IntangibleAssetsOtherThanGoodwill", 
            "OtherFinancialAssets",
            "BiologicalAssets",
            "InventoriesTotal",
            "TradeAndOtherReceivables",
            "CashAndCashEquivalents",
            "TradeAndOtherPayables",
            "Provisions",
            "OtherFinancialLiabilities",
            "Miscellaneous",
        },
            new string[] {
            "RD_Type_PropertyPlantAndEquipment",
            "RD_Type_InvestmentProperty",
            "RD_Type_IntangibleAssetsOtherThanGoodwill",
            "RD_Type_OtherFinancialAssets",
            "RD_Type_BiologicalAssets",
            "RD_Type_InventoriesTotal",
            "RD_Type_TradeAndOtherReceivables",
            "RD_Type_CashAndCashEquivalents",
            "RD_Type_TradeAndOtherPayables",
            "RD_Type_Provisions",
            "RD_Type_OtherFinancialLiabilities",
            "RD_Type_Miscellaneous",
            })]
        [StringLength(255)]
        public string ResourceDefinitionType { get; set; }

        #endregion

        #region Common with Relations

        [VisibilityDisplay(Name = "Entity_Currency"), VisibilityChoiceList]
        [NotNull]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Center"), VisibilityChoiceList]
        [NotNull]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Image"), VisibilityChoiceList]
        [NotNull]
        public string ImageVisibility { get; set; }

        [VisibilityDisplay(Name = "Description"), VisibilityChoiceList]
        [NotNull]
        public string DescriptionVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Location"), VisibilityChoiceList]
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
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
        [NotNull]
        public string Text2Visibility { get; set; }

        [Display(Name = "Definition_PreprocessScript")]
        public string PreprocessScript { get; set; }

        [Display(Name = "Definition_ValidateScript")]
        public string ValidateScript { get; set; }

        #endregion

        #region Resources Only

        // Resource Properties

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Primary)]
        [StringLength(50)]
        public string IdentifierLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Secondary)]
        [StringLength(50)]
        public string IdentifierLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Ternary)]
        [StringLength(50)]
        public string IdentifierLabel3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Identifier"), VisibilityChoiceList]
        [NotNull]
        public string IdentifierVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_VatRate"), VisibilityChoiceList]
        [NotNull]
        public string VatRateVisibility { get; set; }

        [DefaultDisplay(Name = "Resource_VatRate")]
        public decimal? DefaultVatRate { get; set; }

        // Inventory

        [VisibilityDisplay(Name = "Resource_ReorderLevel"), VisibilityChoiceList]
        [NotNull]
        public string ReorderLevelVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_EconomicOrderQuantity"), VisibilityChoiceList]
        [NotNull]
        public string EconomicOrderQuantityVisibility { get; set; }

        [Display(Name = "ResourceDefinition_UnitCardinality")]
        [NotNull]
        [ChoiceList(new object[] { "None", "Single", "Multiple" }, 
            new string[] { "Cardinality_None", "Cardinality_Single", "Cardinality_Multiple" })]
        [StringLength(50)]
        public string UnitCardinality { get; set; }

        [DefaultDisplay(Name = "Resource_Unit")]
        public int? DefaultUnitId { get; set; }

        [VisibilityDisplay(Name = "Resource_UnitMass"), VisibilityChoiceList]
        [NotNull]
        public string UnitMassVisibility { get; set; }

        [DefaultDisplay(Name = "Resource_UnitMassUnit")]
        public int? DefaultUnitMassUnitId { get; set; }

        // Financial Instruments

        [VisibilityDisplay(Name = "Resource_MonetaryValue"), VisibilityChoiceList]
        [NotNull]
        public string MonetaryValueVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_Participant"), VisibilityChoiceList]
        [NotNull]
        public string ParticipantVisibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Participant")]
        public int? ParticipantDefinitionId { get; set; }

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
        [ForeignKey(nameof(ResourceDefinitionReportDefinition.ResourceDefinitionId))]
        public List<TReportDefinition> ReportDefinitions { get; set; }
    }

    public class ResourceDefinitionForSave : ResourceDefinitionForSave<ResourceDefinitionReportDefinitionForSave>
    {
    }

    public class ResourceDefinition : ResourceDefinitionForSave<ResourceDefinitionReportDefinition>
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

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }

        [DefaultDisplay(Name = "Resource_UnitMassUnit")]
        [ForeignKey(nameof(DefaultUnitId))]
        public Unit DefaultUnit { get; set; }

        [DefaultDisplay(Name = "Resource_UnitMassUnit")]
        [ForeignKey(nameof(DefaultUnitMassUnitId))]
        public Unit DefaultUnitMassUnit { get; set; }

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

        [DefinitionDefinitionDisplay(Name = "Resource_Participant")]
        [ForeignKey(nameof(ParticipantDefinitionId))]
        public RelationDefinition ParticipantDefinition { get; set; }
    }
}
