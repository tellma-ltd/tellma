import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryRelationDefinitionForSave extends EntityForSave {
    RelationDefinitionId?: number;
}

export interface LineDefinitionEntryRelationDefinition extends LineDefinitionEntryRelationDefinitionForSave {
    LineDefinitionEntryId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
