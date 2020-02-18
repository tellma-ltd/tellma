// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { Action } from '../views';

export interface AdminPermissionForSave extends EntityForSave {
  View?: string;
  Action?: Action;
  Criteria?: string;
  Memo?: string;
}

export interface AdminPermission extends AdminPermissionForSave {
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}
