import { DtoKeyBase } from './dto-key-base';

export class EntitiesResponse<TDto extends DtoKeyBase = DtoKeyBase> {
  Bag: { [key: string]: any; };
  Data: TDto[];
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoKeyBase[]; };
}

export class GetResponse<TDto extends DtoKeyBase = DtoKeyBase> extends EntitiesResponse<TDto> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
