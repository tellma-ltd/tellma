// tslint:disable:max-line-length
// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute } from '@angular/router';

export interface InboxRecord extends EntityWithKey {
    DocumentId?: number;
    Comment?: string;
    CreatedAt?: string;
    CreatedById?: number;
    OpenedAt?: string;
}

const _select = [];
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_InboxRecord(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
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
            collection: 'InboxRecord',
            titleSingular: () => trx.instant('InboxRecord'),
            titlePlural: () => trx.instant('Inbox'),
            select: _select,
            apiEndpoint: 'inbox',
            masterScreenUrl: 'inbox',
            navigateToDetailsSelect: ['Document.DefinitionId'],
            navigateToDetails: (entity: InboxRecord, router: Router, stateKey: string) => {
                const id = entity.Id;
                const definitionId = ws.Document[id].DefinitionId;
                const extras = { state_key: stateKey };
                router.navigate(['app', wss.ws.tenantId + '', 'documents', definitionId, id, extras]);
            },
            orderby: () => ['CreatedAt desc'],
            inactiveFilter: null,
            format: (__: EntityWithKey) => '',
            properties: {
                Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DocumentId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Assignment_Document')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Document: { datatype: 'entity', control: 'Document', label: () => trx.instant('Assignment_Document'), foreignKeyName: 'DocumentId' },
                Comment: { datatype: 'string', control: 'text', label: () => trx.instant('Document_Comment') },

                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('Document_AssignedAt') },
                CreatedById: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Document_AssignedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('Document_AssignedBy'), foreignKeyName: 'CreatedById' },
                OpenedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('Document_OpenedAt') }
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
