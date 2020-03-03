// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { EntryForSave, Entry } from './entry';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';

export type LineState = 0 | -1 | 1 | -2 | 2 | -3 | 3 | -4 | 4;

export interface LineForSave<TEntry = EntryForSave> extends EntityForSave {
    DefinitionId?: string;
    CurrencyId?: string;
    AgentId?: number;
    ResourceId?: number;
    MonetaryValue?: number;
    Memo?: string;
    Entries?: TEntry[];
}

export interface Line extends LineForSave<Entry> {
    DocumentId?: number;
    State?: LineState;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
    SortKey?: number;
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Line(wss: WorkspaceService, trx: TranslateService, _: string): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'Line',
            titleSingular: () => trx.instant('Line'),
            titlePlural: () => trx.instant('Lines'),
            select: [],
            orderby: ['Id'],
            format: (item: EntityWithKey) => '',
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DocumentId: { control: 'number', label: () => `${trx.instant('Line_Document')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Document: { control: 'navigation', label: () => trx.instant('Line_Document'), type: 'Document', foreignKeyName: 'DocumentId' },
                DefinitionId: { control: 'text', label: () => trx.instant('Line_Definition') },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Line_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Line_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                AgentId: { control: 'number', label: () => `${trx.instant('Line_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent: { control: 'navigation', label: () => trx.instant('Line_Agent'), type: 'Agent', foreignKeyName: 'AgentId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Line_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Line_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                MonetaryValue: { control: 'number', label: () => trx.instant('Line_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                Memo: { control: 'text', label: () => trx.instant('Memo') },
                State: {
                    control: 'state',
                    label: () => trx.instant('State'),
                    choices: [0, -1, 1, -2, 2, -3, 3, -4, 4],
                    format: (c: number) => {
                        switch (c) {
                            case 0: return trx.instant('Document_State_Draft');
                            case -1: return trx.instant('Document_State_Void');
                            case 1: return trx.instant('Document_State_Requested');
                            case -2: return trx.instant('Document_State_Rejected');
                            case 2: return trx.instant('Document_State_Authorized');
                            case -3: return trx.instant('Document_State_Failed');
                            case 3: return trx.instant('Document_State_Completed');
                            case -4: return trx.instant('Document_State_Invalid');
                            case 4: return trx.instant('Document_State_Reviewed');
                            default: return null;
                        }
                    },
                    color: (c: number) => {
                        switch (c) {
                            case 0: return '#6c757d';
                            case -1: return '#dc3545';
                            case 1: return '#28a745';
                            case -2: return '#dc3545';
                            case 2: return '#28a745';
                            case -3: return '#dc3545';
                            case 3: return '#28a745';
                            case -4: return '#dc3545';
                            case 4: return '#28a745';
                            default: return null;
                        }
                    }
                },

                // Audit
                CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
