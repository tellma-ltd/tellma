using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class LineForSave<TEntry> : EntityWithKey<int>
    {
        [Display(Name = "Definition")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string DefinitionId { get; set; }

        // HIDDEN

        public int? ResponsibilityCenterId { get; set; } // TODO: Display

        [Display(Name = "Line_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Line_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Line_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "Line_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Line_Quantity")]
        public decimal? Quantity { get; set; }

        [Display(Name = "Line_Unit")]
        public int? UnitId { get; set; }

        public decimal? Value { get; set; } // TODO: Display

        // END HIDDEN

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }        

        [ForeignKey(nameof(Entry.LineId))]
        public List<TEntry> Entries { get; set; }
    }

    public class LineForSave : LineForSave<EntryForSave>
    {

    }

    public class Line : LineForSave<Entry>
    {
        public int? DocumentId { get; set; }

        [Display(Name = "State")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            LineState.Draft,
            LineState.Void,
            LineState.Requested,
            LineState.Rejected,
            LineState.Authorized,
            LineState.Failed,
            LineState.Completed,
            LineState.Invalid,
            LineState.Finalized
        },
            new string[] {
            LineStateName.Draft,
            LineStateName.Void,
            LineStateName.Requested,
            LineStateName.Rejected,
            LineStateName.Authorized,
            LineStateName.Failed,
            LineStateName.Completed,
            LineStateName.Invalid,
            LineStateName.Finalized
        })]
        public short? State { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        public int? SortKey { get; set; }

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

        [Display(Name = "Line_Unit")]
        [ForeignKey(nameof(UnitId))]
        public MeasurementUnit Unit { get; set; }

        [Display(Name = "Line_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Line_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Line_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }
    }
}
