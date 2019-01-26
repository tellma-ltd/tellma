import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class RoleMembershipForSave extends DtoForSaveKeyBase {
    UserId: number;
    RoleId: number;
    Memo: string;
}

export class RoleMembership extends RoleMembershipForSave {
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
