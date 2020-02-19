import { ReportOrderDirection, Aggregation, ReportType, ChartType, Modifier } from '../entities/report-definition';

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
    Prefix: string;
    // TODO
}

export interface LineDefinitionForClient extends MasterDetailsDefinitionForClient {
    // TODO
    Bla: string;
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

    CountUnitLabel: string;
    CountUnitLabel2: string;
    CountUnitLabel3: string;
    CountUnitVisibility: Visibility;
    CountUnitDefaultValue: number;

    CountLabel: string;
    CountLabel2: string;
    CountLabel3: string;
    CountVisibility: Visibility;
    CountDefaultValue: number;

    MassUnitLabel: string;
    MassUnitLabel2: string;
    MassUnitLabel3: string;
    MassUnitVisibility: Visibility;
    MassUnitDefaultValue: number;

    MassLabel: string;
    MassLabel2: string;
    MassLabel3: string;
    MassVisibility: Visibility;
    MassDefaultValue: number;

    VolumeUnitLabel: string;
    VolumeUnitLabel2: string;
    VolumeUnitLabel3: string;
    VolumeUnitVisibility: Visibility;
    VolumeUnitDefaultValue: number;

    VolumeLabel: string;
    VolumeLabel2: string;
    VolumeLabel3: string;
    VolumeVisibility: Visibility;
    VolumeDefaultValue: number;

    TimeUnitLabel: string;
    TimeUnitLabel2: string;
    TimeUnitLabel3: string;
    TimeUnitVisibility: Visibility;
    TimeUnitDefaultValue: number;

    TimeLabel: string;
    TimeLabel2: string;
    TimeLabel3: string;
    TimeVisibility: Visibility;
    TimeDefaultValue: number;

    DescriptionVisibility: Visibility;

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
