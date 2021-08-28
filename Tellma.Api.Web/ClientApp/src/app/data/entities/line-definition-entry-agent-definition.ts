import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryNotedAgentDefinitionForSave extends EntityForSave {
    NotedAgentDefinitionId?: number;
}

export interface LineDefinitionEntryNotedAgentDefinition extends LineDefinitionEntryNotedAgentDefinitionForSave {
    LineDefinitionEntryId?: number;
    SavedById?: number;
}
