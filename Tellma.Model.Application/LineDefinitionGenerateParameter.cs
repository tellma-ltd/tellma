using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "LineDefinitionGenerateParameter", GroupName = "LineDefinitionGenerateParameters")]
    public class LineDefinitionGenerateParameterForSave : EntityWithKey<int>
    {
        [Display(Name = "Parameter_Key")]
        [Required]
        [StringLength(50)]
        public string Key { get; set; }

        [Display(Name = "Label")]
        [Required]
        [StringLength(50)]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(50)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(50)]
        public string Label3 { get; set; }

        [Display(Name = "Parameter_Visibility")]
        [Required]
        [VisibilityChoiceList]
        public string Visibility { get; set; }

        [Display(Name = "Definition_Control")]
        [Required]
        [StringLength(50)]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }
    }

    public class LineDefinitionGenerateParameter : LineDefinitionGenerateParameterForSave
    {
        [Required]
        public int? Index { get; set; }

        [Display(Name = "Parameter_LineDefinition")]
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
