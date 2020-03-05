// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface EntryForSave extends EntityForSave {
    Direction?: 1 | -1;
    AccountId?: number;
    CurrencyId?: string;
    AgentId?: number;
    ResourceId?: number;
    ResponsibilityCenterId?: number;
    EntryTypeId?: number;
    DueDate?: string;
    MonetaryValue?: number;
    Quantity?: number;
    UnitId?: number;
    Value?: number;
    Time1?: string;
    Time2?: string;
    ExternalReference?: string;
    AdditionalReference?: string;
    NotedAgentId?: number;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;
}

export interface Entry extends EntryForSave {
    LineId?: number;
    ContractType?: string;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
