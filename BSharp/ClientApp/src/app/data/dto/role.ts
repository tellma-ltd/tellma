import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';
import { RoleMembershipForSave, RoleMembership } from './role-membership';

export class RoleForSave<TPermission = PermissionForSave, TRoleMembership = RoleMembershipForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Code: string;
    IsPublic: boolean;
    Permissions: TPermission[];
    Members: TRoleMembership[];
}

export class Role extends RoleForSave<Permission, RoleMembership> {
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

export function Roles_DoNotApplyPermissions(stale: Role, fresh: Role): Role {
    // Set all props except for Permissions
    Object.keys(stale).concat(Object.keys(fresh))
        .filter(p => ['Permissions'].indexOf(p) < 0)
        .forEach(p => stale[p] = fresh[p]);
    return stale;
}
