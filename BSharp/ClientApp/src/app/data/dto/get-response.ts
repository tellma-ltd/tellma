import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class GetResponse<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
  Bag: { [key: string]: any; };
  Data: TDto[];
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
}
