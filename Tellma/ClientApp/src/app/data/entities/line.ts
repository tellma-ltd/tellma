// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { EntryForSave, Entry } from './entry';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';

export type PositiveLineState = 0 | 1 | 2 | 3 | 4;
export type NegativeLineState = -1 | -2 | -3 | -4;
export type LineState = PositiveLineState | NegativeLineState;

export interface LineForSave<TEntry = EntryForSave> extends EntityForSave {
    DefinitionId?: number;
    PostingDate?: string;
    TemplateLineId?: number;
    Multiplier?: number;
    Memo?: string;
    Boolean1?: boolean;
    Decimal1?: number;
    Text1?: string;
    Entries?: TEntry[];

    // Only for client side tracking of new lines
    _flags?: LineFlags;
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

// tslint:disable-next-line:no-empty-interface
export interface LineForQuery extends Line {
}

export interface LineFlags {
    isModified?: boolean;
    isHighlighted?: boolean; // Yellow highlighted line
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_LineForQuery(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'LineForQuery',
            titleSingular: () => trx.instant('Line'),
            titlePlural: () => trx.instant('Lines'),
            select: [],
            orderby: () => ['Id'],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => '',
            properties: {
                Id: { datatype: 'integral', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DocumentId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Line_Document')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Document: { datatype: 'entity', control: 'Document', label: () => trx.instant('Line_Document'), foreignKeyName: 'DocumentId' },
                IsSystem: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('IsSystem') },
                DefinitionId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Definition: { datatype: 'entity', control: 'LineDefinition', label: () => trx.instant('Definition'), foreignKeyName: 'DefinitionId' },
                PostingDate: { datatype: 'date', control: 'date', label: () => trx.instant('Line_PostingDate') },
                TemplateLineId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Line_TemplateLine')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                TemplateLine: { datatype: 'entity', control: 'LineForQuery', label: () => trx.instant('Line_TemplateLine'), foreignKeyName: 'TemplateLineId' },
                Multiplier: { datatype: 'decimal', control: 'number', label: () => trx.instant('Line_Multiplier'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                Memo: { datatype: 'string', control: 'text', label: () => trx.instant('Memo') },
                Boolean1: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('Line_Boolean1') },
                Decimal1: { datatype: 'decimal', control: 'number', label: () => trx.instant('Line_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Text1: { datatype: 'string', control: 'text', label: () => trx.instant('Line_Text1') },
                State: {
                    datatype: 'integral',
                    control: 'choice',
                    label: () => trx.instant('State'),
                    choices: [0, -1, 1, -2, 2, -3, 3, -4, 4],
                    format: (state: number) => {
                        if (state >= 0) {
                            return trx.instant('Line_State_' + state);
                        } else {
                            return trx.instant('Line_State_minus_' + (-state));
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
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
