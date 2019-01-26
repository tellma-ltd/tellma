import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';

export class RoleForSave<TPermission = PermissionForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Code: string;
    IsPublic: boolean;
    Permissions: TPermission[];
}

export class Role extends RoleForSave<Permission> {
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
