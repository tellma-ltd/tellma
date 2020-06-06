import { EntityForSave } from '../entities/base/entity-for-save';

export interface AccountTypeContractDefinitionForSave extends EntityForSave {
    ContractDefinitionId?: number;
}

export interface AccountTypeContractDefinition extends AccountTypeContractDefinitionForSave {
    AccountTypeId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
