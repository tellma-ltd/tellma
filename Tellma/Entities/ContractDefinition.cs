using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "ContractDefinition", Plural = "ContractDefinitions")]
    public class ContractDefinitionForSave : EntityWithKey<int>
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

        // TODO: Add metadata

        [VisibilityDisplay(Name = "Contract_Agent")]
        [VisibilityChoiceList]
        public string AgentVisibility { get; set; }

        [VisibilityDisplay(Name = "Contract_Currency")]
        [VisibilityChoiceList]
        public string CurrencyVisibility { get; set; }

        [VisibilityDisplay(Name = "Contract_TaxIdentificationNumber")]
        [VisibilityChoiceList]
        public string TaxIdentificationNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Image")]
        [VisibilityChoiceList]
        public string ImageVisibility { get; set; }

        [VisibilityDisplay(Name = "Contract_StartDate")]
        [VisibilityChoiceList]
        public string StartDateVisibility { get; set; }

        [DefinitionLabelDisplay(Name = "Contract_StartDate", Language = Language.Primary)]
        public string StartDateLabel { get; set; }

        [DefinitionLabelDisplay(Name = "Contract_StartDate", Language = Language.Secondary)]
        public string StartDateLabel2 { get; set; }

        [DefinitionLabelDisplay(Name = "Contract_StartDate", Language = Language.Ternary)]
        public string StartDateLabel3 { get; set; }

        // TEMP

        public string Prefix { get; set; }

        public byte? CodeWidth { get; set; }

        public bool? IsActive { get; set; } = false;

        // END TEMP

        [VisibilityDisplay(Name = "Contract_Job")]
        [VisibilityChoiceList]
        public string JobVisibility { get; set; }

        [VisibilityDisplay(Name = "Contract_BankAccountNumber")]
        [VisibilityChoiceList]
        public string BankAccountNumberVisibility { get; set; }

        [VisibilityDisplay(Name = "Contract_User")]
        [VisibilityChoiceList]
        public string UserVisibility { get; set; }

        [Display(Name = "ContractDefinition_AllowMultipleUsers")]
        public bool? AllowMultipleUsers { get; set; }

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

    public class ContractDefinition : ContractDefinitionForSave
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
