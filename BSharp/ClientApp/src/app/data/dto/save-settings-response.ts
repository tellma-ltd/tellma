import { GetByIdResponse } from './get-by-id-response';
import { Settings, SettingsForClient } from './settings';
import { DataWithVersion } from './data-with-version';

export class SaveSettingsResponse extends GetByIdResponse<Settings> {
    SettingsForClient: DataWithVersion<SettingsForClient>;
}
