import { DtoBase } from './dto-base';
import { Culture } from './culture';

export class GlobalSettingsForSave extends DtoBase {

}

export class GlobalSettings extends GlobalSettingsForSave {
    SettingsVersion: string;
}

export class GlobalSettingsForClient extends DtoBase {
    ActiveCultures: { [key: string]: Culture};
}
