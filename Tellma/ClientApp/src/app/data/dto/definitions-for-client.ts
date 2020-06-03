import { ReportOrderDirection, Aggregation, ReportType, ChartType, Modifier } from '../entities/report-definition';
import { LineState } from '../entities/line';
import { MarkupTemplateUsage } from '../entities/markup-template';

// tslint:disable:variable-name
export interface DefinitionsForClient {
    Documents: { [definitionId: number]: DocumentDefinitionForClient };
    Lines: { [definitionId: number]: LineDefinitionForClient };
    Agents: { [definitionId: number]: AgentDefinitionForClient };
    Resources: { [definitionId: number]: ResourceDefinitionForClient };
    Lookups: { [definitionId: number]: LookupDefinitionForClient };
    Reports: { [definitionId: number]: ReportDefinitionForClient };
}

export interface DefinitionForClient {
    MainMenuSection: string;
    MainMenuIcon: string;
    MainMenuSortKey: number;
}

export interface MasterDetailsDefinitionForClient extends DefinitionForClient {
    Code?: string;
    TitleSingular: string;
    TitleSingular2: string;
    TitleSingular3: string;
    TitlePlural: string;
    TitlePlural2: string;
    TitlePlural3: string;
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
    IsOriginalDocument: boolean;
    Prefix: string;
    CodeWidth: number;

    // Memo
    MemoVisibility: Visibility;
    MemoIsCommonVisibility: boolean;
    MemoLabel: string;
    MemoLabel2: string;
    MemoLabel3: string;
    MemoRequiredState: LineState;
    MemoReadOnlyState: LineState;

    // Debit Agent
    DebitAgentVisibility: boolean;
    DebitAgentRequiredState: LineState;
    DebitAgentReadOnlyState: LineState;
    DebitAgentDefinitionId: number;
    DebitAgentLabel: string;
    DebitAgentLabel2: string;
    DebitAgentLabel3: string;

    // Credit Agent
    CreditAgentVisibility: boolean;
    CreditAgentRequiredState: LineState;
    CreditAgentReadOnlyState: LineState;
    CreditAgentDefinitionId: number;
    CreditAgentLabel: string;
    CreditAgentLabel2: string;
    CreditAgentLabel3: string;

    // Noted Agent
    NotedAgentVisibility: boolean;
    NotedAgentRequiredState: LineState;
    NotedAgentReadOnlyState: LineState;
    NotedAgentDefinitionId: number;
    NotedAgentLabel: string;
    NotedAgentLabel2: string;
    NotedAgentLabel3: string;

    // Clearance
    ClearanceVisibility: Visibility;

    // Investment Center
    InvestmentCenterVisibility: boolean;
    InvestmentCenterRequiredState: LineState;
    InvestmentCenterReadOnlyState: LineState;
    InvestmentCenterLabel: string;
    InvestmentCenterLabel2: string;
    InvestmentCenterLabel3: string;

    // Time1
    Time1Visibility: boolean;
    Time1RequiredState: LineState;
    Time1ReadOnlyState: LineState;
    Time1Label: string;
    Time1Label2: string;
    Time1Label3: string;

    // Time2
    Time2Visibility: boolean;
    Time2RequiredState: LineState;
    Time2ReadOnlyState: LineState;
    Time2Label: string;
    Time2Label2: string;
    Time2Label3: string;

    // Quantity
    QuantityVisibility: boolean;
    QuantityRequiredState: LineState;
    QuantityReadOnlyState: LineState;
    QuantityLabel: string;
    QuantityLabel2: string;
    QuantityLabel3: string;

    // Unit
    UnitVisibility: boolean;
    UnitRequiredState: LineState;
    UnitReadOnlyState: LineState;
    UnitLabel: string;
    UnitLabel2: string;
    UnitLabel3: string;

    // Currency
    CurrencyVisibility: boolean;
    CurrencyRequiredState: LineState;
    CurrencyReadOnlyState: LineState;
    CurrencyLabel: string;
    CurrencyLabel2: string;
    CurrencyLabel3: string;

    CanReachState1: boolean;
    CanReachState2: boolean;
    CanReachState3: boolean;
    HasWorkflow: boolean;
    LineDefinitions: DocumentDefinitionLineDefinitionForClient[];
    MarkupTemplates: DocumentDefinitionMarkupTemplateForClient[];
}

export interface DocumentDefinitionMarkupTemplateForClient {
    MarkupTemplateId: number;
    Name: string;
    Name2: string;
    Name3: string;
    SupportsPrimaryLanguage: boolean;
    SupportsSecondaryLanguage: boolean;
    SupportsTernaryLanguage: boolean;
    Usage: MarkupTemplateUsage;
}

export interface DocumentDefinitionLineDefinitionForClient {
    LineDefinitionId: number;
    IsVisibleByDefault: boolean;
}

