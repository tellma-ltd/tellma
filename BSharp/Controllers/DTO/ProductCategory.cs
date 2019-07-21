using BSharp.Controllers.Misc;
using BSharp.Services.OData;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [StrongDto]
    public class ProductCategoryForSave : DtoForSaveKeyBase<int?>
    {
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int? ParentIndex { get; set; }

        [BasicField]
        public int? ParentId { get; set; }

        [BasicField]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        public string Name3 { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Code")]
        public string Code { get; set; }
    }

    public class ProductCategory : ProductCategoryForSave
    {
        [BasicField]
        public short? Level { get; set; }

        [BasicField]
        public int? ActiveChildCount { get; set; }

        [BasicField]
        public int? ChildCount { get; set; }

        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey]
        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [ForeignKey]
        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [BasicField]
        public HierarchyId Node { get; set; }

        [BasicField]
        public HierarchyId ParentNode { get; set; }

        [NavigationProperty(ForeignKey = nameof(ParentId))]
        [Display(Name = "TreeParent")]
        public ProductCategory Parent { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUser CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUser ModifiedBy { get; set; }
    }
}
