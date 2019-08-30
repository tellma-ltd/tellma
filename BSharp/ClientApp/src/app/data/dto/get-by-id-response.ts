import { EntityWithKey } from '../entities/base/entity-with-key';
import { EntityForSave } from '../entities/base/entity-for-save';
import { Entity } from '../entities/base/entity';

export class GetByIdResponse<TDto extends EntityWithKey = EntityWithKey> {
  Result: TDto;
  CollectionName: string;
  RelatedEntities: { [key: string]: EntityWithKey[]; };
}
