import { EntityForSave } from './base/entity-for-save';

export interface AgentRateForSave extends EntityForSave {
    ResourceId?: number;
    UnitId?: number;
    Rate?: number;
    CurrencyId?: string;
}

export interface AgentRate extends AgentRateForSave {
    AgentId?: number;
}
