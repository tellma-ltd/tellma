// tslint:disable:variable-name
export interface Entity {
    EntityMetadata?: { [field: string]: 0 | 1 | 2 }; // 1 == Restricted, 2 == Loaded
}
