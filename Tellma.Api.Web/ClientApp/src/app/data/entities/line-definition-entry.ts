import { EntityForSave } from './base/entity-for-save';
import {
    LineDefinitionEntryAgentDefinition,
    LineDefinitionEntryAgentDefinitionForSave
} from './line-definition-entry-agent-definition';
import {
    LineDefinitionEntryResourceDefinitionForSave,
    LineDefinitionEntryResourceDefinition
} from './line-definition-entry-resource-definition';
import {
    LineDefinitionEntryNotedAgentDefinition,
    LineDefinitionEntryNotedAgentDefinitionForSave
} from './line-definition-entry-noted-agent-definition';
import {
    LineDefinitionEntryNotedResourceDefinition,
    LineDefinitionEntryNotedResourceDefinitionForSave
} from './line-definition-entry-noted-resource-definition';

export interface LineDefinitionEntryForSave<
    TAgentDef = LineDefinitionEntryAgentDefinitionForSave,
    TResourceDef = LineDefinitionEntryResourceDefinitionForSave,
    TNotedAgentDef = LineDefinitionEntryNotedAgentDefinitionForSave,
    TNotedResourceDef = LineDefinitionEntryNotedResourceDefinitionForSave> extends EntityForSave {
    Direction?: 1 | -1;
    ParentAccountTypeId?: number;
    EntryTypeId?: number;

    AgentDefinitions?: TAgentDef[];
    ResourceDefinitions?: TResourceDef[];
    NotedAgentDefinitions?: TNotedAgentDef[];
    NotedResourceDefinitions?: TNotedResourceDef[];
}

export interface LineDefinitionEntry extends LineDefinitionEntryForSave<
    LineDefinitionEntryAgentDefinition,
    LineDefinitionEntryResourceDefinition,
    LineDefinitionEntryNotedAgentDefinition,
    LineDefinitionEntryNotedResourceDefinition> {
    LineDefinitionId?: number;
    SavedById?: number;
}
