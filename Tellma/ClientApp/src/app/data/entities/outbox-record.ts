// tslint:disable:max-line-length
// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';

export interface OutboxRecord extends EntityWithKey {
    DocumentId?: number;
    Comment?: string;
    CreatedAt?: string;
    AssigneeId?: number;
    OpenedAt?: string;
}

const _select = [];
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_OutboxRecord(wss: WorkspaceService, trx: TranslateService, _: string): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;

        // clear the cache
        _cache = null;
    }

    if (!_cache) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'OutboxRecord',
            titleSingular: () => trx.instant('OutboxRecord'),
            titlePlural: () => trx.instant('Outbox'),
            select: _select,
            apiEndpoint: 'outbox',
            screenUrl: 'outbox',
            orderby: () => ['CreatedAt desc'],
            format: (__: EntityWithKey) => '',
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DocumentId: { control: 'number', label: () => `${trx.instant('Assignment_Document')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Document: { control: 'navigation', label: () => trx.instant('Assignment_Document'), type: 'Document', foreignKeyName: 'DocumentId' },
                Comment: { control: 'text', label: () => trx.instant('Document_Comment') },
                CreatedAt: { control: 'datetime', label: () => trx.instant('Document_AssignedAt') },
                AssigneeId: { control: 'number', label: () => `${trx.instant('Document_Assignee')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Assignee: { control: 'navigation', label: () => trx.instant('Document_Assignee'), type: 'User', foreignKeyName: 'AssigneeId' },
                OpenedAt: { control: 'datetime', label: () => trx.instant('Document_OpenedAt') }
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
