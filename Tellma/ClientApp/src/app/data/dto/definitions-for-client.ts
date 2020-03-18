import { ReportOrderDirection, Aggregation, ReportType, ChartType, Modifier } from '../entities/report-definition';
import { LineState } from '../entities/line';

// tslint:disable:variable-name
export class DefinitionsForClient {
    Documents: { [definitionId: string]: DocumentDefinitionForClient };
    Lines: { [definitionId: string]: LineDefinitionForClient };
    Agents: { [definitionId: string]: AgentDefinitionForClient };
    Resources: { [definitionId: string]: ResourceDefinitionForClient };
    Lookups: { [definitionId: string]: LookupDefinitionForClient };
    Reports: { [definitionId: string]: ReportDefinitionForClient };
}

export interface DefinitionForClient {
    MainMenuSection: string;
    MainMenuIcon: string;
    MainMenuSortKey: number;
}

export interface MasterDetailsDefinitionForClient extends DefinitionForClient {
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
    DefinitionId?: string;
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
    AgentDefinitionId: string;
    AgentLabel: string;
    AgentLabel2: string;
    AgentLabel3: string;
    ClearanceVisibility: Visibility;
    InvestmentCenterVisibility: Visibility;
    Time1Visibility: Visibility;
    Time1Label: string;
    Time1Label2: string;
    Time1Label3: string;
    Time2Visibility: Visibility;
    Time2Label: string;
    Time2Label2: string;
    Time2Label3: string;
    QuantityVisibility: Visibility;
    QuantityLabel: string;
    QuantityLabel2: string;
    QuantityLabel3: string;
    UnitVisibility: Visibility;
    UnitLabel: string;
    UnitLabel2: string;
    UnitLabel3: string;
    CurrencyVisibility: Visibility;
    LineDefinitions: DocumentDefinitionLineDefinitionForClient[];
}

export interface DocumentDefinitionLineDefinitionForClient {
    LineDefinitionId: string;
    IsVisibleByDefault: boolean;
}

export interface LineDefinitionForClient extends MasterDetailsDefinitionForClient {
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
    AccountTypeParentCode: string;
    AgentDefinitionId: string;
    NotedAgentDefinitionId: string;
    EntryTypeCode: string;

    // Computed
    AccountTypeParentId?: number;
    AccountTypeParentIsResourceClassification: boolean;
    EntryTypeParentId?: number;
}

export interface LineDefinitionColumnForClient {
    TableName: 'Lines' | 'Entries';
    ColumnName: string;
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

    CostObjectVisibility: Visibility;
    ExpenseEntryTypeVisibility: Visibility;
    ExpenseCenterVisibility: Visibility;
    InvestmentCenterVisibility: Visibility;
    ResidualMonetaryValueVisibility: Visibility;
    ResidualValueVisibility: Visibility;

    ReorderLevelVisibility: string;
    ReorderLevelDefaultValue: number;

    EconomicOrderQuantityVisibility: string;
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
    Lookup1DefinitionId: string;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefaultValue: number;
    Lookup2DefinitionId: string;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefaultValue: number;
    Lookup3DefinitionId: string;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefaultValue: number;
    Lookup4DefinitionId: string;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefaultValue: number;
    // Lookup5DefinitionId: string;

    DueDateLabel: string;
    DueDateLabel2: string;
    DueDateLabel3: string;
    DueDateVisibility: Visibility;
    DueDateDefaultValue: string;

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

export interface LookupDefinitionForClient extends MasterDetailsDefinitionForClient {
    Bla: string;
}

export interface AgentDefinitionForClient extends MasterDetailsDefinitionForClient {

    TaxIdentificationNumberVisibility?: string;
    StartDateVisibility?: string;
    StartDateLabel?: string;
    StartDateLabel2?: string;
    StartDateLabel3?: string;
    JobVisibility?: string;
    RatesVisibility?: string;
    RatesLabel?: string;
    RatesLabel2?: string;
    RatesLabel3?: string;
    BankAccountNumberVisibility?: string;
}
