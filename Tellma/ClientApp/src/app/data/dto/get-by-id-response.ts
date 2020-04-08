// tslint:disable:variable-name
import { EntityWithKey } from '../entities/base/entity-with-key';

export class GetByIdResponse<TEntity extends EntityWithKey = EntityWithKey> {
  Extras: { [key: string]: any; };
  Result: TEntity;
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
  ServerTime: string;
}
