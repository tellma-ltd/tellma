// tslint:disable:variable-name
// tslint:disable:max-line-length
import { TenantWorkspace, WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, BooleanPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { LineForSave, Line } from './line';
import { DocumentAssignment } from './document-assignment';
import { AttachmentForSave, Attachment } from './attachment';
import { EntityForSave } from './base/entity-for-save';
import { DocumentStateChange } from './document-state-change';
import { DocumentLineDefinitionEntry, DocumentLineDefinitionEntryForSave } from './document-line-definition-entry';

export type DocumentState = 0 | 1 | -1;
export type DocumentClearance = 0 | 1 | 2;

export interface DocumentForSave<TLine = LineForSave, TLineDefinitionEntry = DocumentLineDefinitionEntryForSave, TAttachment = AttachmentForSave> extends EntityForSave {
    SerialNumber?: number;
    Clearance?: DocumentClearance;
    PostingDate?: string;
    PostingDateIsCommon?: boolean;
    Memo?: string;
    MemoIsCommon?: boolean;

    SegmentId?: number;

    CurrencyId?: string;
    CurrencyIsCommon?: boolean;
    CenterId?: number;
    CenterIsCommon?: boolean;

    CustodianId?: number;
    CustodianIsCommon?: boolean;
    CustodyId?: number;
    CustodyIsCommon?: boolean;
    ParticipantId?: number;
    ParticipantIsCommon?: boolean;
    ResourceId?: number;
    ResourceIsCommon?: boolean;

    Quantity?: number;
    QuantityIsCommon?: boolean;
    UnitId?: number;
    UnitIsCommon?: boolean;
    Time1?: number;
    Time1IsCommon?: boolean;
    Time2?: number;
    Time2IsCommon?: boolean;

    ExternalReference?: string;
    ExternalReferenceIsCommon?: boolean;
    AdditionalReference?: string;
    AdditionalReferenceIsCommon?: boolean;

    Lines?: TLine[];
    LineDefinitionEntries?: TLineDefinitionEntry[];
    Attachments?: TAttachment[];
}

export interface Document extends DocumentForSave<Line, DocumentLineDefinitionEntry, Attachment> {
    DefinitionId?: number;
    Code?: string;
    State?: DocumentState;
    StateAt?: string;
    Comment?: string;
    AssigneeId?: number;
    AssignedAt?: string;
    AssignedById?: number;
    OpenedAt?: string;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
    AssignmentsHistory?: DocumentAssignment[];
    StatesHistory?: DocumentStateChange[];
}

const _select = ['SerialNumber'];
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: number]: EntityDescriptor } = {};
let _definitionIds: number[];

