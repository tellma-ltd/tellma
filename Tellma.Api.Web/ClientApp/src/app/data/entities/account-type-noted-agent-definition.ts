import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeNotedAgentDefinitionForSave extends EntityForSave {
    NotedAgentDefinitionId?: number;
}

export interface AccountTypeNotedAgentDefinition extends AccountTypeNotedAgentDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
