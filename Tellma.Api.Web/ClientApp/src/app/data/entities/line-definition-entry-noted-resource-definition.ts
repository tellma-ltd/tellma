import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryNotedResourceDefinitionForSave extends EntityForSave {
    NotedResourceDefinitionId?: number;
}

export interface LineDefinitionEntryNotedResourceDefinition extends LineDefinitionEntryNotedResourceDefinitionForSave {
    LineDefinitionEntryId?: number;
    SavedById?: number;
}
