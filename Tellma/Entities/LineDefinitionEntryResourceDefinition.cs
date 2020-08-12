using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntryResourceDefinition", Plural = "LineDefinitionEntryResourceDefinitions")]
    public class LineDefinitionEntryResourceDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryResourceDefinition_ResourceDefinition")]
        [Required]
        public int? ResourceDefinitionId { get; set; }
    }

    public class LineDefinitionEntryResourceDefinition : LineDefinitionEntryResourceDefinitionForSave
    {
        [Display(Name = "LineDefinitionEntryResourceDefinition_LineDefinitionEntry")]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryResourceDefinition_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
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
