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
        [NotNull]
        public int? ResourceDefinitionId { get; set; }
    }

    public class LineDefinitionEntryResourceDefinition : LineDefinitionEntryResourceDefinitionForSave
    {
        [Display(Name = "LineDefinitionEntryResourceDefinition_LineDefinitionEntry")]
        [NotNull]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryResourceDefinition_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        [NotNull]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
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
