import { DtoForSaveKeyBase } from "./dto-for-save-key-base";

export class GetResponse<TDto extends DtoForSaveKeyBase> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
  Bag: { [key: string]: any; };
  RelatedEntities: { [key: string]: DtoForSaveKeyBase[]; };
  Data: TDto[];
}
