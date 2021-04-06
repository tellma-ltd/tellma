using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "ReportDefinition", Plural = "ReportDefinitions")]
    public class ReportDefinitionForSave<TParameter, TRow, TColumn, TMeasure, TSelect, TRole> : EntityWithKey<int>
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

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "ReportDefinition_Type")]
        [Required]
        [NotNull]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "Summary", "Details" },
            new string[] { "ReportDefinition_Type_Summary", "ReportDefinition_Type_Details" })]
        public string Type { get; set; }

        [Display(Name = "ReportDefinition_Chart")]
        [AlwaysAccessible]
        [ChoiceList(new object[] {
                // 0 Dimensions
                "Card",
                // 1 Dimension
                "BarsVertical", "BarsHorizontal", "Pie", "Doughnut", "TreeMap", "NumberCards", "Gauge",
                // 1 or 2 Dimensions
                "Line", "Area",
                // 2 Dimensions
                "BarsVerticalGrouped", "BarsVerticalStacked", "BarsVerticalNormalized", "BarsHorizontalGrouped",
                "BarsHorizontalStacked", "BarsHorizontalNormalized", "HeatMap"
            },
            new string[] {
                // 0 Dimensions
                "ReportDefinition_Chart_Card",
                // 1 Dimension
                "ReportDefinition_Chart_BarsVertical", "ReportDefinition_Chart_BarsHorizontal", "ReportDefinition_Chart_Pie", "ReportDefinition_Chart_Doughnut", "ReportDefinition_Chart_TreeMap", "ReportDefinition_Chart_NumberCards", "ReportDefinition_Chart_Gauge",
                // 1 or 2 Dimensions
                "ReportDefinition_Chart_Line", "ReportDefinition_Chart_Area",
                // 2 Dimensions
                "ReportDefinition_Chart_BarsVerticalGrouped", "ReportDefinition_Chart_BarsVerticalStacked", "ReportDefinition_Chart_BarsVerticalNormalized", "ReportDefinition_Chart_BarsHorizontalGrouped",
                "ReportDefinition_Chart_BarsHorizontalStacked", "ReportDefinition_Chart_BarsHorizontalNormalized", "ReportDefinition_Chart_HeatMap"
            })]
        public string Chart { get; set; }

        [Display(Name = "ReportDefinition_DefaultsToChart")]
        [AlwaysAccessible]
        public bool? DefaultsToChart { get; set; }

        [Display(Name = "ReportDefinition_ChartOptions")]
        [AlwaysAccessible]
        [StringLength(1024)]
        public string ChartOptions { get; set; }

        [Display(Name = "ReportDefinition_Collection")]
        [Required]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Collection { get; set; }

        [Display(Name = "ReportDefinition_DefinitionId")]
        [AlwaysAccessible]
        public int? DefinitionId { get; set; }

        [Display(Name = "ReportDefinition_Filter")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Filter { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_Having")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Having { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_OrderBy")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string OrderBy { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_Top")]
        [AlwaysAccessible]
        public int? Top { get; set; }

        [Display(Name = "ReportDefinition_ShowColumnsTotal")]
        [AlwaysAccessible]
        public bool? ShowColumnsTotal { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_ColumnsTotalLabel", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string ColumnsTotalLabel { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_ColumnsTotalLabel", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string ColumnsTotalLabel2 { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_ColumnsTotalLabel", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string ColumnsTotalLabel3 { get; set; }

        [Display(Name = "ReportDefinition_ShowRowsTotal")]
        [AlwaysAccessible]
        public bool? ShowRowsTotal { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_RowsTotalLabel", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string RowsTotalLabel { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_RowsTotalLabel", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string RowsTotalLabel2 { get; set; }

        [MultilingualDisplay(Name = "ReportDefinition_RowsTotalLabel", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string RowsTotalLabel3 { get; set; }

        [Display(Name = "ReportDefinition_IsCustomDrilldown")]
        [AlwaysAccessible]
        public bool? IsCustomDrilldown { get; set; }

        [Display(Name = "Definition_ShowInMainMenu")]
        [AlwaysAccessible]
        public bool? ShowInMainMenu { get; set; }

        [Display(Name = "MainMenuSection")]
        [ChoiceList(new object[] { "Mail", "Financials", "Cash", "FixedAssets", "Inventory", "Production", "Purchasing", "Marketing", "Sales", "HumanCapital", "Investments", "Maintenance", "Administration", "Security", "Studio", "Help" },
            new string[] { "Menu_Mail", "Menu_Financials", "Menu_Cash", "Menu_FixedAssets", "Menu_Inventory", "Menu_Production", "Menu_Purchasing", "Menu_Marketing", "Menu_Sales", "Menu_HumanCapital", "Menu_Investments", "Menu_Maintenance", "Menu_Administration", "Menu_Security", "Menu_Studio", "Menu_Help" })]
        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
       // [ChoiceList(new object[] { "clipboard", "chart-pie" })]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "ReportDefinition_Parameters")]
        [ForeignKey(nameof(ReportDefinitionParameter.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "ReportDefinition_Rows")]
        [ForeignKey(nameof(ReportDefinitionRow.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TRow> Rows { get; set; }

        [Display(Name = "ReportDefinition_Columns")]
        [ForeignKey(nameof(ReportDefinitionColumn.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TColumn> Columns { get; set; }

        [Display(Name = "ReportDefinition_Measures")]
        [ForeignKey(nameof(ReportDefinitionMeasure.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TMeasure> Measures { get; set; }

        [Display(Name = "ReportDefinition_Select")]
        [ForeignKey(nameof(ReportDefinitionSelect.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TSelect> Select { get; set; }

        [Display(Name = "Definition_Roles")]
        [ForeignKey(nameof(ReportDefinitionRole.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TRole> Roles { get; set; }
    }

    public class ReportDefinitionForSave : ReportDefinitionForSave<ReportDefinitionParameterForSave, ReportDefinitionRowForSave, ReportDefinitionColumnForSave, ReportDefinitionMeasureForSave, ReportDefinitionSelectForSave, ReportDefinitionRoleForSave>
    {

    }

    public class ReportDefinition : ReportDefinitionForSave<ReportDefinitionParameter, ReportDefinitionRow, ReportDefinitionColumn, ReportDefinitionMeasure, ReportDefinitionSelect, ReportDefinitionRole>
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

    public class ReportDefinitionParameterForSave : EntityWithKey<int>
    {
        [Display(Name = "Parameter_Key")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Key { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "Parameter_Visibility")]
        [AlwaysAccessible]
        [VisibilityChoiceList]
        public string Visibility { get; set; }

        [Display(Name = "ReportDefinition_DefaultExpression")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string DefaultExpression { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionParameter : ReportDefinitionParameterForSave
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public class ReportDefinitionSelectForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Expression { get; set; }

        [Display(Name = "ReportDefinition_Localize")]
        [NotNull]
        public bool? Localize { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionSelect : ReportDefinitionSelectForSave
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public abstract class ReportDefinitionDimension<TAttribute> : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_KeyExpression")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string KeyExpression { get; set; }

        [Display(Name = "ReportDefinition_DisplayExpression")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string DisplayExpression { get; set; }

        [Display(Name = "ReportDefinition_Localize")]
        [NotNull]
        public bool? Localize { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_Asc", "ReportDefinition_OrderDirection_Desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "ReportDefinition_AutoExpand")]
        [AlwaysAccessible]
        public int? AutoExpandLevel { get; set; }

        [Display(Name = "ReportDefinition_ShowAsTree")]
        [NotNull]
        [AlwaysAccessible]
        public bool? ShowAsTree { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string ControlOptions { get; set; }

        [Display(Name = "ReportDefinition_Attributes")]
        [ForeignKey(nameof(ReportDefinitionDimensionAttribute.ReportDefinitionDimensionId))]
        [AlwaysAccessible]
        public List<TAttribute> Attributes { get; set; }
    }

    public class ReportDefinitionColumnForSave : ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave>
    {

    }

    public class ReportDefinitionColumn : ReportDefinitionDimension<ReportDefinitionDimensionAttribute>
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public class ReportDefinitionRowForSave : ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave>
    {

    }

    public class ReportDefinitionRow : ReportDefinitionDimension<ReportDefinitionDimensionAttribute>
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public class ReportDefinitionDimensionAttributeForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Expression { get; set; }

        [Display(Name = "ReportDefinition_Localize")]
        [NotNull]
        public bool? Localize { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_Asc", "ReportDefinition_OrderDirection_Desc" })]
        public string OrderDirection { get; set; }
    }

    public class ReportDefinitionDimensionAttribute : ReportDefinitionDimensionAttributeForSave
    {
        [NotNull]
        public int? ReportDefinitionDimensionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public class ReportDefinitionMeasureForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Expression")]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Expression { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_asc", "ReportDefinition_OrderDirection_desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "Definition_Control")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Control { get; set; }

        [Display(Name = "Definition_ControlOptions")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string ControlOptions { get; set; }

        [Display(Name = "ReportDefinition_DangerWhen")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string DangerWhen { get; set; }

        [Display(Name = "ReportDefinition_WarningWhen")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string WarningWhen { get; set; }

        [Display(Name = "ReportDefinition_SuccessWhen")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string SuccessWhen { get; set; }
    }

    public class ReportDefinitionMeasure : ReportDefinitionMeasureForSave
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [NotNull]
        public int? Index { get; set; }
    }

    public class ReportDefinitionRoleForSave : EntityWithKey<int>
    {
        [Display(Name = "Definition_Role")]
        [NotNull]
        public int? RoleId { get; set; }
    }

    public class ReportDefinitionRole : ReportDefinitionRoleForSave
    {
        [NotNull]
        public int? ReportDefinitionId { get; set; }

        [Display(Name = "Definition_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }
    }
}
