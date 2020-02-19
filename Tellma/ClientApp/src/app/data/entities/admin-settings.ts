// tslint:disable:variable-name
import { Entity } from './base/entity';

export class AdminSettingsForSave extends Entity {

}

export class AdminSettings extends AdminSettingsForSave {
    SettingsVersion: string;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
