import { Entity } from './entity';

export interface SettingsBase extends Entity {
  serverErrors?: { [key: string]: string[] };
}
