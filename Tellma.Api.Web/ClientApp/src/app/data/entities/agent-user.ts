import { EntityForSave } from './base/entity-for-save';

export interface AgentUserForSave extends EntityForSave {
    UserId?: number;
}

export interface AgentUser extends AgentUserForSave {
    AgentId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
