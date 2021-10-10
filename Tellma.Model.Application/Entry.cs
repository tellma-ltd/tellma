using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Entry", GroupName = "Entries")]
    public class EntryForSave : EntityWithKey<int>
    {
        [Display(Name = "Entry_Direction")]
        [Required]
        [ChoiceList(new object[] { 
                (short)1, 
                (short)-1 }, 
            new string[] { 
                "Entry_Direction_Debit", 
                "Entry_Direction_Credit" })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Currency")]
        [Required]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        public int? NotedAgentId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_NotedResource")]
        public int? NotedResourceId { get; set; }

        [Display(Name = "Entry_Center")]
        [Required]
        public int? CenterId { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Entry_RValue")]
        public decimal? RValue { get; set; }

        [Display(Name = "Entry_PValue")]
        public decimal? PValue { get; set; }

        [Display(Name = "Entry_Time1")]
        [DataType(DataType.DateTime)]
        public DateTime? Time1 { get; set; }

        [Display(Name = "Entry_Duration")]
        public decimal? Duration { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        public int? DurationUnitId { get; set; }

        [Display(Name = "Entry_Time2")]
        [DataType(DataType.DateTime)]
        public DateTime? Time2 { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(50)]
        public string ExternalReference { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        public int? ReferenceSourceId { get; set; }

        [Display(Name = "Entry_InternalReference")]
        [StringLength(50)]
        public string InternalReference { get; set; }

        [Display(Name = "Entry_NotedAgentName")]
        [StringLength(50)]
        public string NotedAgentName { get; set; }

        [Display(Name = "Entry_NotedAmount")]
        public decimal? NotedAmount { get; set; }

        [Display(Name = "Entry_NotedDate")]
        public DateTime? NotedDate { get; set; }
    }

    public class Entry : EntryForSave
    {
        [Required]
        public int? Index { get; set; }

        [Required]
        public int? LineId { get; set; }

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

        [Display(Name = "Entry_Account")]
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        [ForeignKey(nameof(NotedAgentId))]
        public Agent NotedAgent { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_NotedResource")]
        [ForeignKey(nameof(NotedResourceId))]
        public Resource NotedResource { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_DurationUnit")]
        [ForeignKey(nameof(DurationUnitId))]
        public Unit DurationUnit { get; set; }

        [Display(Name = "Entry_ReferenceSource")]
        [ForeignKey(nameof(ReferenceSourceId))]
        public Agent ReferenceSource { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