export function metadata_Document(wss: WorkspaceService, trx: TranslateService, definitionId: number): EntityDescriptor {
    const ws = wss.currentTenant;
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
            _definitionIds = Object.keys(ws.definitions.Documents).map(e => +e);
        }

        const entityDesc: EntityDescriptor = {
            collection: 'Document',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Documents[definitionId], 'TitleSingular') || trx.instant('Document'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Documents[definitionId], 'TitlePlural') || trx.instant('Documents'),
            select: _select,
            apiEndpoint: !!definitionId ? `documents/${definitionId}` : 'documents',
            masterScreenUrl: !!definitionId ? `documents/${definitionId}` : 'documents',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: 'State ne -1',
            includeInactveLabel: () => trx.instant('IncludeCanceled'),
            format: (doc: Document) => !!doc.SerialNumber ? formatSerial(doc.SerialNumber, getPrefix(ws, doc.DefinitionId || definitionId), getCodeWidth(ws, doc.DefinitionId || definitionId)) : `(${trx.instant('New')})`,
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'DocumentDefinition', foreignKeyName: 'DefinitionId' },
                Clearance: {
                    control: 'choice',
                    label: () => trx.instant('Document_Clearance'),
                    choices: [0, 1, 2],
                    format: (c: number) => trx.instant('Document_Clearance_' + c)
                },
                PostingDate: { control: 'date', label: () => trx.instant('Document_PostingDate') },
                PostingDateIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Document_PostingDate') }) },
                Memo: { control: 'text', label: () => trx.instant('Memo') },
                MemoIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Memo') }) },

                SegmentId: { control: 'number', label: () => `${trx.instant('Document_Segment')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Segment: { control: 'navigation', label: () => trx.instant('Document_Segment'), type: 'Center', foreignKeyName: 'SegmentId' },

                CurrencyId: { control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Entry_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                CurrencyIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Currency') }) },
                CenterId: { control: 'number', label: () => `${trx.instant('Document_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Document_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                CenterIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Document_Center') }) },

                CustodianId: { control: 'number', label: () => `${trx.instant('Entry_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custodian: { control: 'navigation', label: () => trx.instant('Entry_Custodian'), type: 'Relation', foreignKeyName: 'CustodianId' },
                CustodianIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Custodian') }) },
                CustodyId: { control: 'number', label: () => `${trx.instant('Entry_Custody')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custody: { control: 'navigation', label: () => trx.instant('Entry_Custody'), type: 'Custody', foreignKeyName: 'CustodyId' },
                CustodyIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Custody') }) },
                ParticipantId: { control: 'number', label: () => `${trx.instant('Entry_Participant')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Participant: { control: 'navigation', label: () => trx.instant('Entry_Participant'), type: 'Relation', foreignKeyName: 'ParticipantId' },
                ParticipantIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Participant') }) },
                ResourceId: { control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Entry_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                ResourceIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Resource') }) },

                Quantity: { control: 'number', label: () => trx.instant('Entry_Quantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                QuantityIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Quantity') }) },
                UnitId: { control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { control: 'navigation', label: () => trx.instant('Entry_Unit'), type: 'Unit', foreignKeyName: 'UnitId' },
                UnitIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Unit') }) },
                Time1: { control: 'date', label: () => trx.instant('Entry_Time1') },
                Time1IsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Time1') }) },
                Time2: { control: 'date', label: () => trx.instant('Entry_Time2'), },
                Time2IsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Time2') }) },

                ExternalReference: { control: 'text', label: () => trx.instant('Entry_ExternalReference') },
                ExternalReferenceIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_ExternalReference') }) },
                AdditionalReference: { control: 'text', label: () => trx.instant('Entry_AdditionalReference') },
                AdditionalReferenceIsCommon: { control: 'boolean', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_AdditionalReference') }) },

                SerialNumber: {
                    control: 'serial', label: () => trx.instant('Document_SerialNumber'),
                    format: (serial: number) => formatSerial(serial, getPrefix(ws, definitionId), getCodeWidth(ws, definitionId))
                },
                Code: { control: 'text', label: () => trx.instant('Code') },
                State: {
                    control: 'state',
                    label: () => trx.instant('Document_State'),
                    choices: [0, -1, 1],
                    format: (state: number) => {
                        if (state >= 0) {
                            return trx.instant('Document_State_' + state);
                        } else {
                            return trx.instant('Document_State_minus_' + (-state));
                        }
                    },
                    color: (c: number) => {
                        switch (c) {
                            case 0: return '#6c757d';
                            case -1: return '#dc3545';
                            case 1: return '#28a745';
                            default: return null;
                        }
                    }
                },
                StateAt: { control: 'datetime', label: () => trx.instant('Document_StateAt') },

                AssigneeId: { control: 'number', label: () => `${trx.instant('Document_Assignee')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Assignee: { control: 'navigation', label: () => trx.instant('Document_Assignee'), type: 'User', foreignKeyName: 'AssigneeId' },
                AssignedById: { control: 'number', label: () => `${trx.instant('Document_AssignedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AssignedBy: { control: 'navigation', label: () => trx.instant('Document_AssignedBy'), type: 'User', foreignKeyName: 'AssignedById' },
                AssignedAt: { control: 'datetime', label: () => trx.instant('Document_AssignedAt') },

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

            // Definition specific adjustments
            if (definition.State === 'Archived') {
                entityDesc.isArchived = true;
            }

            const props = entityDesc.properties;
            delete props.DefinitionId;
            delete props.Definition;

            // Simple properties whose label and visibility are overriden by the definition
            for (const propName of ['Memo', 'PostingDate', 'Quantity', 'Time1', 'Time2', 'AdditionalReference', 'ExternalReference']) {
                if (!definition[propName + 'Visibility']) {
                    delete props[propName];
                    delete props[propName + 'IsCommon'];
                } else {

                    // Nav property
                    const propDesc = props[propName];
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    // IsCommon property
                    const isCommonPropDesc = props[propName + 'IsCommon'] as BooleanPropDescriptor;
                    isCommonPropDesc.label = () => trx.instant('Field0IsCommon', { 0: propDesc.label() });
                }
            }

            if (!definition.MemoIsCommonVisibility) {
                // Memo in particular can remain visible without MemoIsCommon
                delete props.MemoIsCommon;
            }

            // Navigation properties whose label and visibility are overriden by the definition
            for (const propName of ['Currency', 'Center', 'Custodian', 'Custody', 'Participant', 'Resource', 'Unit']) {
                if (!definition[propName + 'Visibility']) {

                    delete props[propName + 'Id'];
                    delete props[propName];
                    delete props[propName + 'IsCommon'];
                } else {

                    // Nav property
                    const propDesc = props[propName] as NavigationPropDescriptor;
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    // Definition
                    const defs = definition[propName + 'DefinitionIds'] as number[];
                    if (!!defs && defs.length === 1) {
                        propDesc.definition = defs[0];
                    }

                    // Foreign key property
                    const idPropDesc = props[propName + 'Id'];
                    idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;

                    // IsCommon property
                    const isCommonPropDesc = props[propName + 'IsCommon'] as BooleanPropDescriptor;
                    isCommonPropDesc.label = () => trx.instant('Field0IsCommon', { 0: propDesc.label() });
                }
            }
        }

        _cache[key] = entityDesc;
    }

    return _cache[key];
}

export function formatSerialFromDefId(serial: number, ws: TenantWorkspace, definitionId: number) {
    return formatSerial(serial, getPrefix(ws, definitionId), getCodeWidth(ws, definitionId));
}

export function formatSerial(serial: number, prefix: string, codeWidth: number) {

    // Handle null and 0
    if (serial === null || serial === undefined) {
        return null;
    }

    let result = serial.toString();

    // Add a padding of zeros when needed
    if (result.length < codeWidth) {
        result = '00000000000000000'.substring(0, codeWidth - result.length) + result;
    }

    // Prepend the prefix
    if (!!prefix) {
        result = prefix + result;
    }

    // Return the result
    return result;
}

function getPrefix(ws: TenantWorkspace, definitionId: number) {
    const def = ws.definitions.Documents[definitionId];
    return !!def ? def.Prefix : '';
}

function getCodeWidth(ws: TenantWorkspace, definitionId: number) {
    const def = ws.definitions.Documents[definitionId];
    return !!def ? def.CodeWidth : 4;
}
