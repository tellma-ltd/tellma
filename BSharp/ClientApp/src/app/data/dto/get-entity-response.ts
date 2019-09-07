import { EntityWithKey } from '../entities/base/entity-with-key';

export class GetEntityResponse<TEntity> {
  Result: TEntity;
  Entities: { [key: string]: EntityWithKey[]; };
}
