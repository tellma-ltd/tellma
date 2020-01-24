// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';

export interface EntryForSave extends EntityWithKey {
    EntryNumber?: number;
    Direction?: 1 | -1;
    AccountId?: number;
    CurrencyId?: string;
    AgentId?: number;
    ResourceId?: number;
    ResponsibilityCenterId?: number;
    EntryTypeId?: number;
    DueDate?: string;
    MonetaryValue?: number;
    Count?: number;
    Mass?: number;
    Volume?: number;
    Time?: number;
    Value?: number;
    Time1?: string;
    Time2?: string;
}

export interface Entry extends EntryForSave {
    LineId?: number;
    ContractType?: string;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
