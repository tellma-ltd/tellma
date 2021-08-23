// tslint:disable:variable-name
import { Entity } from './base/entity';

// tslint:disable-next-line:no-empty-interface
export interface AdminSettingsForSave extends Entity {

}

export interface AdminSettings extends AdminSettingsForSave {
    SettingsVersion: string;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
