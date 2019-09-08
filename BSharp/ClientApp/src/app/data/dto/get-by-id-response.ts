// tslint:disable:variable-name
import { EntityWithKey } from '../entities/base/entity-with-key';

export class GetByIdResponse<TEntity extends EntityWithKey = EntityWithKey> {
  Result: TEntity;
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
}
