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
        public short? State { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
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
        public bool? IsActive { get; set; }
    }

    public class LineDefinitionStateReason : LineDefinitionStateReasonForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        [Display(Name = "StateReason_LineDefinition")]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
