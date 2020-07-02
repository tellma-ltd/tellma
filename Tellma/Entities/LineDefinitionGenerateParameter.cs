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

        [Display(Name = "Parameter_DataType")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string DataType { get; set; }

        [Display(Name = "Parameter_Filter")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Filter { get; set; }
    }

    public class LineDefinitionGenerateParameter : LineDefinitionGenerateParameterForSave
    {
        public int? LineDefinitionId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
