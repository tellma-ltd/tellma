using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AgentDefinition", Plural = "AgentDefinitions")] // TODO: Add to translations file
    public class AgentDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "TitleSingular", Language = Language.Primary)]
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

        public string TaxIdentificationNumberVisibility { get; set; }
        public string ImageVisibility { get; set; }
        public string StartDateVisibility { get; set; }
        public string StartDateLabel { get; set; }
        public string StartDateLabel2 { get; set; }
        public string StartDateLabel3 { get; set; }
        public string JobVisibility { get; set; }
        public string BankAccountNumberVisibility { get; set; }

        // ---------------

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

    public class AgentDefinition : AgentDefinitionForSave
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
