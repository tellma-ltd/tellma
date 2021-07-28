using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionEntryResourceDefinition", GroupName = "LineDefinitionEntryResourceDefinitions")]
    public class LineDefinitionEntryResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryResourceDefinition_ResourceDefinition")]
        [Required, ValidateRequired]
        public int? ResourceDefinitionId { get; set; }
    }

    public class LineDefinitionEntryResourceDefinition : LineDefinitionEntryResourceDefinitionForSave
    {
        [Display(Name = "Entity_LineDefinitionEntry")]
        [Required]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryResourceDefinition_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        [Required]
        public ResourceDefinition ResourceDefinition { get; set; }

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

        // For query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
