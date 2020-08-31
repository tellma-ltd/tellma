import { ReportOrderDirection, Aggregation, ReportType, ChartType, Modifier } from '../entities/report-definition';
import { LineState, PositiveLineState } from '../entities/line';
import { MarkupTemplateUsage } from '../entities/markup-template';
import { DefinitionVisibility as Visibility, DefinitionCardinality, DefinitionState } from '../entities/base/definition-common';

// tslint:disable:variable-name
export interface DefinitionsForClient {
    Documents: { [definitionId: number]: DocumentDefinitionForClient };
    Lines: { [definitionId: number]: LineDefinitionForClient };
    Relations: { [definitionId: number]: RelationDefinitionForClient };
    Custodies: { [definitionId: number]: CustodyDefinitionForClient };
    Resources: { [definitionId: number]: ResourceDefinitionForClient };
    Lookups: { [definitionId: number]: LookupDefinitionForClient };
    Reports: { [definitionId: number]: ReportDefinitionForClient };

    ManualJournalVouchersDefinitionId: number;
    ManualLinesDefinitionId: number;
}

export interface DefinitionForClient {
    MainMenuSection?: string;
    MainMenuIcon?: string;
    MainMenuSortKey?: number;
}

export interface MasterDetailsDefinitionForClient extends DefinitionForClient {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;
    State?: DefinitionState;
}

export interface ReportDefinitionForClient extends DefinitionForClient {
    Title: string;
    Title2?: string;
    Title3?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    Type: ReportType; // summary or details
    Chart?: ChartType;
    DefaultsToChart: boolean; // ?
    Collection: string;
    DefinitionId?: number;
    Select: ReportSelectDefinitionForClient[];
    Parameters?: ReportParameterDefinitionForClient[];
    Filter?: string;
    OrderBy?: string;
    Rows: ReportDimensionDefinitionForClient[];
    Columns: ReportDimensionDefinitionForClient[];
    Measures: ReportMeasureDefinitionForClient[];
    Top?: number;
    ShowColumnsTotal: boolean;
    ShowRowsTotal: boolean;
    ShowInMainMenu: boolean;
}

export interface ReportParameterDefinitionForClient {
    Key: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: Visibility;
    Value?: string;
}

export interface ReportSelectDefinitionForClient {
    Path: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
}

export interface ReportMeasureDefinitionForClient {
    Path: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    Aggregation: Aggregation;
}

export interface ReportDimensionDefinitionForClient {
    Path: string;
    Modifier?: Modifier;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    AutoExpand: boolean;
}

export interface DocumentDefinitionForClient extends MasterDetailsDefinitionForClient {
    IsOriginalDocument?: boolean;
    DocumentType?: number;
    Prefix?: string;
    CodeWidth?: number;

    // Memo
    MemoVisibility?: Visibility;
    MemoIsCommonVisibility?: boolean;
    MemoLabel?: string;
    MemoLabel2?: string;
    MemoLabel3?: string;
    MemoRequiredState?: PositiveLineState | 5;
    MemoReadOnlyState?: PositiveLineState | 5;

    // Posting Date
    PostingDateVisibility?: boolean;
    PostingDateRequiredState?: PositiveLineState | 5;
    PostingDateReadOnlyState?: PositiveLineState | 5;
    PostingDateLabel?: string;
    PostingDateLabel2?: string;
    PostingDateLabel3?: string;

    // Debit Resource
    DebitResourceVisibility?: boolean;
    DebitResourceRequiredState?: PositiveLineState | 5;
    DebitResourceReadOnlyState?: PositiveLineState | 5;
    DebitResourceDefinitionIds?: number[];
    DebitResourceLabel?: string;
    DebitResourceLabel2?: string;
    DebitResourceLabel3?: string;

    // Credit Resource
    CreditResourceVisibility?: boolean;
    CreditResourceRequiredState?: PositiveLineState | 5;
    CreditResourceReadOnlyState?: PositiveLineState | 5;
    CreditResourceDefinitionIds?: number[];
    CreditResourceLabel?: string;
    CreditResourceLabel2?: string;
    CreditResourceLabel3?: string;

    // Debit Custody
    DebitCustodyVisibility?: boolean;
    DebitCustodyRequiredState?: PositiveLineState | 5;
    DebitCustodyReadOnlyState?: PositiveLineState | 5;
    DebitCustodyDefinitionIds?: number[];
    DebitCustodyLabel?: string;
    DebitCustodyLabel2?: string;
    DebitCustodyLabel3?: string;

