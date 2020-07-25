import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeCustodianDefinitionForSave extends EntityForSave {
    CustodianDefinitionId?: number;
}

export interface AccountTypeCustodianDefinition extends AccountTypeCustodianDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
