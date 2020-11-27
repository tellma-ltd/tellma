using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Settings", Plural = "Settings")]
    public class FinancialSettingsForSave : Entity
    {
        [Display(Name = "Settings_FunctionalCurrency")]
        [Required]
        [StringLength(3)]
        public string FunctionalCurrencyId { get; set; }

        [Display(Name = "Settings_ArchiveDate")]
        public DateTime? ArchiveDate { get; set; }
    }

    public class FinancialSettings : FinancialSettingsForSave
    {
        [Display(Name = "ModifiedAt")]
        public DateTimeOffset FinancialModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? FinancialModifiedById { get; set; }

        // For Query

        [Display(Name = "Settings_FunctionalCurrency")]
        [ForeignKey(nameof(FunctionalCurrencyId))]
        public Currency FunctionalCurrency { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(FinancialModifiedById))]
        public User FinancialModifiedBy { get; set; }
    }
}
