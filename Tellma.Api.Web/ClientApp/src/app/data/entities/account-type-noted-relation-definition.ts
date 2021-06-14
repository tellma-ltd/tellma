import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeNotedRelationDefinitionForSave extends EntityForSave {
    NotedRelationDefinitionId?: number;
}

export interface AccountTypeNotedRelationDefinition extends AccountTypeNotedRelationDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
