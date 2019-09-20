// tslint:disable:variable-name
export class DefinitionsForClient {

    Documents: { [subtype: string]: DocumentDefinitionForClient };
    Lines: { [subtype: string]: LineDefinitionForClient };
    Resources: { [subtype: string]: ResourceDefinitionForClient };
    ResourceLookups: { [subtype: string]: ResourceLookupDefinitionForClient };
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

export interface ResourceDefinitionForClient extends DefinitionForClient {
    MassUnit_Label: string;
    MassUnit_Label2: string;
    MassUnit_Label3: string;
    MassUnit_Visibility: 0 | 1 | 2;
    MassUnit_DefaultValue: number;


    VolumeUnit_Label: string;
    VolumeUnit_Label2: string;
    VolumeUnit_Label3: string;
    VolumeUnit_Visibility: 0 | 1 | 2;
    VolumeUnit_DefaultValue: number;


    AreaUnit_Label: string;
    AreaUnit_Label2: string;
    AreaUnit_Label3: string;
    AreaUnit_Visibility: 0 | 1 | 2;
    AreaUnit_DefaultValue: number;


    LengthUnit_Label: string;
    LengthUnit_Label2: string;
    LengthUnit_Label3: string;
    LengthUnit_Visibility: 0 | 1 | 2;
    LengthUnit_DefaultValue: number;


    TimeUnit_Label: string;
    TimeUnit_Label2: string;
    TimeUnit_Label3: string;
    TimeUnit_Visibility: 0 | 1 | 2;
    TimeUnit_DefaultValue: number;


    CountUnit_Label: string;
    CountUnit_Label2: string;
    CountUnit_Label3: string;
    CountUnit_Visibility: 0 | 1 | 2;
    CountUnit_DefaultValue: number;

    Memo_Label: string;
    Memo_Label2: string;
    Memo_Label3: string;
    Memo_Visibility: 0 | 1 | 2;
    Memo_DefaultValue: string;

    CustomsReference_Label: string;
    CustomsReference_Label2: string;
    CustomsReference_Label3: string;
    CustomsReference_Visibility: 0 | 1 | 2;
    CustomsReference_DefaultValue: string;

    // Resource Lookup 1
    ResourceLookup1_Label: string;
    ResourceLookup1_Label2: string;
    ResourceLookup1_Label3: string;
    ResourceLookup1_Visibility: 0 | 1 | 2; // 0, 1, 2 (not visible, visible, visible and required)
    ResourceLookup1_DefaultValue: number;
    ResourceLookup1_DefinitionId: string;

    // Resource Lookup 2
    ResourceLookup2_Label: string;
    ResourceLookup2_Label2: string;
    ResourceLookup2_Label3: string;
    ResourceLookup2_Visibility: 0 | 1 | 2;
    ResourceLookup2_DefaultValue: number;
    ResourceLookup2_DefinitionId: string;

    // Resource Lookup 3
    ResourceLookup3_Label: string;
    ResourceLookup3_Label2: string;
    ResourceLookup3_Label3: string;
    ResourceLookup3_Visibility: 0 | 1 | 2;
    ResourceLookup3_DefaultValue: number;
    ResourceLookup3_DefinitionId: string;

    // Resource Lookup 4
    ResourceLookup4_Label: string;
    ResourceLookup4_Label2: string;
    ResourceLookup4_Label3: string;
    ResourceLookup4_Visibility: 0 | 1 | 2;
    ResourceLookup4_DefaultValue: number;
    ResourceLookup4_DefinitionId: string;
}

export interface ResourceLookupDefinitionForClient extends DefinitionForClient {
    Bla: string;
}

