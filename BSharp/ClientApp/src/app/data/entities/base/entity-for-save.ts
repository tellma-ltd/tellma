import { EntityWithKey } from './entity-with-key';

export abstract class EntityForSave extends EntityWithKey {
  serverErrors?: { [key: string]: string[] };
}
