import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class GetByIdResponse<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase> {
  Entity: TDto;
  CollectionName: string;
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
}
