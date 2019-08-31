import { EntityForSave } from './base/entity-for-save';

export class RoleMembershipForSave extends EntityForSave {
    AgentId: number;
    RoleId: number;
    Memo: string;
}

export class RoleMembership extends RoleMembershipForSave {
    // CreatedAt: string;
    // CreatedById: number | string;
    // ModifiedAt: string;
    // ModifiedById: number | string;
    SavedById: number | string;
}
