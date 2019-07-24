using BSharp.Controllers.Misc;
using BSharp.Services.OData;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [StrongEntity]
    public class IfrsNote : IfrsConcept
    {
        [BasicField]
        public short? Level { get; set; }

        [BasicField]
        public string ParentId { get; set; }

        [BasicField]
        public int? ActiveChildCount { get; set; }

        [BasicField]
        public int? ChildCount { get; set; }

        [Display(Name = "IfrsNotes_IsAggregate")]
        public bool? IsAggregate { get; set; }

        [Display(Name = "IfrsNotes_ForDebit")]
        public bool? ForDebit { get; set; }

        [Display(Name = "IfrsNotes_ForCredit")]
        public bool? ForCredit { get; set; }

        // For Query

        [BasicField]
        public HierarchyId Node { get; set; }

        [BasicField]
        public HierarchyId ParentNode { get; set; }

        [NavigationProperty(ForeignKey = nameof(ParentId))]
        [Display(Name = "TreeParent")]
        public IfrsNote Parent { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUser CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUser ModifiedBy { get; set; }
    }

}
