using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionStateReason", Plural = "LineDefinitionStateReasons")]
    public class LineDefinitionStateReasonForSave : EntityWithKey<int>
    {
        [Display(Name = "State")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
            LineState.Void,
            LineState.Rejected,
            LineState.Failed,
            LineState.Invalid
        },
            new string[] {
            LineStateName.Void,
            LineStateName.Rejected,
            LineStateName.Failed,
            LineStateName.Invalid
        })]
        [Required]
        [NotNull]
        public short? State { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        [NotNull]
        public bool? IsActive { get; set; }
    }

    public class LineDefinitionStateReason : LineDefinitionStateReasonForSave
    {
        [AlwaysAccessible]
        [NotNull]
        public int? Index { get; set; }

        [Display(Name = "StateReason_LineDefinition")]
        [NotNull]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
