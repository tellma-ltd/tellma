// tslint:disable:variable-name
export interface ImportArguments {
  mode?: ImportMode;
  key?: string;
}

export type ImportMode = 'Insert' | 'Update' | 'Merge';

export const ImportArguments_Mode = {
  Insert: 'Mode_Insert',
  Update: 'Mode_Update',
  Merge: 'Mode_Merge'
};
