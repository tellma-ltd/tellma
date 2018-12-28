import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class EntitiesResponse<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase> {
  Bag: { [key: string]: any; };
  Data: TDto[];
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
}

export class GetResponse<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase> extends EntitiesResponse<TDto> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
