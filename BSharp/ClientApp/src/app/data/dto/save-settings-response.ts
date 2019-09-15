// tslint:disable:variable-name
import { Settings } from '../entities/settings';
import { DataWithVersion } from './data-with-version';
import { GetEntityResponse } from './get-entity-response';
import { SettingsForClient } from './settings-for-client';

export class SaveSettingsResponse extends GetEntityResponse<Settings> {
    SettingsForClient: DataWithVersion<SettingsForClient>;
}
