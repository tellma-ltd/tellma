// tslint:disable:max-line-length
// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute } from '@angular/router';

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

export function metadata_OutboxRecord(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
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
            masterScreenUrl: 'outbox',
            navigateToDetailsSelect: ['DocumentId', 'Document.DefinitionId'],
            navigateToDetails: (outboxRecord: OutboxRecord, router: Router) => {
                const docId = outboxRecord.DocumentId;
                const definitionId = ws.Document[docId].DefinitionId;
                entityDesc.navigateToDetailsFromVals([docId, definitionId], router);
            },
            navigateToDetailsFromVals: (vals: any[], router: Router) => {
                const [docId, definitionId] = vals;
                const extras = { state_key: 'from_outbox' }; // fake state key to hide forward and backward navigation in details screen
                router.navigate(['app', wss.ws.tenantId + '', 'documents', definitionId, docId, extras]);
            },
            orderby: () => ['CreatedAt desc'],
            inactiveFilter: null,
            format: (_: EntityWithKey) => '',
            formatFromVals: (_: any[]) => '',
            properties: {
                Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DocumentId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Assignment_Document')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Document: { datatype: 'entity', control: 'Document', label: () => trx.instant('Assignment_Document'), foreignKeyName: 'DocumentId' },
                Comment: { datatype: 'string', control: 'text', label: () => trx.instant('Document_Comment') },

                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                AssigneeId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Document_Assignee')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Assignee: { datatype: 'entity', control: 'User', label: () => trx.instant('Document_Assignee'), foreignKeyName: 'AssigneeId' },
                OpenedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('Document_OpenedAt') }
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
