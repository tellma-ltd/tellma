using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class DocumentLineForSave<TDocumentLineEntry> : EntityWithKey<int>
    {
        [Display(Name = "DocumentLine_LineType")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string LineTypeId { get; set; }

        // TODO: Missing fields
        /*
         * 
    [TemplateLineId]		INT, -- depending on the line type, the user may/may not be allowed to edit
	[ScalingFactor]			FLOAT, -- Qty sold for Price list, Qty produced for BOM
	[AgentId]				INT, -- useful for storing the conversion agent in conversion transactions
         * 
         */

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }

        [Display(Name = "DocumentLine_ExternalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ExternalReference { get; set; }

        [Display(Name = "DocumentLine_AdditionalReference")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string AdditionalReference { get; set; }

        [Display(Name = "DocumentLine_RelatedResource")]
        public int? RelatedResourceId { get; set; }

        [Display(Name = "DocumentLine_RelatedAccount")]
        public int? RelatedAccountId { get; set; }

        [Display(Name = "DocumentLine_RelatedQuantity")]
        public decimal? RelatedQuantity { get; set; }

        [Display(Name = "DocumentLine_RelatedMoneyAmount")]
        public decimal? RelatedMoneyAmount { get; set; }

        public List<TDocumentLineEntry> Entries { get; set; }
    }

    public class DocumentLineForSave : DocumentLineForSave<DocumentLineEntryForSave>
    {

    }

    public class DocumentLine : DocumentLineForSave<DocumentLineEntry>
    {
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

        // TODO
        //[Display(Name = "DocumentLine_LineType")]
        //[ForeignKey(nameof(LineTypeId))]
        //public LineType LineType { get; set; }

        [Display(Name = "DocumentLine_RelatedResource")]
        [ForeignKey(nameof(RelatedResourceId))]
        public Resource RelatedResource { get; set; }

        [Display(Name = "DocumentLine_RelatedAccount")]
        [ForeignKey(nameof(RelatedAccountId))]
        public Account RelatedAccount { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
