import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeAgentDefinitionForSave extends EntityForSave {
    AgentDefinitionId?: number;
}

export interface AccountTypeAgentDefinition extends AccountTypeAgentDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
