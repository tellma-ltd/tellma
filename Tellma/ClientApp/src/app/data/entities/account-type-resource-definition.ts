import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeResourceDefinitionForSave extends EntityForSave {
    ResourceDefinitionId?: number;
}

export interface AccountTypeResourceDefinition extends AccountTypeResourceDefinitionForSave {
    AccountTypeId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
