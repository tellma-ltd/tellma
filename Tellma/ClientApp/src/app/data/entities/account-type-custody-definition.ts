import { EntityForSave } from './base/entity-for-save';

export interface AccountTypeCustodyDefinitionForSave extends EntityForSave {
    CustodyDefinitionId?: number;
}

export interface AccountTypeCustodyDefinition extends AccountTypeCustodyDefinitionForSave {
    AccountTypeId?: number;
    SavedById?: number | string;
}
