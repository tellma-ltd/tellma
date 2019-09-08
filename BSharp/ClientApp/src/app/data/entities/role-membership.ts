// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';

export class RoleMembershipForSave extends EntityForSave {
    AgentId: number;
    RoleId: number;
    Memo: string;
}

export class RoleMembership extends RoleMembershipForSave {
    SavedById: number | string;
}
