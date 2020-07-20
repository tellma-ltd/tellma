import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeNotedContractDefinitionForSave extends EntityForSave {
    NotedContractDefinitionId?: number;
}

export interface AccountTypeNotedContractDefinition extends AccountTypeNotedContractDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
