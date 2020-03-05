using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    public class LineDefinitionColumnForSave : EntityWithKey<int>
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int? EntryIndex { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        public short? RequiredState { get; set; }
        public short? ReadOnlyState { get; set; }
    }

    public class LineDefinitionColumn : LineDefinitionColumnForSave
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
