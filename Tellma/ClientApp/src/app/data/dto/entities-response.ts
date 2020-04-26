import { EntityWithKey } from '../entities/base/entity-with-key';
import { Entity } from '../entities/base/entity';

export interface EntitiesResponse<TEntity extends Entity = EntityWithKey> {
  Extras: {
    [key: string]: any;
  };
  Result: TEntity[];
  CollectionName: string;
  RelatedEntities: {
    [key: string]: EntityWithKey[];
  };
  IsPartial: boolean;
  ServerTime: string;
}
