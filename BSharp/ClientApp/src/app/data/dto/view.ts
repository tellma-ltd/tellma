import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';

export class ViewForSave<TPermission = PermissionForSave> extends DtoForSaveKeyBase {

    Permissions: TPermission[];
}

export class View extends ViewForSave<Permission> {
    Name: string;
    Name2: string;
    Code: string;
    IsActive: boolean;
    AllowedPermissionLevels: ('Read' | 'Update' | 'Create' | 'ReadAndCreate' | 'Sign')[];
}
