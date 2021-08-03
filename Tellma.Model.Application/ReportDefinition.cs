using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "ReportDefinition", GroupName = "ReportDefinitions")]
    public class ReportDefinitionForSave<TParameter, TRow, TColumn, TMeasure, TSelect, TRole> : EntityWithKey<int>
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

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(1024)]
        public string Description3 { get; set; }

        [Display(Name = "ReportDefinition_Type")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { 
                "Summary", 
                "Details" },
            new string[] { 
                "ReportDefinition_Type_Summary", 
                "ReportDefinition_Type_Details" })]
        public string Type { get; set; }

        [Display(Name = "ReportDefinition_Chart")]
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
        public bool? DefaultsToChart { get; set; }

        [Display(Name = "ReportDefinition_ChartOptions")]
        [StringLength(1024)]
        public string ChartOptions { get; set; }

        [Display(Name = "ReportDefinition_Collection")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Collection { get; set; }

        [Display(Name = "ReportDefinition_DefinitionId")]
        public int? DefinitionId { get; set; }

        [Display(Name = "ReportDefinition_Filter")]
        [StringLength(1024)]
        public string Filter { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_Having")]
        [StringLength(1024)]
        public string Having { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_OrderBy")]
        [StringLength(1024)]
        public string OrderBy { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_Top")]
        public int? Top { get; set; }

        [Display(Name = "ReportDefinition_ShowColumnsTotal")]
        public bool? ShowColumnsTotal { get; set; }

        [Display(Name = "ReportDefinition_ColumnsTotalLabel")]
        [StringLength(255)]
        public string ColumnsTotalLabel { get; set; }

        [Display(Name = "ReportDefinition_ColumnsTotalLabel")]
        [StringLength(255)]
        public string ColumnsTotalLabel2 { get; set; }

        [Display(Name = "ReportDefinition_ColumnsTotalLabel")]
        [StringLength(255)]
        public string ColumnsTotalLabel3 { get; set; }

        [Display(Name = "ReportDefinition_ShowRowsTotal")]
        public bool? ShowRowsTotal { get; set; }

        [Display(Name = "ReportDefinition_RowsTotalLabel")]
        [StringLength(255)]
        public string RowsTotalLabel { get; set; }

        [Display(Name = "ReportDefinition_RowsTotalLabel")]
        [StringLength(255)]
        public string RowsTotalLabel2 { get; set; }

        [Display(Name = "ReportDefinition_RowsTotalLabel")]
        [StringLength(255)]
        public string RowsTotalLabel3 { get; set; }

        [Display(Name = "ReportDefinition_IsCustomDrilldown")]
        public bool? IsCustomDrilldown { get; set; }

        [Display(Name = "Definition_ShowInMainMenu")]
        public bool? ShowInMainMenu { get; set; }

        [Display(Name = "MainMenuSection")]
        [ChoiceList(new object[] { 
                "Mail", 
                "Financials", 
                "Cash", 
                "FixedAssets", 
                "Inventory", 
                "Production", 
                "Purchasing", 
                "Marketing", 
                "Sales", 
                "HumanCapital", 
                "Investments", 
                "Maintenance", 
                "Administration", 
                "Security", 
                "Studio", 
                "Help" },
            new string[] { 
                "Menu_Mail", 
                "Menu_Financials", 
                "Menu_Cash", 
                "Menu_FixedAssets",
                "Menu_Inventory", 
                "Menu_Production", 
                "Menu_Purchasing", 
                "Menu_Marketing", 
                "Menu_Sales", 
                "Menu_HumanCapital", 
                "Menu_Investments", 
                "Menu_Maintenance", 
                "Menu_Administration",
                "Menu_Security", 
                "Menu_Studio", 
                "Menu_Help" 
            })]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "ReportDefinition_Parameters")]
        [ForeignKey(nameof(ReportDefinitionParameter.ReportDefinitionId))]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "ReportDefinition_Rows")]
        [ForeignKey(nameof(ReportDefinitionRow.ReportDefinitionId))]
        public List<TRow> Rows { get; set; }

        [Display(Name = "ReportDefinition_Columns")]
        [ForeignKey(nameof(ReportDefinitionColumn.ReportDefinitionId))]
        public List<TColumn> Columns { get; set; }

        [Display(Name = "ReportDefinition_Measures")]
        [ForeignKey(nameof(ReportDefinitionMeasure.ReportDefinitionId))]
        public List<TMeasure> Measures { get; set; }

        [Display(Name = "ReportDefinition_Select")]
        [ForeignKey(nameof(ReportDefinitionSelect.ReportDefinitionId))]
        public List<TSelect> Select { get; set; }

        [Display(Name = "Definition_Roles")]
        [ForeignKey(nameof(ReportDefinitionRole.ReportDefinitionId))]
        public List<TRole> Roles { get; set; }
    }

    public class ReportDefinitionForSave : ReportDefinitionForSave<ReportDefinitionParameterForSave, ReportDefinitionRowForSave, ReportDefinitionColumnForSave, ReportDefinitionMeasureForSave, ReportDefinitionSelectForSave, ReportDefinitionRoleForSave>
    {
    }

    public class ReportDefinition : ReportDefinitionForSave<ReportDefinitionParameter, ReportDefinitionRow, ReportDefinitionColumn, ReportDefinitionMeasure, ReportDefinitionSelect, ReportDefinitionRole>
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
