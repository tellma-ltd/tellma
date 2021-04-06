using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "DashboardDefinition", Plural = "DashboardDefinitions")]
    public class DashboardDefinitionForSave<TWidget, TRole> : EntityWithKey<int>
    {
        [Display(Name = "Code")]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Primary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title2 { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title3 { get; set; }

        [Display(Name = "DashboardDefinition_AutoRefreshPeriodInMinutes")]
        [NotNull]
        [AlwaysAccessible]
        public int? AutoRefreshPeriodInMinutes { get; set; }

        [Display(Name = "Definition_ShowInMainMenu")]
        [AlwaysAccessible]
        public bool? ShowInMainMenu { get; set; }

        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "DashboardDefinition_Widgets")]
        [ForeignKey(nameof(DashboardDefinitionWidget.DashboardDefinitionId))]
        [AlwaysAccessible]
        public List<TWidget> Widgets { get; set; }

        [Display(Name = "Definition_Roles")]
        [ForeignKey(nameof(DashboardDefinitionRole.DashboardDefinitionId))]
        [AlwaysAccessible]
        public List<TRole> Roles { get; set; }
    }

    public class DashboardDefinitionForSave : DashboardDefinitionForSave<DashboardDefinitionWidgetForSave, DashboardDefinitionRoleForSave>
    {

    }

    public class DashboardDefinition : DashboardDefinitionForSave<DashboardDefinitionWidget, DashboardDefinitionRole>
    {
        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }

    public class DashboardDefinitionWidgetForSave : EntityWithKey<int>
    {
        [Display(Name = "DashboardDefinition_ReportDefinition")]
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "DashboardDefinition_OffsetX")]
        [NotNull]
        public int? OffsetX { get; set; }

        [Display(Name = "DashboardDefinition_OffsetY")]
        [NotNull]
        public int? OffsetY { get; set; }

        [Display(Name = "DashboardDefinition_Width")]
        [NotNull]
        public int? Width { get; set; }

        [Display(Name = "DashboardDefinition_Height")]
        [NotNull]
        public int? Height { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Primary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title2 { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Title3 { get; set; }

        [Display(Name = "DashboardDefinition_AutoRefreshPeriodInMinutes")]
        [NotNull]
        [AlwaysAccessible]
        public int? AutoRefreshPeriodInMinutes { get; set; }
    }

    public class DashboardDefinitionWidget : DashboardDefinitionWidgetForSave
    {
        [NotNull]
        public int? DashboardDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }

        [Display(Name = "DashboardDefinition_ReportDefinition")]
        [ForeignKey(nameof(ReportDefinitionId))]
        public ReportDefinition ReportDefinition { get; set; }
    }


    public class DashboardDefinitionRoleForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_Role")]
        [NotNull]
        public int? RoleId { get; set; }
    }

    public class DashboardDefinitionRole : DashboardDefinitionRoleForSave
    {
        [NotNull]
        public int? DashboardDefinitionId { get; set; }

        [Display(Name = "Definition_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
