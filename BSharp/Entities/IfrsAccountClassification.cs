using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    public class IfrsAccountClassification : EntityWithKey<string> // There is no Entity for save
    {
        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public string ParentId { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

        [Display(Name = "IfrsConcepts_IsLeaf")]
        [AlwaysAccessible]
        public bool? IsLeaf { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation { get; set; }

        [MultilingualDisplay(Name = "IfrsoConcepts_Documentation", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation3 { get; set; }

        [Display(Name = "IfrsConcepts_EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "IfrsConcepts_ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        // Specific to IFRS Account Classification

        // TODO:

        public string StatementClassificationId { get; set; }
        public string DebitCashFlowClassificationId { get; set; }
        public string CreditCashFlowClassificationId { get; set; }
        public string IfrsNoteSetting { get; set; }

        /*
            [StatementClassificationId]			NVARCHAR (255)		NULL, -- financial position, comprehensive income
	        [DebitCashFlowClassificationId]		NVARCHAR (255)		NULL,
	        [CreditCashFlowClassificationId]	NVARCHAR (255)		NULL,
            IfrsNoteSetting -- N/A, Optional, Required
         */

        // End specific to IFRS Entry

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        //[Display(Name = "CreatedAt")]
        //public DateTimeOffset? CreatedAt { get; set; }

        //[Display(Name = "CreatedBy")]
        //public int? CreatedById { get; set; }

        //[Display(Name = "ModifiedAt")]
        //public DateTimeOffset? ModifiedAt { get; set; }

        //[Display(Name = "ModifiedBy")]
        //public int? ModifiedById { get; set; }

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [AlwaysAccessible]
        public HierarchyId ParentNode { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public IfrsAccountClassification Parent { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(CreatedById))]
        //public User CreatedBy { get; set; }

        //[Display(Name = "ModifiedBy")]
        //[ForeignKey(nameof(ModifiedById))]
        //public User ModifiedBy { get; set; }
    }
}
