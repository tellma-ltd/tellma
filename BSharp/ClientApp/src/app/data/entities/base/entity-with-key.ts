import { Entity } from './entity';

export abstract class EntityWithKey extends Entity {
  Id: string | number = null;
}
