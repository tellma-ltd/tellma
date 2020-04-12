using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class LineDefinitionEntryForSave : EntityWithKey<int>
    {
        public short? Direction { get; set; }

        [AlwaysAccessible]
        public int? AccountTypeParentId { get; set; }

        public int? EntryTypeId { get; set; }
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }
        public string LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
