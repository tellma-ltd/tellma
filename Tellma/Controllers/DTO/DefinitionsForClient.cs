﻿using System;
using System.Collections.Generic;

namespace Tellma.Controllers.Dto
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
        public Dictionary<string, LineDefinitionForClient> Lines { get; set; }

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

    ///////////////////// Base Classes
    
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

    ///////////////////// Report Definitions

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
        public bool ShowInMainMenu { get; set; }
    }

    public class ReportParameterDefinitionForClient
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Visibility { get; set; }
        public string Value { get; set; }
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
        public string Modifier { get; set; }
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

    ///////////////////// Document Definitions

    public class DocumentDefinitionForClient : MasterDetailDefinitionForClient
    {
        public bool IsOriginalDocument { get; set; }
        public string Prefix { get; set; }
        public byte CodeWidth { get; set; }

        // Memo
        public string MemoVisibility { get; set; }
        public bool MemoIsCommonVisibility { get; set; }
        public string MemoLabel { get; set; }
        public string MemoLabel2 { get; set; }
        public string MemoLabel3 { get; set; }
        public short? MemoRequiredState { get; set; }
        public short? MemoReadOnlyState { get; set; }

        // Debit Agent
        public bool DebitAgentVisibility { get; set; }
        public short? DebitAgentRequiredState { get; set; }
        public short? DebitAgentReadOnlyState { get; set; }
        public string DebitAgentDefinitionId { get; set; }
        public string DebitAgentLabel { get; set; }
        public string DebitAgentLabel2 { get; set; }
        public string DebitAgentLabel3 { get; set; }

        // Credit Agent
        public bool CreditAgentVisibility { get; set; }
        public short? CreditAgentRequiredState { get; set; }
        public short? CreditAgentReadOnlyState { get; set; }
        public string CreditAgentDefinitionId { get; set; }
        public string CreditAgentLabel { get; set; }
        public string CreditAgentLabel2 { get; set; }
        public string CreditAgentLabel3 { get; set; }

        // Noted Agent
        public bool NotedAgentVisibility { get; set; }
        public short? NotedAgentRequiredState { get; set; }
        public short? NotedAgentReadOnlyState { get; set; }
        public string NotedAgentDefinitionId { get; set; }
        public string NotedAgentLabel { get; set; }
        public string NotedAgentLabel2 { get; set; }
        public string NotedAgentLabel3 { get; set; }

        // Clearance
        public string ClearanceVisibility { get; set; }

        // Investment Center
        public bool InvestmentCenterVisibility { get; set; }
        public short? InvestmentCenterRequiredState { get; set; }
        public short? InvestmentCenterReadOnlyState { get; set; }
        public string InvestmentCenterLabel { get; set; }
        public string InvestmentCenterLabel2 { get; set; }
        public string InvestmentCenterLabel3 { get; set; }

        // Time 1
        public bool Time1Visibility { get; set; }
        public short? Time1RequiredState { get; set; }
        public short? Time1ReadOnlyState { get; set; }
        public string Time1Label { get; set; }
        public string Time1Label2 { get; set; }
        public string Time1Label3 { get; set; }

        // Time 2
        public bool Time2Visibility { get; set; }
        public short? Time2RequiredState { get; set; }
        public short? Time2ReadOnlyState { get; set; }
        public string Time2Label { get; set; }
        public string Time2Label2 { get; set; }
        public string Time2Label3 { get; set; }

        // Quantity
        public bool QuantityVisibility { get; set; }
        public short? QuantityRequiredState { get; set; }
        public short? QuantityReadOnlyState { get; set; }
        public string QuantityLabel { get; set; }
        public string QuantityLabel2 { get; set; }
        public string QuantityLabel3 { get; set; }

        // Unit
        public bool UnitVisibility { get; set; }
        public short? UnitRequiredState { get; set; }
        public short? UnitReadOnlyState { get; set; }
        public string UnitLabel { get; set; }
        public string UnitLabel2 { get; set; }
        public string UnitLabel3 { get; set; }

        // Currency
        public bool CurrencyVisibility { get; set; }
        public short? CurrencyRequiredState { get; set; }
        public short? CurrencyReadOnlyState { get; set; }
        public string CurrencyLabel { get; set; }
        public string CurrencyLabel2 { get; set; }
        public string CurrencyLabel3 { get; set; }


        public bool CanReachState1 { get; set; }
        public bool CanReachState2 { get; set; }
        public bool CanReachState3 { get; set; }
        public bool HasWorkflow { get; set; }
        public List<DocumentDefinitionLineDefinitionForClient> LineDefinitions { get; set; }
        public List<DocumentDefinitionMarkupTemplateForClient> MarkupTemplates { get; set; }
    }

    public class DocumentDefinitionLineDefinitionForClient
    {
        public string LineDefinitionId { get; set; }
        public bool IsVisibleByDefault { get; set; }
    }

    public class DocumentDefinitionMarkupTemplateForClient
    {
        public int MarkupTemplateId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public bool SupportsPrimaryLanguage { get; set; }
        public bool SupportsSecondaryLanguage { get; set; }
        public bool SupportsTernaryLanguage { get; set; }
        public string Usage { get; set; }
    }

    public class LineDefinitionForClient // related entity for document definition
    {
        public string TitleSingular { get; set; }
        public string TitleSingular2 { get; set; }
        public string TitleSingular3 { get; set; }
        public string TitlePlural { get; set; }
        public string TitlePlural2 { get; set; }
        public string TitlePlural3 { get; set; }
        public bool AllowSelectiveSigning { get; set; }
        public bool ViewDefaultsToForm { get; set; }
        public List<LineDefinitionEntryForClient> Entries { get; set; }
        public List<LineDefinitionColumnForClient> Columns { get; set; }
        public List<LineDefinitionStateReasonForClient> StateReasons { get; set; }
    }

    public class LineDefinitionEntryForClient
    {
        public short Direction { get; set; } // Is it needed??
        public int? AccountTypeParentId { get; set; }
        public int? EntryTypeId { get; set; }

        // Computed from AccountTypeParent
        public bool IsResourceClassification { get; set; }
        public int? EntryTypeParentId { get; set; }
        public string ResourceDefinitionId { get; set; }
        public string AgentDefinitionId { get; set; }
        public string NotedAgentDefinitionId { get; set; }
        //public string DueDateLabel { get; set; }
        //public string DueDateLabel2 { get; set; }
        //public string DueDateLabel3 { get; set; }
        //public string Time1Label { get; set; }
        //public string Time1Label2 { get; set; }
        //public string Time1Label3 { get; set; }
        //public string Time2Label { get; set; }
        //public string Time2Label2 { get; set; }
        //public string Time2Label3 { get; set; }
        //public string ExternalReferenceLabel { get; set; }
        //public string ExternalReferenceLabel2 { get; set; }
        //public string ExternalReferenceLabel3 { get; set; }
        //public string AdditionalReferenceLabel { get; set; }
        //public string AdditionalReferenceLabel2 { get; set; }
        //public string AdditionalReferenceLabel3 { get; set; }
        //public string NotedAgentNameLabel { get; set; }
        //public string NotedAgentNameLabel2 { get; set; }
        //public string NotedAgentNameLabel3 { get; set; }
        //public string NotedAmountLabel { get; set; }
        //public string NotedAmountLabel2 { get; set; }
        //public string NotedAmountLabel3 { get; set; }
        //public string NotedDateLabel { get; set; }
        //public string NotedDateLabel2 { get; set; }
        //public string NotedDateLabel3 { get; set; }
    }

    public class LineDefinitionColumnForClient
    {
        public string ColumnName { get; set; }
        public int EntryIndex { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public short? RequiredState { get; set; }
        public short? ReadOnlyState { get; set; }
        public bool? InheritsFromHeader { get; set; }
    }

    public class LineDefinitionStateReasonForClient
    {
        public int Id { get; set; }
        public short? State { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public bool IsActive { get; set; }
    }

    ///////////////////// Other Definitions
    
    public class ResourceDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string IdentifierLabel { get; set; } // Yes
        public string IdentifierLabel2 { get; set; } // Yes
        public string IdentifierLabel3 { get; set; } // Yes
        public string IdentifierVisibility { get; set; } // Yes
        public string IdentifierDefaultValue { get; set; }

        public string CurrencyLabel { get; set; }
        public string CurrencyLabel2 { get; set; }
        public string CurrencyLabel3 { get; set; }
        public string CurrencyVisibility { get; set; } // Yes
        public string CurrencyDefaultValue { get; set; }

        public string MonetaryValueLabel { get; set; }
        public string MonetaryValueLabel2 { get; set; }
        public string MonetaryValueLabel3 { get; set; }
        public string MonetaryValueVisibility { get; set; }
        public int? MonetaryValueDefaultValue { get; set; }

        public string AssetTypeVisibility { get; set; }
        public string RevenueTypeVisibility { get; set; }
        public string ExpenseTypeVisibility { get; set; }
        public string DescriptionVisibility { get; set; } // Yes
        public string ExpenseEntryTypeVisibility { get; set; } // Yes
        public string CenterVisibility { get; set; } // Yes
        public string ResidualMonetaryValueVisibility { get; set; } // Yes
        public string ResidualValueVisibility { get; set; } // Yes


        public string ReorderLevelVisibility { get; set; } // Yes
        public decimal? ReorderLevelDefaultValue { get; set; }

        public string EconomicOrderQuantityVisibility { get; set; } // Yes
        public decimal? EconomicOrderQuantityDefaultValue { get; set; }


        public string AvailableSinceLabel { get; set; } // Yes
        public string AvailableSinceLabel2 { get; set; } // Yes
        public string AvailableSinceLabel3 { get; set; } // Yes
        public string AvailableSinceVisibility { get; set; } // Yes
        public DateTime? AvailableSinceDefaultValue { get; set; }

        public string AvailableTillLabel { get; set; } // Yes
        public string AvailableTillLabel2 { get; set; } // Yes
        public string AvailableTillLabel3 { get; set; } // Yes
        public string AvailableTillVisibility { get; set; } // Yes
        public DateTime? AvailableTillDefaultValue { get; set; }

        // Decimal 1
        public string Decimal1Label { get; set; } // Yes
        public string Decimal1Label2 { get; set; } // Yes
        public string Decimal1Label3 { get; set; } // Yes
        public string Decimal1Visibility { get; set; } // Yes
        public decimal? Decimal1DefaultValue { get; set; }

        // Decimal 2
        public string Decimal2Label { get; set; } // Yes
        public string Decimal2Label2 { get; set; } // Yes
        public string Decimal2Label3 { get; set; } // Yes
        public string Decimal2Visibility { get; set; } // Yes
        public decimal? Decimal2DefaultValue { get; set; }

        // Int 1
        public string Int1Label { get; set; } // Yes
        public string Int1Label2 { get; set; } // Yes
        public string Int1Label3 { get; set; } // Yes
        public string Int1Visibility { get; set; } // Yes
        public int? Int1DefaultValue { get; set; }

        // Int 2
        public string Int2Label { get; set; } // Yes
        public string Int2Label2 { get; set; } // Yes
        public string Int2Label3 { get; set; } // Yes
        public string Int2Visibility { get; set; } // Yes
        public int? Int2DefaultValue { get; set; }

        // Lookup 1
        public string Lookup1Label { get; set; } // Yes
        public string Lookup1Label2 { get; set; } // Yes
        public string Lookup1Label3 { get; set; } // Yes
        public string Lookup1Visibility { get; set; } // Yes
        public int? Lookup1DefaultValue { get; set; }
        public string Lookup1DefinitionId { get; set; } // Yes

        // Lookup 2
        public string Lookup2Label { get; set; } // Yes
        public string Lookup2Label2 { get; set; } // Yes
        public string Lookup2Label3 { get; set; } // Yes
        public string Lookup2Visibility { get; set; } // Yes
        public int? Lookup2DefaultValue { get; set; }
        public string Lookup2DefinitionId { get; set; } // Yes

        // Lookup 3
        public string Lookup3Label { get; set; } // Yes
        public string Lookup3Label2 { get; set; } // Yes
        public string Lookup3Label3 { get; set; } // Yes
        public string Lookup3Visibility { get; set; } // Yes
        public int? Lookup3DefaultValue { get; set; }
        public string Lookup3DefinitionId { get; set; } // Yes

        // Lookup 4
        public string Lookup4Label { get; set; } // Yes
        public string Lookup4Label2 { get; set; } // Yes
        public string Lookup4Label3 { get; set; } // Yes
        public string Lookup4Visibility { get; set; } // Yes
        public int? Lookup4DefaultValue { get; set; }
        public string Lookup4DefinitionId { get; set; } // Yes

        //// Lookup 5
        //public string Lookup5Label { get; set; }
        //public string Lookup5Label2 { get; set; }
        //public string Lookup5Label3 { get; set; }
        //public string Lookup5Visibility { get; set; }
        //public int? Lookup5DefaultValue { get; set; }
        //public string Lookup5DefinitionId { get; set; }
        
        // Text 1
        public string Text1Label { get; set; } // Yes
        public string Text1Label2 { get; set; } // Yes
        public string Text1Label3 { get; set; } // Yes
        public string Text1Visibility { get; set; } // Yes
        public string Text1DefaultValue { get; set; }

        // Text 2
        public string Text2Label { get; set; } // Yes
        public string Text2Label2 { get; set; } // Yes
        public string Text2Label3 { get; set; } // Yes
        public string Text2Visibility { get; set; } // Yes
        public string Text2DefaultValue { get; set; }
    }

    public class AgentDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string TaxIdentificationNumberVisibility { get; set; }
        public string ImageVisibility { get; set; }
        public string StartDateVisibility { get; set; }
        public string StartDateLabel { get; set; }
        public string StartDateLabel2 { get; set; }
        public string StartDateLabel3 { get; set; }
        public string JobVisibility { get; set; }
        public string RatesVisibility { get; set; }
        public string RatesLabel { get; set; }
        public string RatesLabel2 { get; set; }
        public string RatesLabel3 { get; set; }
        public string BankAccountNumberVisibility { get; set; }
    }

    public class LookupDefinitionForClient : MasterDetailDefinitionForClient
    {

    }

    ///////////////////// Supporting Classes

    public static class Visibility
    {
        public const string Optional = nameof(Optional);
        public const string Required = nameof(Required);
        public const string None = nameof(None);
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
