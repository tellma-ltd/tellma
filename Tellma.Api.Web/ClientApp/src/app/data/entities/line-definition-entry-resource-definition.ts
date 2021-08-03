import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryResourceDefinitionForSave extends EntityForSave {
    ResourceDefinitionId?: number;
}

export interface LineDefinitionEntryResourceDefinition extends LineDefinitionEntryResourceDefinitionForSave {
    LineDefinitionEntryId?: number;
    SavedById?: number;
}
