// tslint:disable:variable-name
import { EntityWithKey } from '../entities/base/entity-with-key';

export interface GetEntityResponse<TEntity> {
  Result: TEntity;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
}
