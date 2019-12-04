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
        /// Mapping from resource definition Id to resource definition
        /// </summary>
        public Dictionary<string, AccountDefinitionForClient> Accounts { get; set; }

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

    public class AccountDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string ResponsibilityCenter_Label { get; set; }
        public string ResponsibilityCenter_Label2 { get; set; }
        public string ResponsibilityCenter_Label3 { get; set; }
        public string ResponsibilityCenter_Visibility { get; set; }
        public int? ResponsibilityCenter_DefaultValue { get; set; }

        public string Custodian_Label { get; set; }
        public string Custodian_Label2 { get; set; }
        public string Custodian_Label3 { get; set; }
        public string Custodian_Visibility { get; set; }
        public int? Custodian_DefaultValue { get; set; }

        public string Resource_Label { get; set; }
        public string Resource_Label2 { get; set; }
        public string Resource_Label3 { get; set; }
        public string Resource_Visibility { get; set; }
        public int? Resource_DefaultValue { get; set; }
        public string Resource_DefinitionList { get; set; }

        public string Location_Label { get; set; }
        public string Location_Label2 { get; set; }
        public string Location_Label3 { get; set; }
        public string Location_Visibility { get; set; }
        public int? Location_DefaultValue { get; set; }
        public string Location_DefinitionList { get; set; }

        public string PartyReference_Label { get; set; }
        public string PartyReference_Label2 { get; set; }
        public string PartyReference_Label3 { get; set; }
        public string PartyReference_Visibility { get; set; }
    }

    public class ResourceDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string MassUnit_Label { get; set; }
        public string MassUnit_Label2 { get; set; }
        public string MassUnit_Label3 { get; set; }
        public byte MassUnit_Visibility { get; set; }
        public int? MassUnit_DefaultValue { get; set; }

        public string VolumeUnit_Label { get; set; }
        public string VolumeUnit_Label2 { get; set; }
        public string VolumeUnit_Label3 { get; set; }
        public byte VolumeUnit_Visibility { get; set; }
        public int? VolumeUnit_DefaultValue { get; set; }

        public string AreaUnit_Label { get; set; }
        public string AreaUnit_Label2 { get; set; }
        public string AreaUnit_Label3 { get; set; }
        public byte AreaUnit_Visibility { get; set; }
        public int? AreaUnit_DefaultValue { get; set; }

        public string LengthUnit_Label { get; set; }
        public string LengthUnit_Label2 { get; set; }
        public string LengthUnit_Label3 { get; set; }
        public byte LengthUnit_Visibility { get; set; }
        public int? LengthUnit_DefaultValue { get; set; }

        public string TimeUnit_Label { get; set; }
        public string TimeUnit_Label2 { get; set; }
        public string TimeUnit_Label3 { get; set; }
        public byte TimeUnit_Visibility { get; set; }
        public int? TimeUnit_DefaultValue { get; set; }

        public string CountUnit_Label { get; set; }
        public string CountUnit_Label2 { get; set; }
        public string CountUnit_Label3 { get; set; }
        public byte CountUnit_Visibility { get; set; }
        public int? CountUnit_DefaultValue { get; set; }

        public string Memo_Label { get; set; }
        public string Memo_Label2 { get; set; }
        public string Memo_Label3 { get; set; }
        public byte Memo_Visibility { get; set; }
        public string Memo_DefaultValue { get; set; }

        public string CustomsReference_Label { get; set; }
        public string CustomsReference_Label2 { get; set; }
        public string CustomsReference_Label3 { get; set; }
        public byte CustomsReference_Visibility { get; set; }
        public string CustomsReference_DefaultValue { get; set; }


        // Lookup 1
        public string Lookup1_Label { get; set; }
        public string Lookup1_Label2 { get; set; }
        public string Lookup1_Label3 { get; set; }
        public byte Lookup1_Visibility { get; set; } // 0, 1, 2 (not visible, visible, visible and required)
        public int? Lookup1_DefaultValue { get; set; }
        public string Lookup1_DefinitionId { get; set; }

        // Lookup 2
        public string Lookup2_Label { get; set; }
        public string Lookup2_Label2 { get; set; }
        public string Lookup2_Label3 { get; set; }
        public byte Lookup2_Visibility { get; set; }
        public int? Lookup2_DefaultValue { get; set; }
        public string Lookup2_DefinitionId { get; set; }

        // Lookup 3
        public string Lookup3_Label { get; set; }
        public string Lookup3_Label2 { get; set; }
        public string Lookup3_Label3 { get; set; }
        public byte Lookup3_Visibility { get; set; }
        public int? Lookup3_DefaultValue { get; set; }
        public string Lookup3_DefinitionId { get; set; }

        // Lookup 4
        public string Lookup4_Label { get; set; }
        public string Lookup4_Label2 { get; set; }
        public string Lookup4_Label3 { get; set; }
        public byte Lookup4_Visibility { get; set; }
        public int? Lookup4_DefaultValue { get; set; }
        public string Lookup4_DefinitionId { get; set; }

        // Lookup 5
        public string Lookup5_Label { get; set; }
        public string Lookup5_Label2 { get; set; }
        public string Lookup5_Label3 { get; set; }
        public byte Lookup5_Visibility { get; set; }
        public int? Lookup5_DefaultValue { get; set; }
        public string Lookup5_DefinitionId { get; set; }
    }

    public class AgentDefinitionForClient : MasterDetailDefinitionForClient
    {
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

    public static class AccountVisibility
    {
        public const string None = nameof(None);
        public const string RequiredInAccounts = nameof(RequiredInAccounts);
        public const string RequiredInEntries = nameof(RequiredInEntries);
        public const string OptionalInEntries = nameof(OptionalInEntries);
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
