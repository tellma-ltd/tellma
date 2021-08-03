using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "DashboardDefinition", GroupName = "DashboardDefinitions")]
    public class DashboardDefinitionForSave<TWidget, TRole> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [Required]
        [StringLength(50)]
        public string Code { get; set; }

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

        [Display(Name = "Definition_ShowInMainMenu")]
        public bool? ShowInMainMenu { get; set; }

        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "DashboardDefinition_Widgets")]
        [ForeignKey(nameof(DashboardDefinitionWidget.DashboardDefinitionId))]
        public List<TWidget> Widgets { get; set; }

        [Display(Name = "Definition_Roles")]
        [ForeignKey(nameof(DashboardDefinitionRole.DashboardDefinitionId))]
        public List<TRole> Roles { get; set; }
    }

    public class DashboardDefinitionForSave : DashboardDefinitionForSave<DashboardDefinitionWidgetForSave, DashboardDefinitionRoleForSave>
    {
    }

    public class DashboardDefinition : DashboardDefinitionForSave<DashboardDefinitionWidget, DashboardDefinitionRole>
    {
        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
