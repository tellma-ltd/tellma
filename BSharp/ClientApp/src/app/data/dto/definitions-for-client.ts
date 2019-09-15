// tslint:disable:variable-name
export class DefinitionsForClient {

    Documents: { [subtype: string]: DocumentDefinitionForClient };
    Lines: { [subtype: string]: LineDefinitionForClient };
    Resources: { [subtype: string]: ResourceDefinitionForClient };
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

