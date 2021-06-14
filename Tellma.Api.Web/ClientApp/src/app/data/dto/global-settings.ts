// tslint:disable:variable-name
// tslint:disable:no-empty-interface
import { Entity } from '../entities/base/entity';

export interface GlobalSettingsForClient extends Entity {
    EmailEnabled: boolean;
    SmsEnabled: boolean;
    PushEnabled: boolean;
}
