// tslint:disable:variable-name
export class DefinitionsForClient {

    Documents: { [subtype: string]: DocumentDefinitionForClient };
    Lines: { [subtype: string]: LineDefinitionForClient };
    Resources: { [subtype: string]: ResourceDefinitionForClient };
    ResourceLookups: { [subtype: string]: ResourceLookupDefinitionForClient };
}

export abstract class DefinitionForClient {
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

export class DocumentDefinitionForClient extends DefinitionForClient {
    // TODO
    IsSourceDocument: boolean;
    FinalState: string;
}

export class LineDefinitionForClient extends DefinitionForClient {
    // TODO
}

export class ResourceDefinitionForClient extends DefinitionForClient {
    // TODO
}

export class ResourceLookupDefinitionForClient extends DefinitionForClient {
}

