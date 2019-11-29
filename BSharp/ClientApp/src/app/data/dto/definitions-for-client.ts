import { ReportOrderDirection, Aggregation, ReportType, ChartType } from '../entities/report-definition';

// tslint:disable:variable-name
export class DefinitionsForClient {
    Documents: { [definitionId: string]: DocumentDefinitionForClient };
    Lines: { [definitionId: string]: LineDefinitionForClient };
    Agents: { [definitionId: string]: AgentDefinitionForClient };
    Resources: { [definitionId: string]: ResourceDefinitionForClient };
    Accounts: { [definitionId: string]: AccountDefinitionForClient };
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
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    AutoExpand: boolean;
}

export interface DocumentDefinitionForClient extends MasterDetailsDefinitionForClient {
    // TODO
    IsSourceDocument: boolean;
    FinalState: string;
}

export interface LineDefinitionForClient extends MasterDetailsDefinitionForClient {
    // TODO
    Bla: string;
}

export type AccountVisibility = 'None' | 'RequiredInAccounts' | 'RequiredInEntries' | 'OptionalInEntries';
export type Visibility = 0 | 1 | 2;

export interface AccountDefinitionForClient extends MasterDetailsDefinitionForClient {
    ResponsibilityCenter_Label: string;
    ResponsibilityCenter_Label2: string;
    ResponsibilityCenter_Label3: string;
    ResponsibilityCenter_Visibility: AccountVisibility;
    ResponsibilityCenter_DefaultValue: number;

    Custodian_Label: string;
    Custodian_Label2: string;
    Custodian_Label3: string;
    Custodian_Visibility: AccountVisibility;
    Custodian_DefaultValue: number;

    Resource_Label: string;
    Resource_Label2: string;
    Resource_Label3: string;
    Resource_Visibility: AccountVisibility;
    Resource_DefaultValue: number;


    Location_Label: string;
    Location_Label2: string;
    Location_Label3: string;
    Location_Visibility: AccountVisibility;
    Location_DefaultValue: number;


    PartyReference_Label: string;
    PartyReference_Label2: string;
    PartyReference_Label3: string;
    PartyReference_Visibility: AccountVisibility;

}

export interface ResourceDefinitionForClient extends MasterDetailsDefinitionForClient {
    MassUnit_Label: string;
    MassUnit_Label2: string;
    MassUnit_Label3: string;
    MassUnit_Visibility: Visibility;
    MassUnit_DefaultValue: number;


    VolumeUnit_Label: string;
    VolumeUnit_Label2: string;
    VolumeUnit_Label3: string;
    VolumeUnit_Visibility: Visibility;
    VolumeUnit_DefaultValue: number;


    AreaUnit_Label: string;
    AreaUnit_Label2: string;
    AreaUnit_Label3: string;
    AreaUnit_Visibility: Visibility;
    AreaUnit_DefaultValue: number;


    LengthUnit_Label: string;
    LengthUnit_Label2: string;
    LengthUnit_Label3: string;
    LengthUnit_Visibility: Visibility;
    LengthUnit_DefaultValue: number;


    TimeUnit_Label: string;
    TimeUnit_Label2: string;
    TimeUnit_Label3: string;
    TimeUnit_Visibility: Visibility;
    TimeUnit_DefaultValue: number;


    CountUnit_Label: string;
    CountUnit_Label2: string;
    CountUnit_Label3: string;
    CountUnit_Visibility: Visibility;
    CountUnit_DefaultValue: number;

    Memo_Label: string;
    Memo_Label2: string;
    Memo_Label3: string;
    Memo_Visibility: Visibility;
    Memo_DefaultValue: string;

    CustomsReference_Label: string;
    CustomsReference_Label2: string;
    CustomsReference_Label3: string;
    CustomsReference_Visibility: Visibility;
    CustomsReference_DefaultValue: string;

    // Lookup 1
    Lookup1_Label: string;
    Lookup1_Label2: string;
    Lookup1_Label3: string;
    Lookup1_Visibility: Visibility;
    Lookup1_DefaultValue: number;
    Lookup1_DefinitionId: string;

    // Lookup 2
    Lookup2_Label: string;
    Lookup2_Label2: string;
    Lookup2_Label3: string;
    Lookup2_Visibility: Visibility;
    Lookup2_DefaultValue: number;
    Lookup2_DefinitionId: string;

    // Lookup 3
    Lookup3_Label: string;
    Lookup3_Label2: string;
    Lookup3_Label3: string;
    Lookup3_Visibility: Visibility;
    Lookup3_DefaultValue: number;
    Lookup3_DefinitionId: string;

    // Lookup 4
    Lookup4_Label: string;
    Lookup4_Label2: string;
    Lookup4_Label3: string;
    Lookup4_Visibility: Visibility;
    Lookup4_DefaultValue: number;
    Lookup4_DefinitionId: string;

    // Lookup 5
    Lookup5_Label: string;
    Lookup5_Label2: string;
    Lookup5_Label3: string;
    Lookup5_Visibility: Visibility;
    Lookup5_DefaultValue: number;
    Lookup5_DefinitionId: string;
}

export interface LookupDefinitionForClient extends MasterDetailsDefinitionForClient {
    Bla: string;
}

export interface AgentDefinitionForClient extends MasterDetailsDefinitionForClient {
    Bla: string;
}

