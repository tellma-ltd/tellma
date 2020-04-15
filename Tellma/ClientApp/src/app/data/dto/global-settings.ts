// tslint:disable:variable-name
// tslint:disable:no-empty-interface
import { Entity } from '../entities/base/entity';

export interface GlobalSettingsForSave extends Entity {

}

export interface GlobalSettings extends GlobalSettingsForSave {
    SettingsVersion: string;
}

export interface GlobalSettingsForClient extends Entity {
}
