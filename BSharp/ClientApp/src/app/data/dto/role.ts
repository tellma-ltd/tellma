import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';
import { RoleMembershipForSave, RoleMembership } from './role-membership';
import { RequiredSignatureForSave, RequiredSignature } from './required-signature';

export class RoleForSave<TPermission = PermissionForSave, TRequiredSignature = RequiredSignatureForSave,
    TRoleMembership = RoleMembershipForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Code: string;
    IsPublic: boolean;
    Permissions: TPermission[];
    Signatures: TRequiredSignature[];
    Members: TRoleMembership[];
}

export class Role extends RoleForSave<Permission, RequiredSignature, RoleMembership> {
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
