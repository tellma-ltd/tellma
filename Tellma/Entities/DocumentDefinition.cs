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
        public string Prefix { get; set; }
        public byte? CodeWidth { get; set; }

        // New Stuff
        public string AgentDefinitionId { get; set; }
        public string AgentLabel { get; set; }
        public string AgentLabel2 { get; set; }
        public string AgentLabel3 { get; set; }
        public string ClearanceVisibility { get; set; }
        public string InvestmentCenterVisibility { get; set; }
        public string Time1Visibility { get; set; }
        public string Time1Label { get; set; }
        public string Time1Label2 { get; set; }
        public string Time1Label3 { get; set; }
        public string Time2Visibility { get; set; }
        public string Time2Label { get; set; }
        public string Time2Label2 { get; set; }
        public string Time2Label3 { get; set; }
        public string QuantityVisibility { get; set; }
        public string QuantityLabel { get; set; }
        public string QuantityLabel2 { get; set; }
        public string QuantityLabel3 { get; set; }
        public string UnitVisibility { get; set; }
        public string UnitLabel { get; set; }
        public string UnitLabel2 { get; set; }
        public string UnitLabel3 { get; set; }
        public string CurrencyVisibility { get; set; }

        // End: New stuff

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

        public bool? CanReachState1 { get; set; }
        public bool? CanReachState2 { get; set; }
        public bool? CanReachState3 { get; set; }
        public bool? HasWorkflow { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
