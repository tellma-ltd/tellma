// tslint:disable:variable-name
import { EntityWithKey } from '../entities/base/entity-with-key';
import { Entity } from '../entities/base/entity';

export class EntitiesResponse<TEntity extends Entity = EntityWithKey> {
  Extras: { [key: string]: any; };
  Result: TEntity[];
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
  IsPartial: boolean;
  ServerTime: string;
}

export class GetResponse<TEntity extends Entity = EntityWithKey> extends EntitiesResponse<TEntity> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
