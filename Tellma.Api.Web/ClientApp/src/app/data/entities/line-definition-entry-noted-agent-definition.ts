import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryAgentDefinitionForSave extends EntityForSave {
    AgentDefinitionId?: number;
}

export interface LineDefinitionEntryAgentDefinition extends LineDefinitionEntryAgentDefinitionForSave {
    LineDefinitionEntryId?: number;
    SavedById?: number;
}
