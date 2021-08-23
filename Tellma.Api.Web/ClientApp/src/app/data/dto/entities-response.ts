import { EntityWithKey } from '../entities/base/entity-with-key';
import { Entity } from '../entities/base/entity';
import { Collection } from '../entities/base/metadata';

export interface EntitiesResponse<TEntity extends Entity = EntityWithKey> {
  Extras: {
    [key: string]: any;
  };
  Result: TEntity[];
  CollectionName: Collection;
  RelatedEntities: {
    [key: string]: EntityWithKey[];
  };
  ServerTime: string;
}
