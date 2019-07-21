import { Settings, SettingsForClient } from './settings';
import { DataWithVersion } from './data-with-version';
import { GetEntityResponse } from './get-entity-response';

export class SaveSettingsResponse extends GetEntityResponse<Settings> {
    SettingsForClient: DataWithVersion<SettingsForClient>;
}
