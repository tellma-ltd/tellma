// tslint:disable:variable-name
export class DefinitionsForClient {

    Documents: { [subtype: string]: DocumentDefinitionForClient };
    Lines: { [subtype: string]: LineDefinitionForClient };
    Resources: { [subtype: string]: ResourceDefinitionForClient };
    ResourceLookups: { [subtype: string]: ResourceLookupDefinitionForClient };
}

export class DocumentDefinitionForClient {
    // TODO
    IsSourceDocument: boolean;
    FinalState: string;
}

export class LineDefinitionForClient {
    // TODO
}

export class ResourceDefinitionForClient {
    // TODO
}

export class ResourceLookupDefinitionForClient {
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

