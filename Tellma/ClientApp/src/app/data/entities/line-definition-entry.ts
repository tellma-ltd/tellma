import { EntityForSave } from './base/entity-for-save';
import {
    LineDefinitionEntryCustodyDefinitionForSave,
    LineDefinitionEntryCustodyDefinition
} from './line-definition-entry-custody-definition';
import {
    LineDefinitionEntryNotedRelationDefinitionForSave,
    LineDefinitionEntryNotedRelationDefinition
} from './line-definition-entry-noted-relation-definition';
import {
    LineDefinitionEntryResourceDefinitionForSave,
    LineDefinitionEntryResourceDefinition
} from './line-definition-entry-resource-definition';

export interface LineDefinitionEntryForSave<
    TCustodyDef = LineDefinitionEntryCustodyDefinitionForSave,
    TNotedRelationDef = LineDefinitionEntryNotedRelationDefinitionForSave,
    TResourceDef = LineDefinitionEntryResourceDefinitionForSave> extends EntityForSave {
    Direction?: 1 | -1;
    AccountTypeId?: number;
    EntryTypeId?: number;
    CustodyDefinitions?: TCustodyDef[];
    NotedRelationDefinitions?: TNotedRelationDef[];
    ResourceDefinitions?: TResourceDef[];
}

export interface LineDefinitionEntry extends LineDefinitionEntryForSave<
    LineDefinitionEntryCustodyDefinition,
    LineDefinitionEntryNotedRelationDefinition,
    LineDefinitionEntryResourceDefinition> {
    LineDefinitionId?: number;
    SavedById?: number;
}
