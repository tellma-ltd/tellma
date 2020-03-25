using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class ExchangeRateForSave : EntityWithKey<int>
    {
        [Display(Name = "ExchangeRate_Currency")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string CurrencyId { get; set; }

        [Display(Name = "ExchangeRate_ValidAsOf")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [AlwaysAccessible]
        public DateTime? ValidAsOf { get; set; }

        [Display(Name = "ExchangeRate_AmountInCurrency")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [AlwaysAccessible]
        public decimal? AmountInCurrency { get; set; }

        [Display(Name = "ExchangeRate_AmountInFunctional")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [AlwaysAccessible]
        public decimal? AmountInFunctional { get; set; }
    }

    public class ExchangeRate : ExchangeRateForSave
    {
        [Display(Name = "ExchangeRate_Rate")]
        public decimal? Rate { get; set; }

        [Display(Name = "ExchangeRate_ValidTill")]
        public DateTime? ValidTill { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
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
