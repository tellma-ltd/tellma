// tslint:disable:variable-name
// tslint:disable:max-line-length
import { TenantWorkspace, WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, BitPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { LineForSave, Line } from './line';
import { DocumentAssignment } from './document-assignment';
import { AttachmentForSave, Attachment } from './attachment';
import { EntityForSave } from './base/entity-for-save';
import { DocumentStateChange } from './document-state-change';
import { DocumentLineDefinitionEntry, DocumentLineDefinitionEntryForSave } from './document-line-definition-entry';
import { DateGranularity, TimeGranularity } from './base/metadata-types';

export type DocumentState = 0 | 1 | -1;
export type DocumentClearance = 0 | 1 | 2;

export interface DocumentForSave<TLine = LineForSave, TLineDefinitionEntry = DocumentLineDefinitionEntryForSave, TAttachment = AttachmentForSave> extends EntityForSave {
    SerialNumber?: number;
    Clearance?: DocumentClearance;
    PostingDate?: string;
    PostingDateIsCommon?: boolean;
    Memo?: string;
    MemoIsCommon?: boolean;

    CurrencyId?: string;
    CurrencyIsCommon?: boolean;
    CenterId?: number;
    CenterIsCommon?: boolean;

    AgentId?: number;
    AgentIsCommon?: boolean;
    ResourceId?: number;
    ResourceIsCommon?: boolean;
    NotedAgentId?: number;
    NotedAgentIsCommon?: boolean;
    NotedResourceId?: number;
    NotedResourceIsCommon?: boolean;

    Quantity?: number;
    QuantityIsCommon?: boolean;
    UnitId?: number;
    UnitIsCommon?: boolean;
    Time1?: number;
    Time1IsCommon?: boolean;
    Duration?: number;
    DurationIsCommon?: boolean;
    DurationUnitId?: number;
    DurationUnitIsCommon?: boolean;
    Time2?: number;
    Time2IsCommon?: boolean;

    ExternalReference?: string;
    ExternalReferenceIsCommon?: boolean;
    ReferenceSourceId?: number;
    ReferenceSourceIsCommon?: boolean;
    InternalReference?: string;
    InternalReferenceIsCommon?: boolean;

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
            formatFromVals: (vals: any[]) => !!vals[0] ? formatSerial(vals[0], getPrefix(ws, definitionId), getCodeWidth(ws, definitionId)) : `(${trx.instant('New')})`,
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Definition: { datatype: 'entity', control: 'DocumentDefinition', label: () => trx.instant('Definition'), foreignKeyName: 'DefinitionId' },
                Clearance: {
                    datatype: 'numeric',
                    control: 'choice',
                    label: () => trx.instant('Document_Clearance'),
                    choices: [0, 1, 2],
                    format: (c: number) => trx.instant('Document_Clearance_' + c)
                },
                PostingDate: { datatype: 'date', control: 'date', label: () => trx.instant('Document_PostingDate'), granularity: DateGranularity.days },
                PostingDateIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Document_PostingDate') }) },
                Memo: { datatype: 'string', control: 'text', label: () => trx.instant('Memo') },
                MemoIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Memo') }) },

                CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('Entry_Currency'), foreignKeyName: 'CurrencyId' },
                CurrencyIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Currency') }) },
                CenterId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Document_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { datatype: 'entity', control: 'Center', label: () => trx.instant('Document_Center'), foreignKeyName: 'CenterId', filter: 'CenterType eq \'BusinessUnit\'' },
                CenterIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Document_Center') }) },

                AgentId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent: { datatype: 'entity', control: 'Agent', label: () => trx.instant('Entry_Agent'), foreignKeyName: 'AgentId' },
                AgentIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Agent') }) },
                ResourceId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { datatype: 'entity', control: 'Resource', label: () => trx.instant('Entry_Resource'), foreignKeyName: 'ResourceId' },
                ResourceIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Resource') }) },
                NotedAgentId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_NotedAgent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                NotedAgent: { datatype: 'entity', control: 'Agent', label: () => trx.instant('Entry_NotedAgent'), foreignKeyName: 'NotedAgentId' },
                NotedAgentIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_NotedAgent') }) },
                NotedResourceId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_NotedResource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                NotedResource: { datatype: 'entity', control: 'Resource', label: () => trx.instant('Entry_NotedResource'), foreignKeyName: 'NotedResourceId' },
                NotedResourceIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_NotedResource') }) },

                Quantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_Quantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                QuantityIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Quantity') }) },
                UnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Entry_Unit'), foreignKeyName: 'UnitId' },
                UnitIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Unit') }) },
                Time1: { datatype: 'datetime', control: 'date', label: () => trx.instant('Entry_Time1'), granularity: DateGranularity.days },
                Time1IsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Time1') }) },
                Duration: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_Duration'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                DurationIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Duration') }) },
                DurationUnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_DurationUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DurationUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Entry_DurationUnit'), foreignKeyName: 'DurationUnitId' },
                DurationUnitIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_DurationUnit') }) },
                Time2: { datatype: 'datetime', control: 'date', label: () => trx.instant('Entry_Time2'), granularity: DateGranularity.days },
                Time2IsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_Time2') }) },

                ExternalReference: { datatype: 'string', control: 'text', label: () => trx.instant('Entry_ExternalReference') },
                ExternalReferenceIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_ExternalReference') }) },
                ReferenceSourceId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_ReferenceSource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ReferenceSource: { datatype: 'entity', control: 'Agent', label: () => trx.instant('Entry_ReferenceSource'), foreignKeyName: 'ReferenceSourceId' },
                ReferenceSourceIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_ReferenceSource') }) },
                InternalReference: { datatype: 'string', control: 'text', label: () => trx.instant('Entry_InternalReference') },
                InternalReferenceIsCommon: { datatype: 'bit', control: 'check', label: () => trx.instant('Field0IsCommon', { 0: trx.instant('Entry_InternalReference') }) },

                SerialNumber: {
                    datatype: 'numeric',
                    control: 'serial', label: () => trx.instant('Document_SerialNumber'),
                    prefix: getPrefix(ws, definitionId),
                    codeWidth: getCodeWidth(ws, definitionId)
                },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                State: {
                    datatype: 'numeric',
                    control: 'choice',
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
                StateAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('Document_StateAt'), granularity: TimeGranularity.minutes },
                Comment: { datatype: 'string', control: 'text', label: () => trx.instant('Document_Comment') },

                AssigneeId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Document_Assignee')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Assignee: { datatype: 'entity', control: 'User', label: () => trx.instant('Document_Assignee'), foreignKeyName: 'AssigneeId' },
                AssignedById: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Document_AssignedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AssignedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('Document_AssignedBy'), foreignKeyName: 'AssignedById' },
                AssignedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('Document_AssignedAt'), granularity: TimeGranularity.minutes },

                // Audit
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
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
            for (const propName of ['Memo', 'PostingDate', 'Quantity', 'Time1', 'Duration', 'Time2', 'InternalReference', 'ExternalReference']) {
                if (!definition[propName + 'Visibility']) {
                    delete props[propName];
                    delete props[propName + 'IsCommon'];
                } else {

                    // Nav property
                    const propDesc = props[propName];
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    // IsCommon property
                    const isCommonPropDesc = props[propName + 'IsCommon'] as BitPropDescriptor;
                    isCommonPropDesc.label = () => trx.instant('Field0IsCommon', { 0: propDesc.label() });
                }
            }

            // Navigation properties whose label and visibility are overriden by the definition
            for (const propName of ['Currency', 'Center', 'Agent', 'Resource', 'NotedAgent', 'NotedResource', 'Unit', 'DurationUnit', 'ReferenceSource']) {
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
                        propDesc.definitionId = defs[0];
                    }

                    // Foreign key property
                    const idPropDesc = props[propName + 'Id'];
                    idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;

                    // IsCommon property
                    const isCommonPropDesc = props[propName + 'IsCommon'] as BitPropDescriptor;
                    isCommonPropDesc.label = () => trx.instant('Field0IsCommon', { 0: propDesc.label() });
                }
            }

            // The following 3 properties can remain visible without their 'IsCommon' counterpart
            if (!definition.PostingDateIsCommonVisibility) {
                delete props.PostingDateIsCommon;
            }

            if (!definition.CenterIsCommonVisibility) {
                delete props.CenterIsCommon;
            }

            if (!definition.MemoIsCommonVisibility) {
                delete props.MemoIsCommon;
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
