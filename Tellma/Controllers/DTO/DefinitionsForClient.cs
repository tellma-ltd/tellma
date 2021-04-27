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
        /// Mapping from lookup definition Id to lookup definition
        /// </summary>
        public Dictionary<int, LookupDefinitionForClient> Lookups { get; set; }

        /// <summary>
        /// Mapping from report definition Id to report definition
        /// </summary>
        public Dictionary<int, ReportDefinitionForClient> Reports { get; set; }

        /// <summary>
        /// Mapping from dashboard definition Id to dashboard definition
        /// </summary>
        public Dictionary<int, DashboardDefinitionForClient> Dashboards { get; set; }

        /// <summary>
        /// A collection of markup templates
        /// </summary>
        public IEnumerable<MarkupTemplateForClient> MarkupTemplates { get; set; }

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
        public int Id { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public string Description { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public string Type { get; set; } // "Summary" or "Details"
        public string Chart { get; set; } // 'Card' | 'BarsVertical' | 'BarsHorizontal' | 'Line' | 'Pie'
        public string ChartOptions { get; set; }
        public bool DefaultsToChart { get; set; }
        public string Collection { get; set; }
        public int? DefinitionId { get; set; }
        public List<ReportDefinitionParameterForClient> Parameters { get; set; }
        public string Filter { get; set; } // On drill down for summary
        public string Having { get; set; }
        public string OrderBy { get; set; } // On drill down for summary
        public List<ReportDefinitionSelectForClient> Select { get; set; }
        public List<ReportDefinitionDimensionForClient> Rows { get; set; }
        public List<ReportDefinitionDimensionForClient> Columns { get; set; }
        public List<ReportDefinitionMeasureForClient> Measures { get; set; }
        public int Top { get; set; }
        public bool ShowColumnsTotal { get; set; }
        public string ColumnsTotalLabel { get; set; }
        public string ColumnsTotalLabel2 { get; set; }
        public string ColumnsTotalLabel3 { get; set; }
        public bool ShowRowsTotal { get; set; }
        public string RowsTotalLabel { get; set; }
        public string RowsTotalLabel2 { get; set; }
        public string RowsTotalLabel3 { get; set; }
        public bool IsCustomDrilldown { get; set; }
        public bool ShowInMainMenu { get; set; }
    }

    public class ReportDefinitionParameterForClient
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Visibility { get; set; }
        public string DefaultExpression { get; set; }
        public string Control { get; set; }
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionSelectForClient
    {
        public string Expression { get; set; }
        public bool Localize { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Control { get; set; }
        public string ControlOptions { get; set; }
    }

    public class ReportDefinitionDimensionForClient
    {
        public string KeyExpression { get; set; }
        public string DisplayExpression { get; set; }
        public bool Localize { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string OrderDirection { get; set; }
        public int AutoExpandLevel { get; set; }
        public bool ShowAsTree { get; set; }
        public string Control { get; set; }
        public string ControlOptions { get; set; }
        public List<ReportDefinitionDimensionAttributeForClient> Attributes { get; set; }
    }

    public class ReportDefinitionDimensionAttributeForClient
    {
        public string Expression { get; set; }
        public bool Localize { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string OrderDirection { get; set; }
    }

    public class ReportDefinitionMeasureForClient
    {
        public string Expression { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string OrderDirection { get; set; }
        public string Control { get; set; }
        public string ControlOptions { get; set; }
        public string DangerWhen { get; set; }
        public string WarningWhen { get; set; }
        public string SuccessWhen { get; set; }
    }

    ///////////////////// Dashboard Definitions

    public class DashboardDefinitionForClient : DefinitionForClient
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public int AutoRefreshPeriodInMinutes { get; set; }
        public List<DashboardDefinitionWidgetForClient> Widgets { get; set; }
        public bool ShowInMainMenu { get; set; }
    }

    public class DashboardDefinitionWidgetForClient
    {
        public int ReportDefinitionId { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public int? AutoRefreshPeriodInMinutes { get; set; }
    }

    ///////////////////// Document Definitions

    public class DocumentDefinitionForClient : MasterDetailDefinitionForClient
    {
        public bool HasAttachments { get; set; }
        public bool HasBookkeeping { get; set; }
        public bool IsOriginalDocument { get; set; }
        public byte DocumentType { get; set; }
        public string Prefix { get; set; }
        public byte CodeWidth { get; set; }

        // Posting Date
        public string PostingDateVisibility { get; set; }
        public bool PostingDateIsCommonVisibility { get; set; }
        public string PostingDateLabel { get; set; }
        public string PostingDateLabel2 { get; set; }
        public string PostingDateLabel3 { get; set; }
        public short? PostingDateRequiredState { get; set; }
        public short? PostingDateReadOnlyState { get; set; }

        // Center
        public string CenterVisibility { get; set; }
        public bool CenterIsCommonVisibility { get; set; }
        public string CenterLabel { get; set; }
        public string CenterLabel2 { get; set; }
        public string CenterLabel3 { get; set; }
        public string CenterFilter { get; set; }
        public short? CenterRequiredState { get; set; }
        public short? CenterReadOnlyState { get; set; }

        // Memo
        public string MemoVisibility { get; set; }
        public bool MemoIsCommonVisibility { get; set; }
        public string MemoLabel { get; set; }
        public string MemoLabel2 { get; set; }
        public string MemoLabel3 { get; set; }
        public short? MemoRequiredState { get; set; }
        public short? MemoReadOnlyState { get; set; }

        // Currency
        public bool CurrencyVisibility { get; set; }
        public short? CurrencyRequiredState { get; set; }
        public short? CurrencyReadOnlyState { get; set; }
        public string CurrencyLabel { get; set; }
        public string CurrencyLabel2 { get; set; }
        public string CurrencyLabel3 { get; set; }
        public string CurrencyFilter { get; set; }

        // Custodian
        public bool CustodianVisibility { get; set; }
        public short? CustodianRequiredState { get; set; }
        public short? CustodianReadOnlyState { get; set; }
        public List<int> CustodianDefinitionIds { get; set; }
        public string CustodianLabel { get; set; }
        public string CustodianLabel2 { get; set; }
        public string CustodianLabel3 { get; set; }
        public string CustodianFilter { get; set; }

        // Relation
        public bool RelationVisibility { get; set; }
        public short? RelationRequiredState { get; set; }
        public short? RelationReadOnlyState { get; set; }
        public List<int> RelationDefinitionIds { get; set; }
        public string RelationLabel { get; set; }
        public string RelationLabel2 { get; set; }
        public string RelationLabel3 { get; set; }
        public string RelationFilter { get; set; }

        // Resource
        public bool ResourceVisibility { get; set; }
        public short? ResourceRequiredState { get; set; }
        public short? ResourceReadOnlyState { get; set; }
        public List<int> ResourceDefinitionIds { get; set; }
        public string ResourceLabel { get; set; }
        public string ResourceLabel2 { get; set; }
        public string ResourceLabel3 { get; set; }
        public string ResourceFilter { get; set; }

        // NotedRelation
        public bool NotedRelationVisibility { get; set; }
        public short? NotedRelationRequiredState { get; set; }
        public short? NotedRelationReadOnlyState { get; set; }
        public List<int> NotedRelationDefinitionIds { get; set; }
        public string NotedRelationLabel { get; set; }
        public string NotedRelationLabel2 { get; set; }
        public string NotedRelationLabel3 { get; set; }
        public string NotedRelationFilter { get; set; }

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
        public string UnitFilter { get; set; }

        // Time1
        public bool Time1Visibility { get; set; }
        public short? Time1RequiredState { get; set; }
        public short? Time1ReadOnlyState { get; set; }
        public string Time1Label { get; set; }
        public string Time1Label2 { get; set; }
        public string Time1Label3 { get; set; }

        // Duration
        public bool DurationVisibility { get; set; }
        public short? DurationRequiredState { get; set; }
        public short? DurationReadOnlyState { get; set; }
        public string DurationLabel { get; set; }
        public string DurationLabel2 { get; set; }
        public string DurationLabel3 { get; set; }

        // DurationUnit
        public bool DurationUnitVisibility { get; set; }
        public short? DurationUnitRequiredState { get; set; }
        public short? DurationUnitReadOnlyState { get; set; }
        public string DurationUnitLabel { get; set; }
        public string DurationUnitLabel2 { get; set; }
        public string DurationUnitLabel3 { get; set; }
        public string DurationUnitFilter { get; set; }

        // Time2
        public bool Time2Visibility { get; set; }
        public short? Time2RequiredState { get; set; }
        public short? Time2ReadOnlyState { get; set; }
        public string Time2Label { get; set; }
        public string Time2Label2 { get; set; }
        public string Time2Label3 { get; set; }

        // ExternalReference
        public bool ExternalReferenceVisibility { get; set; }
        public short? ExternalReferenceRequiredState { get; set; }
        public short? ExternalReferenceReadOnlyState { get; set; }
        public string ExternalReferenceLabel { get; set; }
        public string ExternalReferenceLabel2 { get; set; }
        public string ExternalReferenceLabel3 { get; set; }

        // ReferenceSource
        public bool ReferenceSourceVisibility { get; set; }
        public short? ReferenceSourceRequiredState { get; set; }
        public short? ReferenceSourceReadOnlyState { get; set; }
        public string ReferenceSourceLabel { get; set; }
        public string ReferenceSourceLabel2 { get; set; }
        public string ReferenceSourceLabel3 { get; set; }
        public string ReferenceSourceFilter { get; set; }

        // InternalReference
        public bool InternalReferenceVisibility { get; set; }
        public short? InternalReferenceRequiredState { get; set; }
        public short? InternalReferenceReadOnlyState { get; set; }
        public string InternalReferenceLabel { get; set; }
        public string InternalReferenceLabel2 { get; set; }
        public string InternalReferenceLabel3 { get; set; }

        // Clearance
        public string ClearanceVisibility { get; set; }

        public bool CanReachState1 { get; set; }
        public bool CanReachState2 { get; set; }
        public bool CanReachState3 { get; set; }
        public bool HasWorkflow { get; set; }
        public List<DocumentDefinitionLineDefinitionForClient> LineDefinitions { get; set; }
    }

    public class DocumentDefinitionLineDefinitionForClient
    {
        public int LineDefinitionId { get; set; }
        public bool IsVisibleByDefault { get; set; }
    }

    public class MarkupTemplateForClient
    {
        public int MarkupTemplateId { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public bool SupportsPrimaryLanguage { get; set; }
        public bool SupportsSecondaryLanguage { get; set; }
        public bool SupportsTernaryLanguage { get; set; }
        public string Usage { get; set; }
        public string Collection { get; set; }
        public int? DefinitionId { get; set; }
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

        public int? BarcodeColumnIndex { get; set; }
        public string BarcodeProperty { get; set; }
        public string BarcodeExistingItemHandling { get; set; }
        public bool BarcodeBeepsEnabled { get; set; }

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
        public List<int> RelationDefinitionIds { get; set; }
        public List<int> ResourceDefinitionIds { get; set; }
        public List<int> NotedRelationDefinitionIds { get; set; }
        // public List<int> ReferenceSourceDefinitionIds { get; set; }
    }

    public class LineDefinitionColumnForClient
    {
        public string ColumnName { get; set; }
        public int EntryIndex { get; set; }
        public string Label { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Filter { get; set; }
        public short? RequiredState { get; set; }
        public short? ReadOnlyState { get; set; }
        public byte? InheritsFromHeader { get; set; }
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
        public string Control { get; set; }
        public string ControlOptions { get; set; }
    }

    ///////////////////// Other Definitions

    public class ResourceDefinitionForClient : MasterDetailDefinitionForClient
    {
        public string ResourceDefinitionType { get; set; }
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

        public List<DefinitionReportDefinitionForClient> ReportDefinitions { get; set; }
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

        public string DateOfBirthVisibility { get; set; }
        public string ContactEmailVisibility { get; set; }
        public string ContactMobileVisibility { get; set; }
        public string ContactAddressVisibility { get; set; }

        // Date 1
        public string Date1Label { get; set; }
        public string Date1Label2 { get; set; }
        public string Date1Label3 { get; set; }
        public string Date1Visibility { get; set; }

        // Date 2
        public string Date2Label { get; set; }
        public string Date2Label2 { get; set; }
        public string Date2Label3 { get; set; }
        public string Date2Visibility { get; set; }

        // Date 3
        public string Date3Label { get; set; }
        public string Date3Label2 { get; set; }
        public string Date3Label3 { get; set; }
        public string Date3Visibility { get; set; }

        // Date 4
        public string Date4Label { get; set; }
        public string Date4Label2 { get; set; }
        public string Date4Label3 { get; set; }
        public string Date4Visibility { get; set; }

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


        // Lookup 5
        public string Lookup5Label { get; set; }
        public string Lookup5Label2 { get; set; }
        public string Lookup5Label3 { get; set; }
        public string Lookup5Visibility { get; set; }
        public int? Lookup5DefinitionId { get; set; }

        // Lookup 6
        public string Lookup6Label { get; set; }
        public string Lookup6Label2 { get; set; }
        public string Lookup6Label3 { get; set; }
        public string Lookup6Visibility { get; set; }
        public int? Lookup6DefinitionId { get; set; }

        // Lookup 7
        public string Lookup7Label { get; set; }
        public string Lookup7Label2 { get; set; }
        public string Lookup7Label3 { get; set; }
        public string Lookup7Visibility { get; set; }
        public int? Lookup7DefinitionId { get; set; }

        // Lookup 8
        public string Lookup8Label { get; set; }
        public string Lookup8Label2 { get; set; }
        public string Lookup8Label3 { get; set; }
        public string Lookup8Visibility { get; set; }
        public int? Lookup8DefinitionId { get; set; }

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

        // Text 3
        public string Text3Label { get; set; }
        public string Text3Label2 { get; set; }
        public string Text3Label3 { get; set; }
        public string Text3Visibility { get; set; }

        // Text 4
        public string Text4Label { get; set; }
        public string Text4Label2 { get; set; }
        public string Text4Label3 { get; set; }
        public string Text4Visibility { get; set; }

        // ExternalReference
        public string ExternalReferenceLabel { get; set; }
        public string ExternalReferenceLabel2 { get; set; }
        public string ExternalReferenceLabel3 { get; set; }
        public string ExternalReferenceVisibility { get; set; }

        public string AgentVisibility { get; set; }
        public string TaxIdentificationNumberVisibility { get; set; }
        public string BankAccountNumberVisibility { get; set; }
        public string UserCardinality { get; set; }
        public bool? HasAttachments { get; set; }
        public int? AttachmentsCategoryDefinitionId { get; set; }


        // Relation 1
        public string Relation1Label { get; set; }
        public string Relation1Label2 { get; set; }
        public string Relation1Label3 { get; set; }
        public string Relation1Visibility { get; set; }
        public int? Relation1DefinitionId { get; set; }

        public List<DefinitionReportDefinitionForClient> ReportDefinitions { get; set; }
    }

    public class LookupDefinitionForClient : MasterDetailDefinitionForClient
    {
        public List<DefinitionReportDefinitionForClient> ReportDefinitions { get; set; }
    }

    public class DefinitionReportDefinitionForClient
    {
        public int ReportDefinitionId { get; set; }

        public string Name { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
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
