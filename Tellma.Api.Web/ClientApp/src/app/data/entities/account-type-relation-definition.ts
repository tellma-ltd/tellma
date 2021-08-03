import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeRelationDefinitionForSave extends EntityForSave {
    RelationDefinitionId?: number;
}

export interface AccountTypeRelationDefinition extends AccountTypeRelationDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
