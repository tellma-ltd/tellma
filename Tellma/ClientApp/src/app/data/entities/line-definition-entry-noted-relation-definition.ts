import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryNotedRelationDefinitionForSave extends EntityForSave {
    NotedRelationDefinitionId?: number;
}

export interface LineDefinitionEntryNotedRelationDefinition extends LineDefinitionEntryNotedRelationDefinitionForSave {
    LineDefinitionEntryId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
