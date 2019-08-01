import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';

export class ViewForSave<TPermission = PermissionForSave> extends DtoForSaveKeyBase {

    Permissions: TPermission[];
}

export class View extends ViewForSave<Permission> {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    IsActive: boolean;
    Actions: ViewAction[];
    // AllowedPermissionLevels: ('Read' | 'Update' | 'Create' | 'ReadCreate' | 'Sign')[];
}

export class ViewAction extends DtoForSaveKeyBase {
    ViewId: string;
    Action: string;
    SupportsCriteria: boolean;
    SupportsMask: boolean;
}

export function Views_DoNotApplyPermissions(stale: View, fresh: View): View {
    // Set all props except for Permissions
    // TODO
    // Object.keys(stale).concat(Object.keys(fresh))
    //     .filter(p => ['Permissions'].indexOf(p) < 0)
    //     .forEach(p => stale[p] = fresh[p]);
    // return stale;
    return fresh;
}
