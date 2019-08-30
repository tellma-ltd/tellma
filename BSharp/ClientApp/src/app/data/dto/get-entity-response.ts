import { EntityWithKey } from '../entities/base/entity-with-key';

export class GetEntityResponse<TDto> {
  Result: TDto;
  Entities: { [key: string]: EntityWithKey[]; };
}
