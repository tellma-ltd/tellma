// tslint:disable:variable-name
import { EntityForSave } from '../entities/base/entity-for-save';
import { Action } from '../views';

export interface PermissionForSave extends EntityForSave {
  View?: string;
  Action?: Action;
  Criteria?: string;
  Mask?: string;
  Memo?: string;
}

export interface Permission extends PermissionForSave {
  RoleId?: number;
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}
