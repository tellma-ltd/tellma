import { DtoKeyBase } from './dto-key-base';
import { DtoBase } from './dto-base';

export class EntitiesResponse<TDto extends DtoBase = DtoKeyBase> {
  Bag: { [key: string]: any; };
  Result: TDto[];
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoKeyBase[]; };
}

export class GetResponse extends EntitiesResponse {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
