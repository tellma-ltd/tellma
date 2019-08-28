import { DtoBase } from './dto-base';

export class GlobalSettingsForSave extends DtoBase {

}

export class GlobalSettings extends GlobalSettingsForSave {
    SettingsVersion: string;
}

export class GlobalSettingsForClient extends DtoBase {
}
