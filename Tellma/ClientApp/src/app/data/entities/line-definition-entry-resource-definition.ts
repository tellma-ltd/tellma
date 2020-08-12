import { EntityForSave } from './base/entity-for-save';

export interface LineDefinitionEntryResourceDefinitionForSave extends EntityForSave {
    ResourceDefinitionId?: number;
}

export interface LineDefinitionEntryResourceDefinition extends LineDefinitionEntryResourceDefinitionForSave {
    LineDefinitionEntryId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
