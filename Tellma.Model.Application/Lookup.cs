using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Lookup", GroupName = "Lookups")]
    public class LookupForSave : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(50)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(50)]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(10)]
        public string Code { get; set; }
    }

    public class Lookup : LookupForSave
    {
        [Required]
        public int? DefinitionId { get; set; }

        public decimal SortKey { get; set; }

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

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public LookupDefinition Definition { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
