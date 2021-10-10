import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeNotedResourceDefinitionForSave extends EntityForSave {
    NotedResourceDefinitionId?: number;
}

export interface AccountTypeNotedResourceDefinition extends AccountTypeNotedResourceDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
