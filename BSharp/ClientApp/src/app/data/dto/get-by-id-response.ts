import { EntityWithKey } from '../entities/base/entity-with-key';
import { EntityForSave } from '../entities/base/entity-for-save';
import { Entity } from '../entities/base/entity';

export class GetByIdResponse<TEntity extends EntityWithKey = EntityWithKey> {
  Result: TEntity;
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
}
