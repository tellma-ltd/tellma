// tslint:disable:variable-name
import { Entity } from './entity';

export interface EntityWithKey extends Entity {
  Id?: string | number;
}
