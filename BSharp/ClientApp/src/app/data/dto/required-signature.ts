import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class RequiredSignatureForSave extends DtoForSaveKeyBase {
  ViewId: string;
  RoleId: number;
  Criteria: string;
  Memo: string;
}

export class RequiredSignature extends RequiredSignatureForSave {
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
  'ReadCreate': 'Permission_ReadAndCreate'
};
