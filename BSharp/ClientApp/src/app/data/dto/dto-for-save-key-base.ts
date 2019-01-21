import { DtoKeyBase } from './dto-key-base';

export abstract class DtoForSaveKeyBase extends DtoKeyBase {
  EntityState: 'Inserted' | 'Updated' | 'Deleted' = 'Inserted';
}
