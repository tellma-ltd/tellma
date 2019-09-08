// tslint:disable:variable-name
import { Entity } from '../entities/base/entity';

export class GlobalSettingsForSave extends Entity {

}

export class GlobalSettings extends GlobalSettingsForSave {
    SettingsVersion: string;
}

export class GlobalSettingsForClient extends Entity {
}
