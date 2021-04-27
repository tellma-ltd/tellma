import { EntityForSave } from './base/entity-for-save';
import {
    LineDefinitionEntryRelationDefinition,
    LineDefinitionEntryRelationDefinitionForSave
} from './line-definition-entry-noted-relation-definition';
import {
    LineDefinitionEntryNotedRelationDefinition,
    LineDefinitionEntryNotedRelationDefinitionForSave
} from './line-definition-entry-relation-definition';
import {
    LineDefinitionEntryResourceDefinitionForSave,
    LineDefinitionEntryResourceDefinition
} from './line-definition-entry-resource-definition';

export interface LineDefinitionEntryForSave<
    TRelationDef = LineDefinitionEntryRelationDefinitionForSave,
    TResourceDef = LineDefinitionEntryResourceDefinitionForSave,
    TNotedRelationDef = LineDefinitionEntryNotedRelationDefinitionForSave> extends EntityForSave {
    Direction?: 1 | -1;
    ParentAccountTypeId?: number;
    EntryTypeId?: number;

    RelationDefinitions?: TRelationDef[];
    ResourceDefinitions?: TResourceDef[];
    NotedRelationDefinitions?: TNotedRelationDef[];
}

export interface LineDefinitionEntry extends LineDefinitionEntryForSave<
    LineDefinitionEntryRelationDefinition,
    LineDefinitionEntryResourceDefinition,
    LineDefinitionEntryNotedRelationDefinition> {
    LineDefinitionId?: number;
    SavedById?: number;
}
