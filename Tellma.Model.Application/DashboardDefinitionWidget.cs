using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DashboardDefinitionWidget", GroupName = "DashboardDefinitionWidgets")]
    public class DashboardDefinitionWidgetForSave : EntityWithKey<int>
    {
        [Display(Name = "DashboardDefinition_ReportDefinition")]
        [Required]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "DashboardDefinition_OffsetX")]
        [Required]
        public int? OffsetX { get; set; }

        [Display(Name = "DashboardDefinition_OffsetY")]
        [Required]
        public int? OffsetY { get; set; }

        [Display(Name = "DashboardDefinition_Width")]
        [Required]
        public int? Width { get; set; }

        [Display(Name = "DashboardDefinition_Height")]
        [Required]
        public int? Height { get; set; }

        [Display(Name = "Title")]
        [StringLength(50)]
        public string Title { get; set; }

        [Display(Name = "Title")]
        [StringLength(50)]
        public string Title2 { get; set; }

        [Display(Name = "Title")]
        [StringLength(50)]
        public string Title3 { get; set; }

        [Display(Name = "DashboardDefinition_AutoRefreshPeriodInMinutes")]
        [Required]
        public int? AutoRefreshPeriodInMinutes { get; set; }
    }

    public class DashboardDefinitionWidget : DashboardDefinitionWidgetForSave
    {
        [Required]
        public int? DashboardDefinitionId { get; set; }

        [Required]
        public int? Index { get; set; }

        [Display(Name = "DashboardDefinition_ReportDefinition")]
        [ForeignKey(nameof(ReportDefinitionId))]
        public ReportDefinition ReportDefinition { get; set; }
    }
}
