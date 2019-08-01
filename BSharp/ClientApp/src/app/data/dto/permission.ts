import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class PermissionForSave extends DtoForSaveKeyBase {
  ViewId: string;
  RoleId: number;
  Action: 'Read' | 'Update' | 'IsActive' | 'ResendInvitationEmail';
  Criteria: string;
  Mask: string;
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
  'IsActive': 'Permission_IsActive',
  'ResendInvitationEmail': 'ResendInvitationEmail',
};

export class PermissionsForClient {
  [viewId: string]: ViewPermissionsForClient;
}

export class ViewPermissionsForClient {
  [action: string]: boolean;
}
