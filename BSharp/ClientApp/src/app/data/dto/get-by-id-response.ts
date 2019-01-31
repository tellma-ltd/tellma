import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { DtoBase } from './dto-base';

export class GetByIdResponse<TDto extends DtoBase = DtoForSaveKeyBase> {
  Entity: TDto;
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
}
