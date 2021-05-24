using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "ExchangeRate", GroupName = "ExchangeRates")]
    public class ExchangeRateForSave : EntityWithKey<int>
    {
        [Display(Name = "ExchangeRate_Currency")]
        [Required]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "ExchangeRate_ValidAsOf")]
        [Required]
        public DateTime? ValidAsOf { get; set; }

        [Display(Name = "ExchangeRate_AmountInCurrency")]
        [Required]
        public decimal? AmountInCurrency { get; set; }

        [Display(Name = "ExchangeRate_AmountInFunctional")]
        [Required]
        public decimal? AmountInFunctional { get; set; }
    }

    public class ExchangeRate : ExchangeRateForSave
    {
        [Display(Name = "ExchangeRate_Rate")]
        [Required]
        public decimal? Rate { get; set; }

        [Display(Name = "ExchangeRate_ValidTill")]
        [Required]
        public DateTime? ValidTill { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query
        [Display(Name = "ExchangeRate_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
