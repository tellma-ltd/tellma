import { DtoKeyBase } from './dto-key-base';

export class GetEntityResponse<TDto> {
  Result: TDto;
  Entities: { [key: string]: DtoKeyBase[]; };
}
