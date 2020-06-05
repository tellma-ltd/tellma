using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionEntry", Plural = "LineDefinitionEntries")]
    public class LineDefinitionEntryForSave : EntityWithKey<int>
    {
        public short? Direction { get; set; }

        [AlwaysAccessible]
        public int? AccountTypeId { get; set; }

        public int? ContractDefinitionId { get; set; }

        public int? NotedContractDefinitionId { get; set; }

        public int? ResourceDefinitionId { get; set; }

        public int? EntryTypeId { get; set; }
    }

    public class LineDefinitionEntry : LineDefinitionEntryForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
