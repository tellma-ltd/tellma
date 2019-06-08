import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class ProductCategoryForSave extends DtoForSaveKeyBase {
  Name: string;
  Name2: string;
  Name3: string;
  Code: string;
  ParentId: number;
}

export class ProductCategory extends ProductCategoryForSave {
  Level: number;
  ChildCount: number;
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}
