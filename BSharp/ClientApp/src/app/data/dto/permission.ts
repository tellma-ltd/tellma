import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class PermissionForSave extends DtoForSaveKeyBase {
  ViewId: string;
  RoleId: number;
  Level: 'Read' | 'Update' | 'Create' | 'ReadAndCreate' | 'Sign';
  Criteria: string;
  Memo: string;
}

export class Permission extends PermissionForSave {
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

// Choice list (Also repeated in measurement units master template)
export const Permission_Level = {
  'Read': 'Permission_Read',
  'Update': 'Permission_Update',
  'Create': 'Permission_Create',
  'ReadCreate': 'Permission_ReadAndCreate',
  'Sign': 'Permission_Sign',
};

export class PermissionsForClient {
  [viewId: string]: ViewPermissionsForClient;
}

export class ViewPermissionsForClient {
  Read: boolean;
  Create: boolean;
  Update: boolean;
  Sign: boolean;
}
