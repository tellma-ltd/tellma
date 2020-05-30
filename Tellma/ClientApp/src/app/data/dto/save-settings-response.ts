// tslint:disable:variable-name
import { Settings } from '../entities/settings';
import { Versioned } from './versioned';
import { GetEntityResponse } from './get-entity-response';
import { SettingsForClient } from './settings-for-client';

export interface SaveSettingsResponse extends GetEntityResponse<Settings> {
    SettingsForClient: Versioned<SettingsForClient>;
}
