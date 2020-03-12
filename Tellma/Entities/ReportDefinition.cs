using Tellma.Data.Queries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Tellma.Entities
{
    [StrongEntity]
    public class ReportDefinitionForSave<TParameter, TRow, TColumn, TMeasure, TSelect> : EntityWithKey<string>
    {
        [MultilingualDisplay(Name = "Title", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Title { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Title2 { get; set; }

        [MultilingualDisplay(Name = "Title", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Title3 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "ReportDefinition_Type")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
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

        [Display(Name = "ReportDefinition_Collection")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Collection { get; set; }

        [Display(Name = "ReportDefinition_DefinitionId")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string DefinitionId { get; set; }

        [Display(Name = "ReportDefinition_Filter")]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Filter { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_OrderBy")]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string OrderBy { get; set; } // On drill down for summary

        [Display(Name = "ReportDefinition_Top")]
        [AlwaysAccessible]
        public int? Top { get; set; }

        [Display(Name = "ReportDefinition_ShowColumnsTotal")]
        [AlwaysAccessible]
        public bool? ShowColumnsTotal { get; set; }

        [Display(Name = "ReportDefinition_ShowRowsTotal")]
        [AlwaysAccessible]
        public bool? ShowRowsTotal { get; set; }

        [Display(Name = "ReportDefinition_ShowInMainMenu")]
        [AlwaysAccessible]
        public bool? ShowInMainMenu { get; set; }

        [Display(Name = "MainMenuSection")]
        [ChoiceList(new object[] { "Financials", "Cash", "FixedAssets", "Inventory", "Production", "Purchasing", "Sales", "HumanCapital", "Investments", "Maintenance", "Administration", "Security", "Studio", "Help" },
            new string[] { "Menu_Financials", "Menu_Cash", "Menu_FixedAssets", "Menu_Inventory", "Menu_Production", "Menu_Purchasing", "Menu_Sales", "Menu_HumanCapital", "Menu_Investments", "Menu_Maintenance", "Menu_Administration", "Menu_Security", "Menu_Studio", "Menu_Help" })]
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
        [ForeignKey(nameof(ReportParameterDefinition.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TParameter> Parameters { get; set; }

        [Display(Name = "ReportDefinition_Rows")]
        [ForeignKey(nameof(ReportRowDefinition.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TRow> Rows { get; set; }

        [Display(Name = "ReportDefinition_Columns")]
        [ForeignKey(nameof(ReportColumnDefinition.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TColumn> Columns { get; set; }

        [Display(Name = "ReportDefinition_Measures")]
        [ForeignKey(nameof(ReportMeasureDefinition.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TMeasure> Measures { get; set; }

        [Display(Name = "ReportDefinition_Select")]
        [ForeignKey(nameof(ReportSelectDefinition.ReportDefinitionId))]
        [AlwaysAccessible]
        public List<TSelect> Select { get; set; }
    }

    public class ReportDefinitionForSave : ReportDefinitionForSave<ReportParameterDefinitionForSave, ReportRowDefinitionForSave, ReportColumnDefinitionForSave, ReportMeasureDefinitionForSave, ReportSelectDefinitionForSave>
    {

    }

    public class ReportDefinition : ReportDefinitionForSave<ReportParameterDefinition, ReportRowDefinition, ReportColumnDefinition, ReportMeasureDefinition, ReportSelectDefinition>
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }

    public class ReportParameterDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Key")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Key { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_Visibility")]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "None", "Optional", "Required" },
            new string[] { "ReportDefinition_Visibility_None", "ReportDefinition_Visibility_Optional", "ReportDefinition_Visibility_Required" })]
        public string Visibility { get; set; }
        
        //// TODO: This will come in handy once we upgrade the filter syntax
        //public string Control { get; set; }
        //// TODO
        //public string Collection { get; set; }

        //[Display(Name = "ReportDefinition_Definition")]
        //public string DefinitionId { get; set; }

        //[Display(Name = "ReportDefinition_Filter")]
        //public string Filter { get; set; }

        //// TODO
        //public int MinDecimalPlaces { get; set; }
        //// TODO
        //public int MaxDecimalPlaces { get; set; }

        [Display(Name = "ReportDefinition_Value")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Value { get; set; }
    }

    public class ReportParameterDefinition : ReportParameterDefinitionForSave
    {
        public string ReportDefinitionId { get; set; }

        public int? Index { get; set; }
    }

    public class ReportSelectDefinitionForSave : EntityWithKey<int>
    {

        [Display(Name = "ReportDefinition_Path")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Path { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }
    }

    public class ReportSelectDefinition : ReportSelectDefinitionForSave
    {
        public string ReportDefinitionId { get; set; }

        public int? Index { get; set; }
    }

    public abstract class ReportDimensionDefinition : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Path")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Path { get; set; }

        [Display(Name = "ReportDefinition_Function")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        [ChoiceList(new object[] { 
            Modifiers.year, 
            Modifiers.quarter, 
            Modifiers.month, 
            Modifiers.dayofyear, 
            Modifiers.day,
            Modifiers.week,
            Modifiers.weekday
        }, new string[] {
            "Function_year",
            "Function_quarter",
            "Function_month",
            "Function_dayofyear",
            "Function_day",
            "Function_week",
            "Function_weekday"
        })]
        public string Modifier { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_Asc", "ReportDefinition_OrderDirection_Desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "ReportDefinition_AutoExpand")]
        [AlwaysAccessible]
        public bool? AutoExpand { get; set; }
    }

    public class ReportColumnDefinitionForSave : ReportDimensionDefinition
    {

    }

    public class ReportColumnDefinition : ReportColumnDefinitionForSave
    {
        public string ReportDefinitionId { get; set; }

        public int? Index { get; set; }
    }

    public class ReportRowDefinitionForSave : ReportDimensionDefinition
    {

    }

    public class ReportRowDefinition : ReportRowDefinitionForSave
    {
        public string ReportDefinitionId { get; set; }

        public int? Index { get; set; }
    }

    public class ReportMeasureDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "ReportDefinition_Path")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Path { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Primary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "Label", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [Display(Name = "ReportDefinition_OrderDirection")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "asc", "desc" },
            new string[] { "ReportDefinition_OrderDirection_asc", "ReportDefinition_OrderDirection_desc" })]
        public string OrderDirection { get; set; }

        [Display(Name = "ReportDefinition_Aggregation")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [AlwaysAccessible]
        [ChoiceList(new object[] { "count", "sum", "avg", "max", "min" },
            new string[] {
                "ReportDefinition_Aggregation_count",
                "ReportDefinition_Aggregation_sum",
                "ReportDefinition_Aggregation_avg",
                "ReportDefinition_Aggregation_max",
                "ReportDefinition_Aggregation_min"
            })]
        public string Aggregation { get; set; }
    }

    public class ReportMeasureDefinition : ReportMeasureDefinitionForSave
    {
        public string ReportDefinitionId { get; set; }

        public int? Index { get; set; }
    }
}
