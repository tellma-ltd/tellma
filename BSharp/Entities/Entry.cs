using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class EntryForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? LineIndex { get; set; }

        [NotMapped]
        public int? DocumentIndex { get; set; }

        [AlwaysAccessible]
        public int? EntryNumber { get; set; }

        [AlwaysAccessible]
        [ChoiceList(new object[] { (short)-1, (short)1 })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_ResponsibilityCenter")]
        public int? ResponsibilityCenterId { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; } // EntryTypeId

        [Display(Name = "Entry_DueDate")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_Count")]
        public decimal? Count { get; set; }

        [Display(Name = "Entry_Mass")]
        public decimal? Mass { get; set; }

        [Display(Name = "Entry_Volume")]
        public decimal? Volume { get; set; }

        [Display(Name = "Entry_Time")]
        public decimal? Time { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Entry_Time1")]
        public TimeSpan? Time1 { get; set; }

        [Display(Name = "Entry_Time2")]
        public TimeSpan? Time2 { get; set; }
    }

    public class Entry : EntryForSave
    {
        public int? LineId { get; set; }
        
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Entry_Account")]
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Display(Name = "Entry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "Entry_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Entry_ResponsibilityCenter")]
        [ForeignKey(nameof(ResponsibilityCenterId))]
        public ResponsibilityCenter ResponsibilityCenter { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
