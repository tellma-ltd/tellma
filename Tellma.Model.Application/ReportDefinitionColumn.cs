using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    public abstract class ReportDefinitionDimension<TAttribute> : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_KeyExpression")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string KeyExpression { get; set; }

        [Display(Name = "ReportDefinition_DisplayExpression")]
        [StringLength(255)]
        public string DisplayExpression { get; set; }

        [Display(Name = "ReportDefinition_Localize")]
        [Required]
        public bool? Localize { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label2 { get; set; }

        [Display(Name = "Label")]
        [StringLength(255)]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [ChoiceList(new object[] {
                "asc",
                "desc"
            }, new string[] {
                "ReportDefinition_OrderDirection_Asc",
                "ReportDefinition_OrderDirection_Desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "ReportDefinition_AutoExpand")]
        public int? AutoExpandLevel { get; set; }

        [Display(Name = "ReportDefinition_ShowAsTree")]
        [Required]
        public bool? ShowAsTree { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        public string ControlOptions { get; set; }

        [Display(Name = "ReportDefinition_Attributes")]
        [ForeignKey(nameof(ReportDefinitionDimensionAttribute.ReportDefinitionDimensionId))]
        public List<TAttribute> Attributes { get; set; }
    }

    public class ReportDefinitionColumnForSave : ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave>
    {

    }

    public class ReportDefinitionColumn : ReportDefinitionDimension<ReportDefinitionDimensionAttribute>
    {
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }
    }
}
