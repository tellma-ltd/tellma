using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class DetailsEntry : EntityWithKey<int>
    {
        [Display(Name = "Entry_Line")]
        public int? LineId { get; set; }

        [Display(Name = "Entry_ResponsibilityCenter")]
        public int? ResponsibilityCenterId { get; set; }

        [Display(Name = "Entry_Direction")]
        [ChoiceList(new object[] { (short)-1, (short)1 })]
        public short? Direction { get; set; }

        [Display(Name = "Entry_Account")]
        public int? AccountId { get; set; }

        [Display(Name = "Entry_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Entry_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Entry_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Entry_DueDate")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Entry_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicMonetaryValue")]
        public decimal? AlgebraicMonetaryValue { get; set; }

        [Display(Name = "Entry_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "DetailsEntry_Count")]
        public decimal? Count { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicCount")]
        public decimal? AlgebraicCount { get; set; }

        [Display(Name = "DetailsEntry_Mass")]
        public decimal? Mass { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicMass")]
        public decimal? AlgebraicMass { get; set; }

        [Display(Name = "DetailsEntry_Volume")]
        public decimal? Volume { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicVolume")]
        public decimal? AlgebraicVolume { get; set; }

        [Display(Name = "DetailsEntry_Time")]
        public decimal? Time { get; set; }

        [Display(Name = "DetailsEntry_AlgebraicTime")]
        public decimal? AlgebraicTime { get; set; }

        [Display(Name = "Entry_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "Entry_AlgebraicValue")]
        public decimal? AlgebraicValue { get; set; }

        [Display(Name = "Entry_ExternalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReference { get; set; }

        [Display(Name = "Entry_AdditionalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReference { get; set; }

        [Display(Name = "Entry_NotedAgent")]
        public int? NotedAgentId { get; set; }

        [Display(Name = "Entry_NotedAgentName")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string NotedAgentName { get; set; }

        [Display(Name = "Entry_NotedAmount")]
        public decimal? NotedAmount { get; set; }

        [Display(Name = "Entry_NotedDate")]
        public DateTime? NotedDate { get; set; }

        // For Query

        [Display(Name = "Entry_Line")]
        [ForeignKey(nameof(LineId))]
        public Line Line { get; set; }

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

        [Display(Name = "Entry_NotedAgent")]
        [ForeignKey(nameof(NotedAgentId))]
        public Agent NotedAgent { get; set; }
    }
}
