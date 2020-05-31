using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Line", Plural = "Lines")]
    public class LineForSave<TEntry> : EntityWithKey<int>
    {
        [Display(Name = "Definition")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string DefinitionId { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
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

    public static class LineState
    {
        public const short Draft = 0;
        public const short Void = -1;
        public const short Requested = 1;
        public const short Rejected = -2;
        public const short Authorized = 2;
        public const short Failed = -3;
        public const short Completed = 3;
        public const short Invalid = -4;
        public const short Finalized = 4;
    }

    public static class LineStateName
    {
        private const string _prefix = "Line_State_";

        public const string Draft = _prefix + "0";
        public const string Void = _prefix + "minus_1";
        public const string Requested = _prefix + "1";
        public const string Rejected = _prefix + "minus_2";
        public const string Authorized = _prefix + "2";
        public const string Failed = _prefix + "minus_3";
        public const string Completed = _prefix + "3";
        public const string Invalid = _prefix + "minus_4";
        public const string Finalized = _prefix + "4";
    }
}
