import { EntityForSave } from './base/entity-for-save';
import {
    LineDefinitionEntryCustodyDefinitionForSave,
    LineDefinitionEntryCustodyDefinition
} from './line-definition-entry-custody-definition';
import {
    LineDefinitionEntryResourceDefinitionForSave,
    LineDefinitionEntryResourceDefinition
} from './line-definition-entry-resource-definition';

export interface LineDefinitionEntryForSave<
    TCustodyDef = LineDefinitionEntryCustodyDefinitionForSave,
    TResourceDef = LineDefinitionEntryResourceDefinitionForSave> extends EntityForSave {
    Direction?: 1 | -1;
    ParentAccountTypeId?: number;
    EntryTypeId?: number;
    CustodyDefinitions?: TCustodyDef[];
    ResourceDefinitions?: TResourceDef[];
}

export interface LineDefinitionEntry extends LineDefinitionEntryForSave<
    LineDefinitionEntryCustodyDefinition,
    LineDefinitionEntryResourceDefinition> {
    LineDefinitionId?: number;
    SavedById?: number;
}
