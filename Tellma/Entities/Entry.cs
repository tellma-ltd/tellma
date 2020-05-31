﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Entry", Plural = "Entries")]
    public class EntryForSave : EntityWithKey<int>
    {
        [Display(Name = "Entry_Direction")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { (short)-1, (short)1 })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_Center")]
        public int? CenterId { get; set; }

        [Display(Name = "Entry_AccountIdentifier")]
        [StringLength(10)]
        public string AccountIdentifier { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; } // EntryTypeId

        [Display(Name = "Entry_DueDate")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Entry_Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Entry_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Entry_Time1")]
        public DateTime? Time1 { get; set; }

        [Display(Name = "Entry_Time2")]
        public DateTime? Time2 { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(255)]
        public string ExternalReference { get; set; }

        [Display(Name = "Entry_AdditionalReference")]
        [StringLength(255)]
        public string AdditionalReference { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        public int? NotedAgentId { get; set; }

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
        [AlwaysAccessible]
        public int? Index { get; set; }
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

        [Display(Name = "Entry_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Entry_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Entry_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Entry_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entry_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        [ForeignKey(nameof(NotedAgentId))]
        public Agent NotedAgent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
