import { DtoForSaveKeyBase } from "./dto-for-save-key-base";

export class GetByIdResponse<TDto extends DtoForSaveKeyBase> {
  Entity: TDto;
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
}
