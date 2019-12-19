// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { LineForSave, Line, LineState } from './line';
import { DocumentSignature } from './document-signature';

export type DocumentState = LineState | 5;

export class DocumentForSave<TLine = LineForSave> extends EntityWithKey {
    OperatingSegmentId: number;
    DocumentDate: string;
    Memo: string;
    MemoIsCommon: boolean;
    Lines: TLine[];
}

export class Document extends DocumentForSave<Line> {
    DefinitionId: string;
    SerialNumber: number;
    State: DocumentState;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
    Signatures: DocumentSignature[];
}

const _select = ['SerialNumber'];
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: string]: EntityDescriptor } = {};
let _definitionIds: string[];

export function metadata_Document(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings || ws.definitions !== _definitions) {
        _settings = ws.settings;
        _definitions = ws.definitions;
        _definitionIds = null;

        // clear the cache
        _cache = {};
    }

    const key = definitionId || '-'; // undefined
    if (!_cache[key]) {
        if (!_definitionIds) {
            _definitionIds = Object.keys(ws.definitions.Documents);
        }

        const entityDesc: EntityDescriptor = {
            collection: 'Document',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Documents[definitionId], 'TitleSingular') || trx.instant('Document'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Documents[definitionId], 'TitlePlural') || trx.instant('Documents'),
            select: _select,
            apiEndpoint: !!definitionId ? `documents/${definitionId}` : 'documents',
            screenUrl: !!definitionId ? `documents/${definitionId}` : 'documents',
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: Document) => !!item.SerialNumber ? item.SerialNumber + '' : `(${trx.instant('New')})`,
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { control: 'text', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})` },
                Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'DocumentDefinition', foreignKeyName: 'DefinitionId' },
                OperatingSegmentId: { control: 'number', label: () => `${trx.instant('OperatingSegment')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                OperatingSegment: { control: 'navigation', label: () => trx.instant('OperatingSegment'), type: 'ResponsibilityCenter', foreignKeyName: 'OperatingSegmentId' },
                DocumentDate: { control: 'date', label: () => trx.instant('Document_DocumentDate') },
                Memo: { control: 'text', label: () => trx.instant('Memo') },
                MemoIsCommon: { control: 'boolean', label: () => trx.instant('Document_MemoIsCommon') },

                // TODO
                SerialNumber: { control: 'number', label: () => trx.instant('Document_SerialNumber'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                State: {
                    control: 'state',
                    label: () => trx.instant('State'),
                    choices: [0, -1, 1, -2, 2, -3, 3, -4, 4, 5],
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
                            case 5: return trx.instant('Document_State_Closed');
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
                            case 5: return '#28a745';
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

        // Adjust according to definitions
        const definition = _definitions.Documents[definitionId];
        if (!definition) {
            if (!!definitionId) {
                // Programmer mistake
                console.error(`defintionId '${definitionId}' doesn't exist`);
            }
        } else {
            delete entityDesc.properties.DefinitionId;
            delete entityDesc.properties.Definition;

            // TODO: adjust properties as per definition
        }

        _cache[key] = entityDesc;
    }

    return _cache[key];
}

export function serialNumber(serial: number, prefix: string, digits: number) {

    // Handle null and 0
    if (!serial) {
        return null;
    }

    let result = serial.toString();

    // Add a padding of zeros when needed
    if (result.length < digits) {
        result = '00000000000000000'.substring(0, digits - result.length) + result;
    }

    // Prepend the prefix
    result = prefix + result;

    // Return the result
    return result;
}
