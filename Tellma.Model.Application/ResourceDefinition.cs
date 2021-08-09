using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "ResourceDefinition", GroupName = "ResourceDefinitions")]
    public class ResourceDefinitionForSave<TReportDefinition> : EntityWithKey<int>
    {
        #region Title & Code

        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Code { get; set; }

        [Display(Name = "TitleSingular")]
        [ValidateRequired]
        [StringLength(100)]
        public string TitleSingular { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(100)]
        public string TitleSingular2 { get; set; }

        [Display(Name = "TitleSingular")]
        [StringLength(100)]
        public string TitleSingular3 { get; set; }

        [Display(Name = "TitlePlural")]
        [ValidateRequired]
        [StringLength(100)]
        public string TitlePlural { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(100)]
        public string TitlePlural2 { get; set; }

        [Display(Name = "TitlePlural")]
        [StringLength(100)]
        public string TitlePlural3 { get; set; }

        [Display(Name = "ResourceDefinition_ResourceDefinitionType")]
        [Required, ValidateRequired]
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
        [Required]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Center"), VisibilityChoiceList]
        [Required]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Image"), VisibilityChoiceList]
        [Required]
        public string ImageVisibility { get; set; }

        [VisibilityDisplay(Name = "Description"), VisibilityChoiceList]
        [Required]
        public string DescriptionVisibility { get; set; }

        [VisibilityDisplay(Name = "Entity_Location"), VisibilityChoiceList]
        [Required]
        public string LocationVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_FromDate")]
        [StringLength(50)]
        public string FromDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_FromDate"), VisibilityChoiceList]
        [Required]
        public string FromDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_ToDate")]
        [StringLength(50)]
        public string ToDateLabel3 { get; set; }

        [VisibilityDisplay(Name = "Entity_ToDate"), VisibilityChoiceList]
        [Required]
        public string ToDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal1")]
        [StringLength(50)]
        public string Decimal1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal1"), VisibilityChoiceList]
        [Required]
        public string Decimal1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Decimal2")]
        [StringLength(50)]
        public string Decimal2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Decimal2"), VisibilityChoiceList]
        [Required]
        public string Decimal2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int1")]
        [StringLength(50)]
        public string Int1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int1"), VisibilityChoiceList]
        [Required]
        public string Int1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Int2")]
        [StringLength(50)]
        public string Int2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Int2"), VisibilityChoiceList]
        [Required]
        public string Int2Visibility { get; set; }

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

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup3")]
        [StringLength(50)]
        public string Lookup3Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup3"), VisibilityChoiceList]
        [Required]
        public string Lookup3Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup3")]
        public int? Lookup3DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Lookup4")]
        [StringLength(50)]
        public string Lookup4Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Lookup4"), VisibilityChoiceList]
        [Required]
        public string Lookup4Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Lookup4")]
        public int? Lookup4DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text1"), VisibilityChoiceList]
        [Required]
        public string Text1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Text2"), VisibilityChoiceList]
        [Required]
        public string Text2Visibility { get; set; }

        [Display(Name = "Definition_PreprocessScript")]
        public string PreprocessScript { get; set; }

        [Display(Name = "Definition_ValidateScript")]
        public string ValidateScript { get; set; }

        #endregion

        #region Resources Only

        // Resource Properties

        [DefinitionLabelDisplay(Name = "Resource_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier")]
        [StringLength(50)]
        public string IdentifierLabel3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Identifier"), VisibilityChoiceList]
        [Required]
        public string IdentifierVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_VatRate"), VisibilityChoiceList]
        [Required]
        public string VatRateVisibility { get; set; }

        [DefaultDisplay(Name = "Resource_VatRate")]
        public decimal? DefaultVatRate { get; set; }

        // Inventory

        [VisibilityDisplay(Name = "Resource_ReorderLevel"), VisibilityChoiceList]
        [Required]
        public string ReorderLevelVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_EconomicOrderQuantity"), VisibilityChoiceList]
        [Required]
        public string EconomicOrderQuantityVisibility { get; set; }

        [Display(Name = "ResourceDefinition_UnitCardinality")]
        [Required]
        [ChoiceList(new object[] { "None", "Single", "Multiple" }, 
            new string[] { "Cardinality_None", "Cardinality_Single", "Cardinality_Multiple" })]
        [StringLength(50)]
        public string UnitCardinality { get; set; }

        [DefaultDisplay(Name = "Resource_Unit")]
        public int? DefaultUnitId { get; set; }

        [VisibilityDisplay(Name = "Resource_UnitMass"), VisibilityChoiceList]
        [Required]
        public string UnitMassVisibility { get; set; }

        [DefaultDisplay(Name = "Resource_UnitMassUnit")]
        public int? DefaultUnitMassUnitId { get; set; }

        // Financial Instruments

        [VisibilityDisplay(Name = "Resource_MonetaryValue"), VisibilityChoiceList]
        [Required]
        public string MonetaryValueVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_Participant"), VisibilityChoiceList]
        [Required]
        public string ParticipantVisibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Participant")]
        public int? ParticipantDefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Resource1")]
        [StringLength(50)]
        public string Resource1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Resource1")]
        [StringLength(50)]
        public string Resource1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Entity_Resource1")]
        [StringLength(50)]
        public string Resource1Label3 { get; set; }

        [VisibilityDisplay(Name = "Entity_Resource1"), VisibilityChoiceList]
        [Required]
        public string Resource1Visibility { get; set; }

        [NotMapped]
        public int? Resource1DefinitionIndex { get; set; }

        [DefinitionDefinitionDisplay(Name = "Entity_Resource1")]
        [SelfReferencing(nameof(Resource1DefinitionIndex))]
        public int? Resource1DefinitionId { get; set; }

        #endregion

        #region Main Menu

        [Display(Name = "MainMenuIcon")]
        [StringLength(50)]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(50)]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
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
        [Required]
        [ChoiceList(new object[] {
                DefStates.Hidden,
                DefStates.Visible,
                DefStates.Archived },
            new string[] {
                DefStateNames.Hidden,
                DefStateNames.Visible,
                DefStateNames.Archived })]
        public string State { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ValidFrom { get; set; }

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

        [DefinitionDefinitionDisplay(Name = "Entity_Resource1")]
        [ForeignKey(nameof(Resource1DefinitionId))]
        public LookupDefinition Resource1Definition { get; set; }
    }
}
