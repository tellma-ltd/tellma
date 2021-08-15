// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';
import { TimeGranularity } from './base/metadata-types';
import { AdminSettingsForClient } from '../dto/admin-settings-for-client';

export interface IdentityServerClientForSave extends EntityForSave {
    Name?: string;
    Memo?: string;
    ClientId?: string;
    ClientSecret?: string;
}

export interface IdentityServerClient extends IdentityServerClientForSave {
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

let _settings: AdminSettingsForClient;
let _cache: EntityDescriptor;

export function metadata_IdentityServerClient(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.admin;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        _cache = {
            collection: 'IdentityServerClient',
            titleSingular: () => trx.instant('IdentityServerClient'),
            titlePlural: () => trx.instant('IdentityServerClients'),
            select: ['Name'],
            apiEndpoint: 'identity-server-clients',
            masterScreenUrl: 'identity-server-clients',
            orderby: () => ['Name'],
            inactiveFilter: null,
            format: (item: IdentityServerClientForSave) => item.Name,
            formatFromVals: (vals: any[]) => vals[0],
            isAdmin: true,
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') },
                Memo: { datatype: 'string', control: 'text', label: () => trx.instant('Memo') },
                ClientId: { datatype: 'string', control: 'text', label: () => trx.instant('IdentityServerClient_ClientId') },
                ClientSecret: { datatype: 'string', control: 'text', label: () => trx.instant('IdentityServerClient_ClientSecret') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };
    }

    return _cache;
}
