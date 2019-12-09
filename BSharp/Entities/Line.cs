using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class LineForSave<TEntry> : EntityWithKey<int>
    {
        [NotMapped]
        public int? DocumentIndexId { get; set; }

        [Display(Name = "Definition")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string DefinitionId { get; set; }

        // HIDDEN

        [Display(Name = "Line_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "Line_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Line_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Line_Amount")]
        public decimal Amount { get; set; }

        // END HIDDEN

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }        

        [Display(Name = "Line_ExternalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReference { get; set; } // HIDDEN

        [Display(Name = "Line_AdditionalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReference { get; set; } // HIDDEN

        public List<TEntry> Entries { get; set; }
    }

    public class LineForSave : LineForSave<EntryForSave>
    {

    }

    public class Line : LineForSave<Entry>
    {
        [Display(Name = "State")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            DocState.Draft,
            DocState.Void,
            DocState.Requested,
            DocState.Rejected,
            DocState.Authorized,
            DocState.Failed,
            DocState.Completed,
            DocState.Invalid,
            DocState.Reviewed,
            DocState.Closed
        },
            new string[] {
            DocStateName.Draft,
            DocStateName.Void,
            DocStateName.Requested,
            DocStateName.Rejected,
            DocStateName.Authorized,
            DocStateName.Failed,
            DocStateName.Completed,
            DocStateName.Invalid,
            DocStateName.Reviewed,
            DocStateName.Closed
        })]
        public int State { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        public decimal? SortKey { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        // HIDDEN

        [Display(Name = "Line_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Line_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Line_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }
    }
}
