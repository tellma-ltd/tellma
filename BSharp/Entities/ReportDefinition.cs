using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class ReportDefinitionForSave<TParameter,TRow, TColumn, TMeasure, TSelect> : EntityWithKey<string>
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
        [ChoiceList(new object[] { "Card", "BarsVertical", "BarsHorizontal", "Line", "Pie" }, // TODO Add the rest, here and in TypeScript
            new string[] { "ReportDefinition_Chart_Card", "ReportDefinition_Chart_BarsVertical", "ReportDefinition_Chart_BarsHorizontal", "ReportDefinition_Chart_Line", "ReportDefinition_Chart_Pie" })]
        public string Chart { get; set; }

        [Display(Name = "ReportDefinition_DefaultsToChart")]
        [AlwaysAccessible]
        public bool? DefaultsToChart { get; set; }

        [Display(Name = "ReportDefinition_Collection")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Collection { get; set; }

        [Display(Name = "ReportDefinition_DefinitionId")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
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

        [Display(Name = "MainMenuSection")]
        [ChoiceList(new object[] { "Financials", "Administration" }, // TODO Add the rest, here and in TypeScript
            new string[] { "Financials", "Administration" })]
        [AlwaysAccessible]
        public string MainMenuSection { get; set; }

        [Display(Name = "MainMenuIcon")]
        [ChoiceList(new object[] { "clipboard", "chart-pie" }, // TODO Add the rest, here and in TypeScript
            new string[] { "Icon_Clipboard", "Icon_ChartPie" })]
        [AlwaysAccessible]
        public string MainMenuIcon { get; set; }

        [Display(Name = "MainMenuSortKey")]
        [AlwaysAccessible]
        public decimal? MainMenuSortKey { get; set; }

        [Display(Name = "Definition_State")]
        [ChoiceList(new object[] { "Draft", "Deployed", "Archived" },
            new string[] { "Definition_State_Draft", "Definition_State_Deployed", "Definition_State_Archived" })]
        [AlwaysAccessible]
        public string State { get; set; }

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

    }

    public class ReportParameterDefinitionForSave : EntityWithKey<int>
    {
        public string ReportDefinitionId { get; set; }

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

        [Display(Name = "ReportDefinition_IsRequired")]
        [AlwaysAccessible]
        public bool? IsRequired { get; set; }
    }

    public class ReportParameterDefinition : ReportParameterDefinitionForSave
    {

    }

    public class ReportSelectDefinitionForSave : EntityWithKey<int>
    {
        public string ReportDefinitionId { get; set; }

        [Display(Name = "ReportDefinition_Path")]
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

    }
    public abstract class ReportDimensionDefinition : EntityWithKey<int>
    {
        public string ReportDefinitionId { get; set; }

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

    }
    public class ReportRowDefinitionForSave : ReportDimensionDefinition
    {

    }
    public class ReportRowDefinition : ReportRowDefinitionForSave
    {

    }

    public class ReportMeasureDefinitionForSave : EntityWithKey<int>
    {
        public string ReportDefinitionId { get; set; }

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

    }
}
