using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "AccountClassification", GroupName = "AccountClassifications")]
    public class AccountClassificationForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [SelfReferencing(nameof(ParentIndex))]
        public int? ParentId { get; set; }

        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Code { get; set; } // The basis of the tree structure

        [Display(Name = "AccountClassification_AccountTypeParent")]
        public int? AccountTypeParentId { get; set; }
    }

    public class AccountClassification : AccountClassificationForSave
    {
        [Required]
        public short? Level { get; set; }

        [Required]
        public int? ActiveChildCount { get; set; }

        [Required]
        public int? ChildCount { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

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
        public HierarchyId Node { get; set; }

        [Display(Name = "AccountClassification_AccountTypeParent")]
        [ForeignKey(nameof(AccountTypeParentId))]
        public AccountType AccountTypeParent { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public AccountClassification Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
