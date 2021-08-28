// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface EntryForSave extends EntityForSave {
    Direction?: 1 | -1;
    AccountId?: number;
    CurrencyId?: string;
    AgentId?: number;
    ResourceId?: number;
    NotedAgentId?: number;
    CenterId?: number;
    EntryTypeId?: number;
    MonetaryValue?: number;
    Quantity?: number;
    UnitId?: number;
    Value?: number;
    RValue?: number;
    PValue?: number;
    Time1?: string;
    Duration?: number;
    DurationUnitId?: number;
    Time2?: string;
    ExternalReference?: string;
    ReferenceSourceId?: string;
    InternalReference?: string;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;
}

export interface Entry extends EntryForSave {
    LineId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
