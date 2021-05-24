using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionStateReason", GroupName = "LineDefinitionStateReasons")]
    public class LineDefinitionStateReasonForSave : EntityWithKey<int>
    {
        [Display(Name = "State")]
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

        [Display(Name = "Name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }
    }

    public class LineDefinitionStateReason : LineDefinitionStateReasonForSave
    {
        [Required]
        public int? Index { get; set; }

        [Display(Name = "StateReason_LineDefinition")]
        [Required]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
