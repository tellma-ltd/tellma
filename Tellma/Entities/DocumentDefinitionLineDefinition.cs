using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentDefinitionLineDefinition", Plural = "DocumentDefinitionLineDefinitions")]
    public class DocumentDefinitionLineDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "DocumentDefinitionLineDefinition_LineDefinition")]
        [Required]
        [NotNull]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "DocumentDefinitionLineDefinition_IsVisibleByDefault")]
        public bool? IsVisibleByDefault { get; set; }
    }

    public class DocumentDefinitionLineDefinition : DocumentDefinitionLineDefinitionForSave
    {
        [AlwaysAccessible]
        [NotNull]
        public int? Index { get; set; }

        [Display(Name = "DocumentDefinition")]
        [NotNull]
        public int? DocumentDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        [Display(Name = "DocumentDefinitionLineDefinition_LineDefinition")]
        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition LineDefinition { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
