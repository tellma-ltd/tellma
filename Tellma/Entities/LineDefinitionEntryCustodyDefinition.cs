using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntryCustodyDefinition", Plural = "LineDefinitionEntryCustodyDefinitions")]
    public class LineDefinitionEntryCustodyDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "LineDefinitionEntryCustodyDefinition_CustodyDefinition")]
        [Required]
        [NotNull]
        public int? CustodyDefinitionId { get; set; }
    }

    public class LineDefinitionEntryCustodyDefinition : LineDefinitionEntryCustodyDefinitionForSave
    {
        [Display(Name = "LineDefinitionEntryCustodyDefinition_LineDefinitionEntry")]
        [NotNull]
        public int? LineDefinitionEntryId { get; set; }

        [Display(Name = "LineDefinitionEntryCustodyDefinition_CustodyDefinition")]
        [NotNull]
        [ForeignKey(nameof(CustodyDefinitionId))]
        public CustodyDefinition CustodyDefinition { get; set; }

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
