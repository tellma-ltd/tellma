import { EntityWithKey } from './entity-with-key';

export interface EntityForSave extends EntityWithKey {
  serverErrors?: { [key: string]: string[] };
}
