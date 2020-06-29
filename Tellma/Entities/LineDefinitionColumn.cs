using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionColumn", Plural = "LineDefinitionColumns")]
    public class LineDefinitionColumnForSave : EntityWithKey<int>
    {
        public string ColumnName { get; set; }

        public int? EntryIndex { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        public short? RequiredState { get; set; }

        public short? ReadOnlyState { get; set; }

        public bool? InheritsFromHeader { get; set; }
    }

    public class LineDefinitionColumn : LineDefinitionColumnForSave
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
