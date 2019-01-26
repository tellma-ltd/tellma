import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class CustodyForSave extends DtoForSaveKeyBase {
  Name: string;
  Name2: string;
  Code: string;
  Address: string;
  BirthDateTime: string;
}

export class Custody extends CustodyForSave {
  CustodyType: string;
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

