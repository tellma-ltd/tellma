using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class ResourceDefinitionForSave : EntityWithKey<string>
    {
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
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
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

        // TODO: Add Metadata

        public string IdentifierLabel { get; set; }
        public string IdentifierLabel2 { get; set; }
        public string IdentifierLabel3 { get; set; }
        public string IdentifierVisibility { get; set; }
        public string CurrencyVisibility { get; set; }
        public string DescriptionVisibility { get; set; }
        public string CostObjectVisibility { get; set; }
        public string ExpenseEntryTypeVisibility { get; set; }
        public string ExpenseCenterVisibility { get; set; }
        public string InvestmentCenterVisibility { get; set; }
        public string ResidualMonetaryValueVisibility { get; set; }
        public string ResidualValueVisibility { get; set; }
        public string ReorderLevelVisibility { get; set; }
        public string EconomicOrderQuantityVisibility { get; set; }
        public string AvailableSinceLabel { get; set; }
        public string AvailableSinceLabel2 { get; set; }
        public string AvailableSinceLabel3 { get; set; }
        public string AvailableSinceVisibility { get; set; }
        public string AvailableTillLabel { get; set; }
        public string AvailableTillLabel2 { get; set; }
        public string AvailableTillLabel3 { get; set; }
        public string AvailableTillVisibility { get; set; }
        public string Decimal1Label { get; set; }
        public string Decimal1Label2 { get; set; }
        public string Decimal1Label3 { get; set; }
        public string Decimal1Visibility { get; set; }
        public string Decimal2Label { get; set; }
        public string Decimal2Label2 { get; set; }
        public string Decimal2Label3 { get; set; }
        public string Decimal2Visibility { get; set; }
        public string Int1Label { get; set; }
        public string Int1Label2 { get; set; }
        public string Int1Label3 { get; set; }
        public string Int1Visibility { get; set; }
        public string Int2Label { get; set; }
        public string Int2Label2 { get; set; }
        public string Int2Label3 { get; set; }
        public string Int2Visibility { get; set; }
        public string Lookup1Label { get; set; }
        public string Lookup1Label2 { get; set; }
        public string Lookup1Label3 { get; set; }
        public string Lookup1Visibility { get; set; }
        public string Lookup1DefinitionId { get; set; }
        public string Lookup2Label { get; set; }
        public string Lookup2Label2 { get; set; }
        public string Lookup2Label3 { get; set; }
        public string Lookup2Visibility { get; set; }
        public string Lookup2DefinitionId { get; set; }
        public string Lookup3Label { get; set; }
        public string Lookup3Label2 { get; set; }
        public string Lookup3Label3 { get; set; }
        public string Lookup3Visibility { get; set; }
        public string Lookup3DefinitionId { get; set; }
        public string Lookup4Label { get; set; }
        public string Lookup4Label2 { get; set; }
        public string Lookup4Label3 { get; set; }
        public string Lookup4Visibility { get; set; }
        public string Lookup4DefinitionId { get; set; }
        public string DueDateLabel { get; set; }
        public string DueDateLabel2 { get; set; }
        public string DueDateLabel3 { get; set; }
        public string DueDateVisibility { get; set; }
        public string Text1Label { get; set; }
        public string Text1Label2 { get; set; }
        public string Text1Label3 { get; set; }
        public string Text1Visibility { get; set; }
        public string Text2Label { get; set; }
        public string Text2Label2 { get; set; }
        public string Text2Label3 { get; set; }
        public string Text2Visibility { get; set; }
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
