import { EntityForSave } from '../entities/base/entity-for-save';

export class PermissionForSave extends EntityForSave {
  ViewId: string;
  RoleId: number;
  Action: 'Read' | 'Update' | 'Delete' | 'IsActive' | 'ResendInvitationEmail';
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
  'Delete': 'Permission_Delete',
  'IsActive': 'Permission_IsActive',
  'ResendInvitationEmail': 'ResendInvitationEmail',
};

export class PermissionsForClient {
  [viewId: string]: ViewPermissionsForClient;
}

export class ViewPermissionsForClient {
  [action: string]: boolean;
}