    // Credit Custody
    CreditCustodyVisibility?: boolean;
    CreditCustodyRequiredState?: PositiveLineState | 5;
    CreditCustodyReadOnlyState?: PositiveLineState | 5;
    CreditCustodyDefinitionIds?: number[];
    CreditCustodyLabel?: string;
    CreditCustodyLabel2?: string;
    CreditCustodyLabel3?: string;

    // Noted Relation
    NotedRelationVisibility?: boolean;
    NotedRelationRequiredState?: PositiveLineState | 5;
    NotedRelationReadOnlyState?: PositiveLineState | 5;
    NotedRelationDefinitionIds?: number[];
    NotedRelationLabel?: string;
    NotedRelationLabel2?: string;
    NotedRelationLabel3?: string;

    // Center
    CenterVisibility?: boolean;
    CenterRequiredState?: PositiveLineState | 5;
    CenterReadOnlyState?: PositiveLineState | 5;
    CenterDefinitionIds?: number[];
    CenterLabel?: string;
    CenterLabel2?: string;
    CenterLabel3?: string;

    // Clearance
    ClearanceVisibility?: Visibility;

    // Time1
    Time1Visibility?: boolean;
    Time1RequiredState?: PositiveLineState | 5;
    Time1ReadOnlyState?: PositiveLineState | 5;
    Time1Label?: string;
    Time1Label2?: string;
    Time1Label3?: string;

    // Time2
    Time2Visibility?: boolean;
    Time2RequiredState?: PositiveLineState | 5;
    Time2ReadOnlyState?: PositiveLineState | 5;
    Time2Label?: string;
    Time2Label2?: string;
    Time2Label3?: string;

    // Quantity
    QuantityVisibility?: boolean;
    QuantityRequiredState?: PositiveLineState | 5;
    QuantityReadOnlyState?: PositiveLineState | 5;
    QuantityLabel?: string;
    QuantityLabel2?: string;
    QuantityLabel3?: string;

    // Unit
    UnitVisibility?: boolean;
    UnitRequiredState?: PositiveLineState | 5;
    UnitReadOnlyState?: PositiveLineState | 5;
    UnitLabel?: string;
    UnitLabel2?: string;
    UnitLabel3?: string;

    // Currency
    CurrencyVisibility?: boolean;
    CurrencyRequiredState?: PositiveLineState | 5;
    CurrencyReadOnlyState?: PositiveLineState | 5;
    CurrencyLabel?: string;
    CurrencyLabel2?: string;
    CurrencyLabel3?: string;

    CanReachState1?: boolean;
    CanReachState2?: boolean;
    CanReachState3?: boolean;
    HasWorkflow?: boolean;
    LineDefinitions?: DocumentDefinitionLineDefinitionForClient[];
    MarkupTemplates?: DocumentDefinitionMarkupTemplateForClient[];
}

export interface DocumentDefinitionMarkupTemplateForClient {
    MarkupTemplateId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    SupportsPrimaryLanguage?: boolean;
    SupportsSecondaryLanguage?: boolean;
    SupportsTernaryLanguage?: boolean;
    Usage?: MarkupTemplateUsage;
}

export interface DocumentDefinitionLineDefinitionForClient {
    LineDefinitionId?: number;
    IsVisibleByDefault?: boolean;
}

export interface LineDefinitionForClient {
    Code: string;
    TitleSingular: string;
    TitleSingular2: string;
    TitleSingular3: string;
    TitlePlural: string;
    TitlePlural2: string;
    TitlePlural3: string;
    AllowSelectiveSigning: boolean;
    ViewDefaultsToForm: boolean;
    GenerateScript: boolean;
    GenerateLabel: string;
    GenerateLabel2: string;
    GenerateLabel3: string;
    Entries: LineDefinitionEntryForClient[];
    Columns: LineDefinitionColumnForClient[];
    StateReasons: LineDefinitionStateReasonForClient[];
    GenerateParameters: LineDefinitionGenerateParameterForClient[];
}

export interface LineDefinitionEntryForClient {
    Direction: 1 | -1;
    ParentAccountTypeId?: number;
    EntryTypeId?: number;
    EntryTypeParentId?: number; // Comes from the Account Types
    CustodianDefinitionIds: number[];
    CustodyDefinitionIds: number[];
    ParticipantDefinitionIds: number[];
    ResourceDefinitionIds: number[];
    NotedRelationDefinitionIds: number[];
}

export interface LineDefinitionColumnForClient {
    ColumnName: EntryColumnName;
    EntryIndex: number;
    Label: string;
    Label2: string;
    Label3: string;
    Filter?: string;
    RequiredState?: PositiveLineState | 5;
    ReadOnlyState?: PositiveLineState | 5;
    InheritsFromHeader?: boolean;
}

