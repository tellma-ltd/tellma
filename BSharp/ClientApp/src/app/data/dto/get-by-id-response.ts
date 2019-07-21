import { DtoKeyBase } from './dto-key-base';
import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { DtoBase } from './dto-base';

export class GetByIdResponse<TDto extends DtoKeyBase = DtoKeyBase> {
  Result: TDto;
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoKeyBase[]; };
}
