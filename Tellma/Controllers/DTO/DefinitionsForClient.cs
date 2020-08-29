using System;
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
        public Dictionary<int, DocumentDefinitionForClient> Documents { get; set; }

        /// <summary>
        /// Mapping from line type Id to line type
        /// </summary>
        public Dictionary<int, LineDefinitionForClient> Lines { get; set; }

        /// <summary>
        /// Mapping from resource definition Id to resource definition
        /// </summary>
        public Dictionary<int, ResourceDefinitionForClient> Resources { get; set; }

        /// <summary>
        /// Mapping from relation definition Id to relation definition
        /// </summary>
        public Dictionary<int, RelationDefinitionForClient> Relations { get; set; }

        /// <summary>
        /// Mapping from relation definition Id to relation definition
        /// </summary>
        public Dictionary<int, CustodyDefinitionForClient> Custodies { get; set; }

        /// <summary>
        /// Mapping from lookup definition Id to lookup definition
        /// </summary>
        public Dictionary<int, LookupDefinitionForClient> Lookups { get; set; }

        /// <summary>
        /// Mapping from report definition Id to report definition
        /// </summary>
        public Dictionary<int, ReportDefinitionForClient> Reports { get; set; }

        /// <summary>
        /// The Id of the built-int manual journal vouchers document definition
        /// </summary>
        public int ManualJournalVouchersDefinitionId { get; set; }

        /// <summary>
        /// The Id of the built-int manual line line definition
        /// </summary>
        public int ManualLinesDefinitionId { get; set; }
    }

    ///////////////////// Base Classes

    public abstract class DefinitionForClient
    {
        public string Code { get; set; }
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
        public string State { get; set; }
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
        public byte DocumentType { get; set; }
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

        // Posting Date
        public bool PostingDateVisibility { get; set; }
        public short? PostingDateRequiredState { get; set; }
        public short? PostingDateReadOnlyState { get; set; }
        public string PostingDateLabel { get; set; }
        public string PostingDateLabel2 { get; set; }
        public string PostingDateLabel3 { get; set; }

        // Debit Resource
        public bool DebitResourceVisibility { get; set; }
        public short? DebitResourceRequiredState { get; set; }
        public short? DebitResourceReadOnlyState { get; set; }
        public List<int> DebitResourceDefinitionIds { get; set; }
        public string DebitResourceLabel { get; set; }
        public string DebitResourceLabel2 { get; set; }
        public string DebitResourceLabel3 { get; set; }

        // Credit Resource
        public bool CreditResourceVisibility { get; set; }
        public short? CreditResourceRequiredState { get; set; }
        public short? CreditResourceReadOnlyState { get; set; }
        public List<int> CreditResourceDefinitionIds { get; set; }
        public string CreditResourceLabel { get; set; }
        public string CreditResourceLabel2 { get; set; }
        public string CreditResourceLabel3 { get; set; }

        // Debit Custody
        public bool DebitCustodyVisibility { get; set; }
        public short? DebitCustodyRequiredState { get; set; }
        public short? DebitCustodyReadOnlyState { get; set; }
        public List<int> DebitCustodyDefinitionIds { get; set; }
        public string DebitCustodyLabel { get; set; }
        public string DebitCustodyLabel2 { get; set; }
        public string DebitCustodyLabel3 { get; set; }

        // Credit Custody
        public bool CreditCustodyVisibility { get; set; }
        public short? CreditCustodyRequiredState { get; set; }
        public short? CreditCustodyReadOnlyState { get; set; }
        public List<int> CreditCustodyDefinitionIds { get; set; }
        public string CreditCustodyLabel { get; set; }
        public string CreditCustodyLabel2 { get; set; }
        public string CreditCustodyLabel3 { get; set; }

        // Noted Relation
        public bool NotedRelationVisibility { get; set; }
        public short? NotedRelationRequiredState { get; set; }
        public short? NotedRelationReadOnlyState { get; set; }
        public List<int> NotedRelationDefinitionIds { get; set; }
        public string NotedRelationLabel { get; set; }
        public string NotedRelationLabel2 { get; set; }
        public string NotedRelationLabel3 { get; set; }

        // Center
        public bool CenterVisibility { get; set; }
        public short? CenterRequiredState { get; set; }
        public short? CenterReadOnlyState { get; set; }
        public string CenterLabel { get; set; }
        public string CenterLabel2 { get; set; }
        public string CenterLabel3 { get; set; }

        // Clearance
        public string ClearanceVisibility { get; set; }

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
        public int LineDefinitionId { get; set; }
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
        public string Code { get; set; }
        public string TitleSingular { get; set; }
        public string TitleSingular2 { get; set; }
        public string TitleSingular3 { get; set; }
        public string TitlePlural { get; set; }
        public string TitlePlural2 { get; set; }
        public string TitlePlural3 { get; set; }
        public bool AllowSelectiveSigning { get; set; }
        public bool ViewDefaultsToForm { get; set; }
        public bool GenerateScript { get; set; }
        public string GenerateLabel { get; set; }
        public string GenerateLabel2 { get; set; }
        public string GenerateLabel3 { get; set; }
        public List<LineDefinitionEntryForClient> Entries { get; set; }
        public List<LineDefinitionColumnForClient> Columns { get; set; }
        public List<LineDefinitionStateReasonForClient> StateReasons { get; set; }
        public List<LineDefinitionGenerateParameterForClient> GenerateParameters { get; set; }
    }

    public class LineDefinitionEntryForClient
    {
        public short Direction { get; set; } // Is it needed??
        public int? ParentAccountTypeId { get; set; }
        public int? EntryTypeId { get; set; }

        // Computed from AccountTypeParent
        public int? EntryTypeParentId { get; set; }
        public List<int> CustodianDefinitionIds { get; set; }
        public List<int> CustodyDefinitionIds { get; set; }
        public List<int> ParticipantDefinitionIds { get; set; }
        public List<int> ResourceDefinitionIds { get; set; }
        public List<int> NotedRelationDefinitionIds { get; set; }
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

    public class LineDefinitionGenerateParameterForClient
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Visibility { get; set; }
        public string DataType { get; set; }
        public string Filter { get; set; }
    }

    ///////////////////// Other Definitions

    public class ResourceDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string CurrencyVisibility { get; set; }
        public string CenterVisibility { get; set; }
        public string ImageVisibility { get; set; }
        public string DescriptionVisibility { get; set; }
        public string LocationVisibility { get; set; }

        public string FromDateLabel { get; set; }
        public string FromDateLabel2 { get; set; }
        public string FromDateLabel3 { get; set; }
        public string FromDateVisibility { get; set; }

        public string ToDateLabel { get; set; }
        public string ToDateLabel2 { get; set; }
        public string ToDateLabel3 { get; set; }
        public string ToDateVisibility { get; set; }

        // Decimal 1
        public string Decimal1Label { get; set; }
        public string Decimal1Label2 { get; set; }
        public string Decimal1Label3 { get; set; }
        public string Decimal1Visibility { get; set; }

        // Decimal 2
        public string Decimal2Label { get; set; }
        public string Decimal2Label2 { get; set; }
        public string Decimal2Label3 { get; set; }
        public string Decimal2Visibility { get; set; }

        // Int 1
        public string Int1Label { get; set; }
        public string Int1Label2 { get; set; }
        public string Int1Label3 { get; set; }
        public string Int1Visibility { get; set; }

        // Int 2
        public string Int2Label { get; set; }
        public string Int2Label2 { get; set; }
        public string Int2Label3 { get; set; }
        public string Int2Visibility { get; set; }

        // Lookup 1
        public string Lookup1Label { get; set; }
        public string Lookup1Label2 { get; set; }
        public string Lookup1Label3 { get; set; }
        public string Lookup1Visibility { get; set; }
        public int? Lookup1DefinitionId { get; set; }

        // Lookup 2
        public string Lookup2Label { get; set; }
        public string Lookup2Label2 { get; set; }
        public string Lookup2Label3 { get; set; }
        public string Lookup2Visibility { get; set; }
        public int? Lookup2DefinitionId { get; set; }

        // Lookup 3
        public string Lookup3Label { get; set; }
        public string Lookup3Label2 { get; set; }
        public string Lookup3Label3 { get; set; }
        public string Lookup3Visibility { get; set; }
        public int? Lookup3DefinitionId { get; set; }

        // Lookup 4
        public string Lookup4Label { get; set; }
        public string Lookup4Label2 { get; set; }
        public string Lookup4Label3 { get; set; }
        public string Lookup4Visibility { get; set; }
        public int? Lookup4DefinitionId { get; set; }

        //// Lookup 5
        //public string Lookup5Label { get; set; }
        //public string Lookup5Label2 { get; set; }
        //public string Lookup5Label3 { get; set; }
        //public string Lookup5Visibility { get; set; }
        //public int? Lookup5DefaultValue { get; set; }
        //public int? Lookup5DefinitionId { get; set; }

        // Text 1
        public string Text1Label { get; set; }
        public string Text1Label2 { get; set; }
        public string Text1Label3 { get; set; }
        public string Text1Visibility { get; set; }

        // Text 2
        public string Text2Label { get; set; }
        public string Text2Label2 { get; set; }
        public string Text2Label3 { get; set; }
        public string Text2Visibility { get; set; }

        // Resource Only

        public string IdentifierLabel { get; set; }
        public string IdentifierLabel2 { get; set; }
        public string IdentifierLabel3 { get; set; }
        public string IdentifierVisibility { get; set; }

        public string VatRateVisibility { get; set; }
        public decimal? DefaultVatRate { get; set; }

        public string ReorderLevelVisibility { get; set; }
        public string EconomicOrderQuantityVisibility { get; set; }
        public string UnitCardinality { get; set; }
        public int? DefaultUnitId { get; set; }
        public string UnitMassVisibility { get; set; }
        public int? DefaultUnitMassUnitId { get; set; }
        public string MonetaryValueVisibility { get; set; }
        public string ParticipantVisibility { get; set; }
        public int? ParticipantDefinitionId { get; set; }
    }

    public class RelationDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string CurrencyVisibility { get; set; }
        public string CenterVisibility { get; set; }
        public string ImageVisibility { get; set; }
        public string DescriptionVisibility { get; set; }
        public string LocationVisibility { get; set; }

        public string FromDateLabel { get; set; }
        public string FromDateLabel2 { get; set; }
        public string FromDateLabel3 { get; set; }
        public string FromDateVisibility { get; set; }

        public string ToDateLabel { get; set; }
        public string ToDateLabel2 { get; set; }
        public string ToDateLabel3 { get; set; }
        public string ToDateVisibility { get; set; }

        // Decimal 1
        public string Decimal1Label { get; set; }
        public string Decimal1Label2 { get; set; }
        public string Decimal1Label3 { get; set; }
        public string Decimal1Visibility { get; set; }

        // Decimal 2
        public string Decimal2Label { get; set; }
        public string Decimal2Label2 { get; set; }
        public string Decimal2Label3 { get; set; }
        public string Decimal2Visibility { get; set; }

        // Int 1
        public string Int1Label { get; set; }
        public string Int1Label2 { get; set; }
        public string Int1Label3 { get; set; }
        public string Int1Visibility { get; set; }

        // Int 2
        public string Int2Label { get; set; }
        public string Int2Label2 { get; set; }
        public string Int2Label3 { get; set; }
        public string Int2Visibility { get; set; }

        // Lookup 1
        public string Lookup1Label { get; set; }
        public string Lookup1Label2 { get; set; }
        public string Lookup1Label3 { get; set; }
        public string Lookup1Visibility { get; set; }
        public int? Lookup1DefinitionId { get; set; }

        // Lookup 2
        public string Lookup2Label { get; set; }
        public string Lookup2Label2 { get; set; }
        public string Lookup2Label3 { get; set; }
        public string Lookup2Visibility { get; set; }
        public int? Lookup2DefinitionId { get; set; }

        // Lookup 3
        public string Lookup3Label { get; set; }
        public string Lookup3Label2 { get; set; }
        public string Lookup3Label3 { get; set; }
        public string Lookup3Visibility { get; set; }
        public int? Lookup3DefinitionId { get; set; }

        // Lookup 4
        public string Lookup4Label { get; set; }
        public string Lookup4Label2 { get; set; }
        public string Lookup4Label3 { get; set; }
        public string Lookup4Visibility { get; set; }
        public int? Lookup4DefinitionId { get; set; }

        //// Lookup 5
        //public string Lookup5Label { get; set; }
        //public string Lookup5Label2 { get; set; }
        //public string Lookup5Label3 { get; set; }
        //public string Lookup5Visibility { get; set; }
        //public int? Lookup5DefaultValue { get; set; }
        //public int? Lookup5DefinitionId { get; set; }

        // Text 1
        public string Text1Label { get; set; }
        public string Text1Label2 { get; set; }
        public string Text1Label3 { get; set; }
        public string Text1Visibility { get; set; }

        // Text 2
        public string Text2Label { get; set; }
        public string Text2Label2 { get; set; }
        public string Text2Label3 { get; set; }
        public string Text2Visibility { get; set; }



        public string AgentVisibility { get; set; }
        public string TaxIdentificationNumberVisibility { get; set; }
        public string JobVisibility { get; set; }
        public string BankAccountNumberVisibility { get; set; }
        public string UserCardinality { get; set; }
    }


    public class CustodyDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string CurrencyVisibility { get; set; }
        public string CenterVisibility { get; set; }
        public string ImageVisibility { get; set; }
        public string DescriptionVisibility { get; set; }
        public string LocationVisibility { get; set; }

        public string FromDateLabel { get; set; }
        public string FromDateLabel2 { get; set; }
        public string FromDateLabel3 { get; set; }
        public string FromDateVisibility { get; set; }

        public string ToDateLabel { get; set; }
        public string ToDateLabel2 { get; set; }
        public string ToDateLabel3 { get; set; }
        public string ToDateVisibility { get; set; }

        // Decimal 1
        public string Decimal1Label { get; set; }
        public string Decimal1Label2 { get; set; }
        public string Decimal1Label3 { get; set; }
        public string Decimal1Visibility { get; set; }

        // Decimal 2
        public string Decimal2Label { get; set; }
        public string Decimal2Label2 { get; set; }
        public string Decimal2Label3 { get; set; }
        public string Decimal2Visibility { get; set; }

        // Int 1
        public string Int1Label { get; set; }
        public string Int1Label2 { get; set; }
        public string Int1Label3 { get; set; }
        public string Int1Visibility { get; set; }

        // Int 2
        public string Int2Label { get; set; }
        public string Int2Label2 { get; set; }
        public string Int2Label3 { get; set; }
        public string Int2Visibility { get; set; }

        // Lookup 1
        public string Lookup1Label { get; set; }
        public string Lookup1Label2 { get; set; }
        public string Lookup1Label3 { get; set; }
        public string Lookup1Visibility { get; set; }
        public int? Lookup1DefinitionId { get; set; }

        // Lookup 2
        public string Lookup2Label { get; set; }
        public string Lookup2Label2 { get; set; }
        public string Lookup2Label3 { get; set; }
        public string Lookup2Visibility { get; set; }
        public int? Lookup2DefinitionId { get; set; }

        // Lookup 3
        public string Lookup3Label { get; set; }
        public string Lookup3Label2 { get; set; }
        public string Lookup3Label3 { get; set; }
        public string Lookup3Visibility { get; set; }
        public int? Lookup3DefinitionId { get; set; }

        // Lookup 4
        public string Lookup4Label { get; set; }
        public string Lookup4Label2 { get; set; }
        public string Lookup4Label3 { get; set; }
        public string Lookup4Visibility { get; set; }
        public int? Lookup4DefinitionId { get; set; }

        //// Lookup 5
        //public string Lookup5Label { get; set; }
        //public string Lookup5Label2 { get; set; }
        //public string Lookup5Label3 { get; set; }
        //public string Lookup5Visibility { get; set; }
        //public int? Lookup5DefaultValue { get; set; }
        //public int? Lookup5DefinitionId { get; set; }

        // Text 1
        public string Text1Label { get; set; }
        public string Text1Label2 { get; set; }
        public string Text1Label3 { get; set; }
        public string Text1Visibility { get; set; }

        // Text 2
        public string Text2Label { get; set; }
        public string Text2Label2 { get; set; }
        public string Text2Label3 { get; set; }
        public string Text2Visibility { get; set; }

        public string CustodianVisibility { get; set; }
        public int? CustodianDefinitionId { get; set; }

        public string ExternalReferenceLabel { get; set; }
        public string ExternalReferenceLabel2 { get; set; }
        public string ExternalReferenceLabel3 { get; set; }
        public string ExternalReferenceVisibility { get; set; }
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

    public static class Cardinality
    {
        public const string Single = nameof(Single);
        public const string Multiple = nameof(Multiple);
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
