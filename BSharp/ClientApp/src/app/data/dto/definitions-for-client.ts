// tslint:disable:variable-name
export class DefinitionsForClient {

    Documents: { [definitionId: string]: DocumentDefinitionForClient };
    Lines: { [definitionId: string]: LineDefinitionForClient };
    Resources: { [definitionId: string]: ResourceDefinitionForClient };
    Accounts: { [definitionId: string]: AccountDefinitionForClient };
    Lookups: { [definitionId: string]: LookupDefinitionForClient };
}

export interface DefinitionForClient {
    TitleSingular: string;
    TitleSingular2: string;
    TitleSingular3: string;
    TitlePlural: string;
    TitlePlural2: string;
    TitlePlural3: string;
    MainMenuSection: string;
    MainMenuIcon: string;
    MainMenuSortKey: number;
}

export interface DocumentDefinitionForClient extends DefinitionForClient {
    // TODO
    IsSourceDocument: boolean;
    FinalState: string;
}

export interface LineDefinitionForClient extends DefinitionForClient {
    // TODO
    Bla: string;
}

export type AccountVisibility = 'None' | 'RequiredInAccounts' | 'RequiredInEntries' | 'OptionalInEntries';
export type Visibility = 0 | 1 | 2;

export interface AccountDefinitionForClient extends DefinitionsForClient {
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

export interface ResourceDefinitionForClient extends DefinitionForClient {
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

    // Resource Lookup 1
    ResourceLookup1_Label: string;
    ResourceLookup1_Label2: string;
    ResourceLookup1_Label3: string;
    ResourceLookup1_Visibility: Visibility;
    ResourceLookup1_DefaultValue: number;
    ResourceLookup1_DefinitionId: string;

    // Resource Lookup 2
    ResourceLookup2_Label: string;
    ResourceLookup2_Label2: string;
    ResourceLookup2_Label3: string;
    ResourceLookup2_Visibility: Visibility;
    ResourceLookup2_DefaultValue: number;
    ResourceLookup2_DefinitionId: string;

    // Resource Lookup 3
    ResourceLookup3_Label: string;
    ResourceLookup3_Label2: string;
    ResourceLookup3_Label3: string;
    ResourceLookup3_Visibility: Visibility;
    ResourceLookup3_DefaultValue: number;
    ResourceLookup3_DefinitionId: string;

    // Resource Lookup 4
    ResourceLookup4_Label: string;
    ResourceLookup4_Label2: string;
    ResourceLookup4_Label3: string;
    ResourceLookup4_Visibility: Visibility;
    ResourceLookup4_DefaultValue: number;
    ResourceLookup4_DefinitionId: string;
}

export interface LookupDefinitionForClient extends DefinitionForClient {
    Bla: string;
}