export interface LineDefinitionForClient extends MasterDetailsDefinitionForClient {
    Code: string;
    TitleSingular: string;
    TitleSingular2: string;
    TitleSingular3: string;
    TitlePlural: string;
    TitlePlural2: string;
    TitlePlural3: string;
    AllowSelectiveSigning: boolean;
    ViewDefaultsToForm: boolean;
    Entries: LineDefinitionEntryForClient[];
    Columns: LineDefinitionColumnForClient[];
    StateReasons: LineDefinitionStateReasonForClient[];
}

export interface LineDefinitionEntryForClient {
    Direction: 1 | -1;
    AccountTypeId?: number;
    EntryTypeId: number;

    // Copied from AccountTypeParent
    EntryTypeParentId?: number;
    AgentDefinitionIds: number[];
    NotedAgentDefinitionIds: number[];
    ResourceDefinitionIds: number[];
    // DueDateLabel?: string;
    // DueDateLabel2?: string;
    // DueDateLabel3?: string;
    // Time1Label?: string;
    // Time1Label2?: string;
    // Time1Label3?: string;
    // Time2Label?: string;
    // Time2Label2?: string;
    // Time2Label3?: string;
    // ExternalReferenceLabel?: string;
    // ExternalReferenceLabel2?: string;
    // ExternalReferenceLabel3?: string;
    // AdditionalReferenceLabel?: string;
    // AdditionalReferenceLabel2?: string;
    // AdditionalReferenceLabel3?: string;
    // NotedAgentNameLabel?: string;
    // NotedAgentNameLabel2?: string;
    // NotedAgentNameLabel3?: string;
    // NotedAmountLabel?: string;
    // NotedAmountLabel2?: string;
    // NotedAmountLabel3?: string;
    // NotedDateLabel?: string;
    // NotedDateLabel2?: string;
    // NotedDateLabel3?: string;
}

export interface LineDefinitionColumnForClient {
    ColumnName: EntryColumnName;
    EntryIndex: number;
    Label: string;
    Label2: string;
    Label3: string;
    RequiredState?: number;
    ReadOnlyState?: LineState;
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

export type EntryColumnName = 'Memo' | 'AccountId' | 'CurrencyId' |
    'AgentId' | 'ResourceId' | 'CenterId' | 'EntryTypeId' | 'DueDate' |
    'MonetaryValue' | 'Quantity' | 'UnitId' | 'Time1' | 'Time2' | 'Value' |
    'ExternalReference' | 'AdditionalReference' | 'NotedAgentId' |
    'NotedAgentName' | 'NotedAmount' | 'NotedDate';

export type Visibility = 'None' | 'Optional' | 'Required';

export interface ResourceDefinitionForClient extends MasterDetailsDefinitionForClient {

    IdentifierLabel: string;
    IdentifierLabel2: string;
    IdentifierLabel3: string;
    IdentifierVisibility: Visibility;
    IdentifierDefaultValue: string;

    CurrencyLabel: string;
    CurrencyLabel2: string;
    CurrencyLabel3: string;
    CurrencyVisibility: Visibility;
    CurrencyDefaultValue: string;

    MonetaryValueLabel: string;
    MonetaryValueLabel2: string;
    MonetaryValueLabel3: string;
    MonetaryValueVisibility: Visibility;
    MonetaryValueDefaultValue: number;

    DescriptionVisibility: Visibility;

    ExpenseEntryTypeVisibility: Visibility;
    CenterVisibility: Visibility;
    ResidualMonetaryValueVisibility: Visibility;
    ResidualValueVisibility: Visibility;

    ReorderLevelVisibility: Visibility;
    ReorderLevelDefaultValue: number;

    EconomicOrderQuantityVisibility: Visibility;
    EconomicOrderQuantityDefaultValue: number;

    AvailableSinceLabel: string;
    AvailableSinceLabel2: string;
    AvailableSinceLabel3: string;
    AvailableSinceVisibility: Visibility;
    AvailableSinceDefaultValue: string;

    AvailableTillLabel: string;
    AvailableTillLabel2: string;
    AvailableTillLabel3: string;
    AvailableTillVisibility: Visibility;
    AvailableTillDefaultValue: string;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;
    Decimal1DefaultValue: number;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;
    Decimal2DefaultValue: number;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;
    Int1DefaultValue: number;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;
    Int2DefaultValue: number;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefaultValue: number;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefaultValue: number;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefaultValue: number;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefaultValue: number;
    Lookup4DefinitionId: number;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefaultValue: number;
    // Lookup5DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;
    Text1DefaultValue: string;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;
    Text2DefaultValue: string;
}

// tslint:disable-next-line:no-empty-interface
export interface LookupDefinitionForClient extends MasterDetailsDefinitionForClient {
}

export interface AgentDefinitionForClient extends MasterDetailsDefinitionForClient {

    TaxIdentificationNumberVisibility?: Visibility;
    ImageVisibility: Visibility;
    StartDateVisibility?: Visibility;
    StartDateLabel?: string;
    StartDateLabel2?: string;
    StartDateLabel3?: string;
    JobVisibility?: Visibility;
    BankAccountNumberVisibility?: Visibility;
}
