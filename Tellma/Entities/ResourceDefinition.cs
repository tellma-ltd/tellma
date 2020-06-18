using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "ResourceDefinition", Plural = "ResourceDefinitions")]
    public class ResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitleSingular { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitleSingular2 { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitleSingular3 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitlePlural { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitlePlural2 { get; set; }

        [MultilingualDisplay(Name = "TitlePlural", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string TitlePlural3 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Primary)]
        public string IdentifierLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Secondary)]
        public string IdentifierLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Identifier", Language = Language.Ternary)]
        public string IdentifierLabel3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Identifier"), VisibilityChoiceList]
        public string IdentifierVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_Currency"), VisibilityChoiceList]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Description"), VisibilityChoiceList]
        public string DescriptionVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_Location"), VisibilityChoiceList]
        public string LocationVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_Center"), VisibilityChoiceList]
        public string CenterVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_ResidualMonetaryValue"), VisibilityChoiceList]
        public string ResidualMonetaryValueVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_ResidualValue"), VisibilityChoiceList]
        public string ResidualValueVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_ReorderLevel"), VisibilityChoiceList]
        public string ReorderLevelVisibility { get; set; }

        [VisibilityDisplay(Name = "Resource_EconomicOrderQuantity"), VisibilityChoiceList]
        public string EconomicOrderQuantityVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableSince", Language = Language.Primary)]
        public string AvailableSinceLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableSince", Language = Language.Secondary)]
        public string AvailableSinceLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableSince", Language = Language.Ternary)]
        public string AvailableSinceLabel3 { get; set; }

        [VisibilityDisplay(Name = "Resource_AvailableSince"), VisibilityChoiceList]
        public string AvailableSinceVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableTill", Language = Language.Primary)]
        public string AvailableTillLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableTill", Language = Language.Secondary)]
        public string AvailableTillLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_AvailableTill", Language = Language.Ternary)]
        public string AvailableTillLabel3 { get; set; }

        [VisibilityDisplay(Name = "Resource_AvailableTill"), VisibilityChoiceList]
        public string AvailableTillVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal1", Language = Language.Primary)]
        public string Decimal1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal1", Language = Language.Secondary)]
        public string Decimal1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal1", Language = Language.Ternary)]
        public string Decimal1Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Decimal1"), VisibilityChoiceList]
        public string Decimal1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal2", Language = Language.Primary)]
        public string Decimal2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal2", Language = Language.Secondary)]
        public string Decimal2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Decimal2", Language = Language.Ternary)]
        public string Decimal2Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Decimal2"), VisibilityChoiceList]
        public string Decimal2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int1", Language = Language.Primary)]
        public string Int1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int1", Language = Language.Secondary)]
        public string Int1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int1", Language = Language.Ternary)]
        public string Int1Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Int1"), VisibilityChoiceList]
        public string Int1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int2", Language = Language.Primary)]
        public string Int2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int2", Language = Language.Secondary)]
        public string Int2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Int2", Language = Language.Ternary)]
        public string Int2Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Int2"), VisibilityChoiceList]
        public string Int2Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup1", Language = Language.Primary)]
        public string Lookup1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup1", Language = Language.Secondary)]
        public string Lookup1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup1", Language = Language.Ternary)]
        public string Lookup1Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Lookup1"), VisibilityChoiceList]
        public string Lookup1Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Lookup1")]
        public int? Lookup1DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup2", Language = Language.Primary)]
        public string Lookup2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup2", Language = Language.Secondary)]
        public string Lookup2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup2", Language = Language.Ternary)]
        public string Lookup2Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Lookup2"), VisibilityChoiceList]
        public string Lookup2Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Lookup2")]
        public int? Lookup2DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup3", Language = Language.Primary)]
        public string Lookup3Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup3", Language = Language.Secondary)]
        public string Lookup3Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup3", Language = Language.Ternary)]
        public string Lookup3Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Lookup3"), VisibilityChoiceList]
        public string Lookup3Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Lookup3")]
        public int? Lookup3DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup4", Language = Language.Primary)]
        public string Lookup4Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup4", Language = Language.Secondary)]
        public string Lookup4Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Lookup4", Language = Language.Ternary)]
        public string Lookup4Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Lookup4"), VisibilityChoiceList]
        public string Lookup4Visibility { get; set; }

        [DefinitionDefinitionDisplay(Name = "Resource_Lookup4")]
        public int? Lookup4DefinitionId { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text1", Language = Language.Primary)]
        public string Text1Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text1", Language = Language.Secondary)]
        public string Text1Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text1", Language = Language.Ternary)]
        public string Text1Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Text1"), VisibilityChoiceList]
        public string Text1Visibility { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text2", Language = Language.Primary)]
        public string Text2Label { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text2", Language = Language.Secondary)]
        public string Text2Label2 { get; set; }

        [DefinitionLabelDisplay(Name = "Resource_Text2", Language = Language.Ternary)]
        public string Text2Label3 { get; set; }

        [VisibilityDisplay(Name = "Resource_Text2"), VisibilityChoiceList]
        public string Text2Visibility { get; set; }

        [Display(Name = "MainMenuIcon")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSection")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }
    }

    public class ResourceDefinition : ResourceDefinitionForSave
    {
        [Display(Name = "Definition_State")]
        [ChoiceList(new object[] { "Draft", "Deployed", "Archived" },
            new string[] { "Definition_State_Draft", "Definition_State_Deployed", "Definition_State_Archived" })]
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
