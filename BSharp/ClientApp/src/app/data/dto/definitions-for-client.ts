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
}

export interface ReportParameterDefinitionForClient {
    Key: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    IsRequired: boolean;
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

export type Visibility = null | 'Optional' | 'Required';

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

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility; // 0, 1, 2 (not visible, visible, visible and required)
    Lookup1DefaultValue: number;
    Lookup1DefinitionId: string;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefaultValue: number;
    Lookup2DefinitionId: string;

    //// Lookup 3
    // Lookup3Label: string;
    // Lookup3Label2: string;
    // Lookup3Label3: string;
    // Lookup3Visibility: Visibility;
    // Lookup3DefaultValue: number;
    // Lookup3DefinitionId: string;

    //// Lookup 4
    // Lookup4Label: string;
    // Lookup4Label2: string;
    // Lookup4Label3: string;
    // Lookup4Visibility: Visibility;
    // Lookup4DefaultValue: number;
    // Lookup4DefinitionId: string;

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
    BasicSalaryVisibility?: string;
    TransportationAllowanceVisibility?: string;
    OvertimeRateVisibility?: string;
    BankAccountNumberVisibility?: string;
}