export interface LineDefinitionStateReasonForClient {
    Id: number;
    State: number;
    Name: string;
    Name2: string;
    Name3: string;
    IsActive: boolean;
}

export interface LineDefinitionGenerateParameterForClient {
    Key: string;
    Label: string;
    Label2: string;
    Label3: string;
    DataType: string;
    Filter: string;
    Visibility: Visibility;
}

export const entryColumnNames = ['Memo', 'PostingDate', 'TemplateLineId',
    'Multiplier', 'AccountId', 'CurrencyId',
    'CustodianId', 'CustodyId', 'ParticipantId', 'ResourceId', 'CenterId', 'EntryTypeId',
    'MonetaryValue', 'Quantity', 'UnitId', 'Time1', 'Time2', 'Value',
    'ExternalReference', 'AdditionalReference', 'NotedRelationId',
    'NotedAgentName', 'NotedAmount', 'NotedDate'];

export type EntryColumnName = 'Memo' | 'PostingDate' | 'TemplateLineId' |
    'Multiplier' | 'AccountId' | 'CurrencyId' |
    'CustodianId' | 'CustodyId' | 'ParticipantId' | 'ResourceId' | 'CenterId' | 'EntryTypeId' |
    'MonetaryValue' | 'Quantity' | 'UnitId' | 'Time1' | 'Time2' | 'Value' |
    'ExternalReference' | 'AdditionalReference' | 'NotedRelationId' |
    'NotedAgentName' | 'NotedAmount' | 'NotedDate';

export interface ResourceDefinitionForClient extends MasterDetailsDefinitionForClient {

    CurrencyVisibility: Visibility;
    CenterVisibility: Visibility;
    ImageVisibility: Visibility;
    DescriptionVisibility: Visibility;
    LocationVisibility: Visibility;

    FromDateLabel: string;
    FromDateLabel2: string;
    FromDateLabel3: string;
    FromDateVisibility: Visibility;

    ToDateLabel: string;
    ToDateLabel2: string;
    ToDateLabel3: string;
    ToDateVisibility: Visibility;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefinitionId: number;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;

    // Resource Only

    IdentifierLabel: string;
    IdentifierLabel2: string;
    IdentifierLabel3: string;
    IdentifierVisibility: Visibility;

    VatRateVisibility: Visibility;
    DefaultVatRate?: number;

    ReorderLevelVisibility: Visibility;
    EconomicOrderQuantityVisibility: Visibility;
    UnitCardinality: DefinitionCardinality;
    DefaultUnitId?: number;
    UnitMassVisibility?: Visibility;
    DefaultUnitMassUnitId?: number;
    MonetaryValueVisibility: Visibility;
    ParticipantVisibility?: Visibility;
    ParticipantDefinitionId?: number;
}

// tslint:disable-next-line:no-empty-interface
export interface LookupDefinitionForClient extends MasterDetailsDefinitionForClient {
}

export interface RelationDefinitionForClient extends MasterDetailsDefinitionForClient {

    CurrencyVisibility: Visibility;
    CenterVisibility: Visibility;
    ImageVisibility: Visibility;
    DescriptionVisibility: Visibility;
    LocationVisibility: Visibility;

    FromDateLabel: string;
    FromDateLabel2: string;
    FromDateLabel3: string;
    FromDateVisibility: Visibility;

    ToDateLabel: string;
    ToDateLabel2: string;
    ToDateLabel3: string;
    ToDateVisibility: Visibility;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefinitionId: number;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;

    // Relation Only

    AgentVisibility?: Visibility;
    TaxIdentificationNumberVisibility?: Visibility;
    JobVisibility?: Visibility;
    BankAccountNumberVisibility?: Visibility;
    UserCardinality?: DefinitionCardinality;
}

export interface CustodyDefinitionForClient extends MasterDetailsDefinitionForClient {

    CurrencyVisibility: Visibility;
    CenterVisibility: Visibility;
    ImageVisibility: Visibility;
    DescriptionVisibility: Visibility;
    LocationVisibility: Visibility;

    FromDateLabel: string;
    FromDateLabel2: string;
    FromDateLabel3: string;
    FromDateVisibility: Visibility;

    ToDateLabel: string;
    ToDateLabel2: string;
    ToDateLabel3: string;
    ToDateVisibility: Visibility;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefinitionId: number;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;

    // Custody Only

    CustodianVisibility?: Visibility;
    CustodianDefinitionId?: number;

    ExternalReferenceLabel: string;
    ExternalReferenceLabel2: string;
    ExternalReferenceLabel3: string;
    ExternalReferenceVisibility?: Visibility;
}
