import { DtoBase } from './dto-base';

export abstract class DtoKeyBase extends DtoBase {
  Id: string | number = null;
}
