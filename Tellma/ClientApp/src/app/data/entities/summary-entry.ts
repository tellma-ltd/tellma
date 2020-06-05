// tslint:disable:variable-name
// tslint:disable:max-line-length
import { Entity } from './base/entity';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export interface SummaryEntry extends Entity {
    AccountId?: number;
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_SummaryEntry(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'SummaryEntry',
            titleSingular: () => trx.instant('SummaryEntry'),
            titlePlural: () => trx.instant('SummaryEntries'),
            select: [],
            apiEndpoint: 'summary-entries',
            parameters: [
                { key: 'FromDate', isRequired: true, desc: { control: 'date', label: () => trx.instant('FromDate') } },
                { key: 'ToDate', isRequired: true, desc: { control: 'date', label: () => trx.instant('ToDate') } },
            ],
            screenUrl: 'summary-entries',
            orderby: () => ['AccountId'],
            format: (__: Entity) => '',
            properties: {
                AccountId: { control: 'number', label: () => `${trx.instant('Entry_Account')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Account: { control: 'navigation', label: () => trx.instant('Entry_Account'), type: 'Account', foreignKeyName: 'AccountId' },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
