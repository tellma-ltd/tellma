using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "LineDefinitionGenerateParameter", Plural = "LineDefinitionGenerateParameters")]
    public class LineDefinitionGenerateParameterForSave : EntityWithKey<int>
    {
        [Display(Name = "Parameter_Key")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Key { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "Parameter_Visibility")]
        [AlwaysAccessible]
        [VisibilityChoiceList]
        public string Visibility { get; set; }

        [Display(Name = "Definition_Control")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string ControlOptions { get; set; }
    }

    public class LineDefinitionGenerateParameter : LineDefinitionGenerateParameterForSave
    {
        [AlwaysAccessible]
        public int? Index { get; set; }

        [Display(Name = "Parameter_LineDefinition")]
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
