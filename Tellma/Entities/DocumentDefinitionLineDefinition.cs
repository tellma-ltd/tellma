using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "DocumentDefinitionLineDefinition", Plural = "DocumentDefinitionLineDefinitions")]
    public class DocumentDefinitionLineDefinitionForSave : EntityWithKey<int>
    {
        public string LineDefinitionId { get; set; }
        public bool? IsVisibleByDefault { get; set; }
    }

    public class DocumentDefinitionLineDefinition : DocumentDefinitionLineDefinitionForSave
    {
        public string DocumentDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        [ForeignKey(nameof(LineDefinitionId))]
        public LineDefinition LineDefinition { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
