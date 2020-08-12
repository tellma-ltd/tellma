import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryCustodyDefinitionForSave extends EntityForSave {
    CustodyDefinitionId?: number;
}

export interface LineDefinitionEntryCustodyDefinition extends LineDefinitionEntryCustodyDefinitionForSave {
    LineDefinitionEntryId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
