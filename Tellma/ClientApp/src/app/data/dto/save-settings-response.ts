// tslint:disable:variable-name
import { Versioned } from './versioned';
import { GetEntityResponse } from './get-entity-response';
import { SettingsForClient } from './settings-for-client';

export interface SaveSettingsResponse<TSettings> extends GetEntityResponse<TSettings> {
    SettingsForClient: Versioned<SettingsForClient>;
}
