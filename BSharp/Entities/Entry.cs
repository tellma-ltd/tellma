using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class EntryForSave : EntityWithKey<int>
    {
        [AlwaysAccessible]
        public int? EntryNumber { get; set; }

        [AlwaysAccessible]
        [ChoiceList(new object[] { -1, 1 })]
        public byte? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_IfrsEntryClassification")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string IfrsEntryClassificationId { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Entry_ResponsibilityCenter")]
        public int? ResponsibilityCenterId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_BatchCode")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string BatchCode { get; set; }

        public DateTime? DueDate { get; set; } // TODO

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_Mass")]
        public decimal? Mass { get; set; }

        [Display(Name = "Entry_Volume")]
        public decimal? Volume { get; set; }

        [Display(Name = "Entry_Area")]
        public decimal? Area { get; set; }

        [Display(Name = "Entry_Length")]
        public decimal? Length { get; set; }

        [Display(Name = "Entry_Time")]
        public decimal? Time { get; set; }

        [Display(Name = "Entry_Count")]
        public decimal? Count { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }
    }

    public class Entry : EntryForSave
    {
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

        [Display(Name = "Entry_IfrsEntryClassification")]
        [ForeignKey(nameof(IfrsEntryClassificationId))]
        public IfrsEntryClassification IfrsEntryClassification { get; set; }

        [Display(Name = "Entry_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Entry_ResponsibilityCenter")]
        [ForeignKey(nameof(ResponsibilityCenterId))]
        public ResponsibilityCenter ResponsibilityCenter { get; set; }

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
