// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';

export interface RoleMembershipForSave extends EntityForSave {
    AgentId?: number;
    RoleId?: number;
    Memo?: string;
}

export interface RoleMembership extends RoleMembershipForSave {
    SavedById?: number | string;
}
