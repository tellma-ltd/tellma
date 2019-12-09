using System;
using System.Collections.Generic;

namespace BSharp.Controllers.Dto
{
    /// <summary>
    /// A structure that stores all definitions of a particular database
    /// </summary>
    public class DefinitionsForClient
    {
        /// <summary>
        /// Mapping from document definition Id to document definition
        /// </summary>
        public Dictionary<string, DocumentDefinitionForClient> Documents { get; set; }

        /// <summary>
        /// Mapping from line type Id to line type
        /// </summary>
        public Dictionary<string, LineTypeForClient> Lines { get; set; }

        /// <summary>
        /// Mapping from resource definition Id to resource definition
        /// </summary>
        public Dictionary<string, ResourceDefinitionForClient> Resources { get; set; }

        /// <summary>
        /// Mapping from agent definition Id to agent definition
        /// </summary>
        public Dictionary<string, AgentDefinitionForClient> Agents { get; set; }
        
        /// <summary>
        /// Mapping from lookup definition Id to lookup definition
        /// </summary>
        public Dictionary<string, LookupDefinitionForClient> Lookups { get; set; }

        /// <summary>
        /// Mapping from report definition Id to report definition
        /// </summary>
        public Dictionary<string, ReportDefinitionForClient> Reports { get; set; }
    }

    public abstract class DefinitionForClient
    {
        public string MainMenuSection { get; set; }
        public string MainMenuIcon { get; set; }
        public decimal MainMenuSortKey { get; set; }
    }

    public abstract class MasterDetailDefinitionForClient : DefinitionForClient
    {
        public string TitleSingular { get; set; }
        public string TitleSingular2 { get; set; }
        public string TitleSingular3 { get; set; }
        public string TitlePlural { get; set; }
        public string TitlePlural2 { get; set; }
        public string TitlePlural3 { get; set; }
    }

    public class ReportDefinitionForClient : DefinitionForClient
    {
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public string Description { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public string Type { get; set; } // "Summary" or "Details"
        public string Chart { get; set; } // 'Card' | 'BarsVertical' | 'BarsHorizontal' | 'Line' | 'Pie'
        public bool DefaultsToChart { get; set; }
        public string Collection { get; set; }
        public string DefinitionId { get; set; }
        public List<ReportParameterDefinitionForClient> Parameters { get; set; }
        public string Filter { get; set; } // On drill down for summary
        public string OrderBy { get; set; } // On drill down for summary
        public List<ReportSelectDefinitionForClient> Select { get; set; }
        public List<ReportDimensionDefinitionForClient> Rows { get; set; }
        public List<ReportDimensionDefinitionForClient> Columns { get; set; }
        public List<ReportMeasureDefinitionForClient> Measures { get; set; }
        public int Top { get; set; }
        public bool ShowColumnsTotal { get; set; }
        public bool ShowRowsTotal { get; set; }
    }

    public class ReportParameterDefinitionForClient
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public bool IsRequired { get; set; }
    }
    
