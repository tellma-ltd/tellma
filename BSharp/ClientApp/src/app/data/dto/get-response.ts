import { EntityWithKey } from '../entities/base/entity-with-key';
import { Entity } from '../entities/base/entity';

export class EntitiesResponse<TDto extends Entity = EntityWithKey> {
  Bag: { [key: string]: any; };
  Result: TDto[];
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
}

export class GetResponse extends EntitiesResponse {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
