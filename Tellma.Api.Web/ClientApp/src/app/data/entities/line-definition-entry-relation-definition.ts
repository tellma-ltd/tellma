import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryNotedRelationDefinitionForSave extends EntityForSave {
    NotedRelationDefinitionId?: number;
}

export interface LineDefinitionEntryNotedRelationDefinition extends LineDefinitionEntryNotedRelationDefinitionForSave {
    LineDefinitionEntryId?: number;
    SavedById?: number;
}
