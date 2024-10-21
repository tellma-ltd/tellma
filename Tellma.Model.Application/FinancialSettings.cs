using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Settings", GroupName = "Settings")]
    public class FinancialSettingsForSave : Entity
    {
        [Display(Name = "Settings_FunctionalCurrency")]
        [Required, ValidateRequired]
        [StringLength(3)]
        public string FunctionalCurrencyId { get; set; }

        [Display(Name = "Settings_TIN")]
        [StringLength(50)]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Settings_FirstDayOfPeriod")]
        public byte? FirstDayOfPeriod { get; set; }

        [Display(Name = "Settings_ArchiveDate")]
        public DateTime? ArchiveDate { get; set; }

        [Display(Name = "Settings_FreezeDate")]
        public DateTime? FreezeDate { get; set; }
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
