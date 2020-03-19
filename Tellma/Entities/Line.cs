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
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string DefinitionId { get; set; }

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

        public int? Index { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Line_Document")]
        [ForeignKey(nameof(DocumentId))]
        public Document Document { get; set; }
    }
}