    public class ReportSelectDefinitionForClient
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
    }

    public class ReportDimensionDefinitionForClient
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string OrderDirection { get; set; }
        public bool AutoExpand { get; set; }
    }

    public class ReportMeasureDefinitionForClient
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string OrderDirection { get; set; }
        public string Aggregation { get; set; }
    }

    public class DocumentDefinitionForClient : MasterDetailDefinitionForClient
    {
        // TODO
        public bool IsSourceDocument { get; internal set; }
        public string FinalState { get; internal set; }
    }

    public class LineTypeForClient // related entity for document definition
    {
        // TODO
    }

    public class ResourceDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string OperatingSegmentLabel { get; set; }
        public string OperatingSegmentLabel2 { get; set; }
        public string OperatingSegmentLabel3 { get; set; }
        public string OperatingSegmentVisibility { get; set; }
        public int? OperatingSegmentDefaultValue { get; set; }

        public string IdentifierLabel { get; set; }
        public string IdentifierLabel2 { get; set; }
        public string IdentifierLabel3 { get; set; }
        public string IdentifierVisibility { get; set; }
        public string IdentifierDefaultValue { get; set; }

        public string CurrencyLabel { get; set; }
        public string CurrencyLabel2 { get; set; }
        public string CurrencyLabel3 { get; set; }
        public string CurrencyVisibility { get; set; }
        public string CurrencyDefaultValue { get; set; }

        public string MonetaryValueLabel { get; set; }
        public string MonetaryValueLabel2 { get; set; }
        public string MonetaryValueLabel3 { get; set; }
        public string MonetaryValueVisibility { get; set; }
        public int? MonetaryValueDefaultValue { get; set; }

        public string CountUnitLabel { get; set; }
        public string CountUnitLabel2 { get; set; }
        public string CountUnitLabel3 { get; set; }
        public string CountUnitVisibility { get; set; }
        public int? CountUnitDefaultValue { get; set; }

        public string CountLabel { get; set; }
        public string CountLabel2 { get; set; }
        public string CountLabel3 { get; set; }
        public string CountVisibility { get; set; }
        public int? CountDefaultValue { get; set; }

        public string MassUnitLabel { get; set; }
        public string MassUnitLabel2 { get; set; }
        public string MassUnitLabel3 { get; set; }
        public string MassUnitVisibility { get; set; }
        public int? MassUnitDefaultValue { get; set; }

        public string MassLabel { get; set; }
        public string MassLabel2 { get; set; }
        public string MassLabel3 { get; set; }
        public string MassVisibility { get; set; }
        public int? MassDefaultValue { get; set; }

        public string VolumeUnitLabel { get; set; }
        public string VolumeUnitLabel2 { get; set; }
        public string VolumeUnitLabel3 { get; set; }
        public string VolumeUnitVisibility { get; set; }
        public int? VolumeUnitDefaultValue { get; set; }

        public string VolumeLabel { get; set; }
        public string VolumeLabel2 { get; set; }
        public string VolumeLabel3 { get; set; }
        public string VolumeVisibility { get; set; }
        public int? VolumeDefaultValue { get; set; }

        public string TimeUnitLabel { get; set; }
        public string TimeUnitLabel2 { get; set; }
        public string TimeUnitLabel3 { get; set; }
        public string TimeUnitVisibility { get; set; }
        public int? TimeUnitDefaultValue { get; set; }

        public string TimeLabel { get; set; }
        public string TimeLabel2 { get; set; }
        public string TimeLabel3 { get; set; }
        public string TimeVisibility { get; set; }
        public int? TimeDefaultValue { get; set; }

        public string DescriptionVisibility { get; set; }

        public string AvailableSinceLabel { get; set; }
        public string AvailableSinceLabel2 { get; set; }
        public string AvailableSinceLabel3 { get; set; }
        public string AvailableSinceVisibility { get; set; }
        public DateTime? AvailableSinceDefaultValue { get; set; }

        public string AvailableTillLabel { get; set; }
        public string AvailableTillLabel2 { get; set; }
        public string AvailableTillLabel3 { get; set; }
        public string AvailableTillVisibility { get; set; }
        public DateTime? AvailableTillDefaultValue { get; set; }

        // Lookup 1
        public string Lookup1Label { get; set; }
        public string Lookup1Label2 { get; set; }
        public string Lookup1Label3 { get; set; }
        public string Lookup1Visibility { get; set; }
        public int? Lookup1DefaultValue { get; set; }
        public string Lookup1DefinitionId { get; set; }

        // Lookup 2
        public string Lookup2Label { get; set; }
        public string Lookup2Label2 { get; set; }
        public string Lookup2Label3 { get; set; }
        public string Lookup2Visibility { get; set; }
        public int? Lookup2DefaultValue { get; set; }
        public string Lookup2DefinitionId { get; set; }

        //// Lookup 3
        //public string Lookup3Label { get; set; }
        //public string Lookup3Label2 { get; set; }
        //public string Lookup3Label3 { get; set; }
        //public string Lookup3Visibility { get; set; }
        //public int? Lookup3DefaultValue { get; set; }
        //public string Lookup3DefinitionId { get; set; }

        //// Lookup 4
        //public string Lookup4Label { get; set; }
        //public string Lookup4Label2 { get; set; }
        //public string Lookup4Label3 { get; set; }
        //public string Lookup4Visibility { get; set; }
        //public int? Lookup4DefaultValue { get; set; }
        //public string Lookup4DefinitionId { get; set; }

        //// Lookup 5
        //public string Lookup5Label { get; set; }
        //public string Lookup5Label2 { get; set; }
        //public string Lookup5Label3 { get; set; }
        //public string Lookup5Visibility { get; set; }
        //public int? Lookup5DefaultValue { get; set; }
        //public string Lookup5DefinitionId { get; set; }
    }

    public class AgentDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string OperatingSegmentLabel { get; set; }
        public string OperatingSegmentLabel2 { get; set; }
        public string OperatingSegmentLabel3 { get; set; }
        public string OperatingSegmentVisibility { get; set; }
        public int? OperatingSegmentDefaultValue { get; set; }

        public string TaxIdentificationNumberVisibility { get; set; }
        public string StartDateVisibility { get; set; }
        public string StartDateLabel { get; set; }
        public string StartDateLabel2 { get; set; }
        public string StartDateLabel3 { get; set; }
        public string JobVisibility { get; set; }
        public string BasicSalaryVisibility { get; set; }
        public string TransportationAllowanceVisibility { get; set; }   
        public string OvertimeRateVisibility { get; set; }
        public string BankAccountNumberVisibility { get; set; }
    }

    public class LookupDefinitionForClient : MasterDetailDefinitionForClient
    {

    }

    public static class VisibilityOld
    {
        public const byte Hidden = 0;
        public const byte Visible = 1;
        public const byte Required = 2;
    }

    public static class Visibility
    {
        public const string Optional = "Optional";
        public const string Required = "Required";
    }

    public static class ReportType
    {
        public const string Summary = nameof(Summary);
        public const string Details = nameof(Details);
    }

    public static class ReportOrderDirection
    {
        public const string Asc = "asc";
        public const string Desc = "desc";
    }
}
