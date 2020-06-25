import { EntityForSave } from './base/entity-for-save';

export interface ContractUserForSave extends EntityForSave {
    UserId?: number;
}

export interface ContractUser extends ContractUserForSave {
    ContractId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
