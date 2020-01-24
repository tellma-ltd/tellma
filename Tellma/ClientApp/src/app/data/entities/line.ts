// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { EntryForSave, Entry } from './entry';

export type LineState = 0 | -1 | 1 | -2 | 2 | -3 | 3 | -4 | 4;

export interface LineForSave<TEntry = EntryForSave> extends EntityWithKey {
    DefinitionId?: string;
    CurrencyId?: string;
    AgentId?: number;
    ResourceId?: number;
    Amount?: number;
    Memo?: string;
    ExternalReference?: string;
    AdditionalReference?: string;
    NotedAgentId?: number;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;
    Entries?: TEntry[];
}

export interface Line extends LineForSave<Entry> {
    DocumentId?: number;
    State?: LineState;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
    SortKey?: number;
}
