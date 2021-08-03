// tslint:disable:variable-name
// tslint:disable:max-line-length
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityWithKey } from './base/entity-with-key';
import { Router } from '@angular/router';
import { DateGranularity } from './base/metadata-types';

export interface DetailsEntry extends EntityWithKey {
    LineId?: number;
    CenterId?: number;
    Direction?: number;
    AccountId?: number;
    RelationId?: number;
    NotedRelationId?: number;
    EntryTypeId?: number;
    ResourceId?: number;
    Quantity?: number;
    UnitId?: number;
    MonetaryValue?: number;
    CurrencyId?: string;
    Value?: number;
    RValue?: number;
    PValue?: number;
    Time1?: string;
    Duration?: number;
    DurationUnitId?: number;
    Time2?: string;
    ExternalReference?: string;
    ReferenceSourceId?: string;
    InternalReference?: string;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;
    BaseQuantity?: number;
    BaseUnitId?: number;

    Accumulation?: number; // Used by account statement
    QuantityAccumulation?: number; // Used by account statement
    MonetaryValueAccumulation?: number; // Used by account statement
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_DetailsEntry(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'DetailsEntry',
            titleSingular: () => trx.instant('DetailsEntry'),
            titlePlural: () => trx.instant('DetailsEntries'),
            select: [],
            apiEndpoint: 'details-entries',
            masterScreenUrl: 'details-entries',
            navigateToDetailsSelect: ['Line.DocumentId', 'Line.Document.DefinitionId'],
            navigateToDetails: (detailsEntry: DetailsEntry, router: Router, _: string) => {
                const line = ws.get('LineForQuery', detailsEntry.LineId);
                const docId = line.DocumentId;
                const definitionId = ws.Document[docId].DefinitionId;
                entityDesc.navigateToDetailsFromVals([docId, definitionId], router);
            },
            navigateToDetailsFromVals: (vals: any[], router: Router) => {
                const [docId, definitionId] = vals;
                const extras = { state_key: 'from_entries' }; // fake state key to hide forward and backward navigation in details screen
                router.navigate(['app', wss.ws.tenantId + '', 'documents', definitionId, docId, extras]);
            },
            orderby: () => ['Id'],
            inactiveFilter: null,
            format: (__: EntityWithKey) => '',
            formatFromVals: (vals: any[]) => '',
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LineId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Line')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Line: { datatype: 'entity', control: 'LineForQuery', label: () => trx.instant('Entry_Line'), foreignKeyName: 'LineId' },
                CenterId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { datatype: 'entity', control: 'Center', label: () => trx.instant('Entry_Center'), foreignKeyName: 'CenterId' },
                Direction: {
                    datatype: 'numeric',
                    control: 'choice',
                    label: () => trx.instant('Entry_Direction'),
                    choices: [-1, 1],
                    format: (c: number) => {
                        switch (c) {
                            case 1: return trx.instant('Entry_Direction_Debit');
                            case -1: return trx.instant('Entry_Direction_Credit');
                            default: return c + '';
                        }
                    }
                },
                AccountId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Account')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Account: { datatype: 'entity', control: 'Account', label: () => trx.instant('Entry_Account'), foreignKeyName: 'AccountId' },
                RelationId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Relation')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Relation: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Entry_Relation'), foreignKeyName: 'RelationId' },
                EntryTypeId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { datatype: 'entity', control: 'EntryType', label: () => trx.instant('Entry_EntryType'), foreignKeyName: 'EntryTypeId' },
                ResourceId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { datatype: 'entity', control: 'Resource', label: () => trx.instant('Entry_Resource'), foreignKeyName: 'ResourceId' },
                NotedRelationId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_NotedRelation')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                NotedRelation: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Entry_NotedRelation'), foreignKeyName: 'NotedRelationId' },
                Quantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_Quantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                UnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Entry_Unit'), foreignKeyName: 'UnitId' },
                MonetaryValue: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('Entry_Currency'), foreignKeyName: 'CurrencyId' },
                Value: {
                    datatype: 'numeric',
                    control: 'number',
                    label: () => `${trx.instant('Entry_Value')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    isRightAligned: true, noSeparator: false
                },
                RValue: {
                    datatype: 'numeric',
                    control: 'number',
                    label: () => `${trx.instant('Entry_RValue')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    isRightAligned: true, noSeparator: false
                },
                PValue: {
                    datatype: 'numeric',
                    control: 'number',
                    label: () => `${trx.instant('Entry_PValue')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    isRightAligned: true, noSeparator: false
                },
                Time1: { datatype: 'datetime', control: 'date', label: () => trx.instant('Entry_Time1'), granularity: DateGranularity.days },
                Duration: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_Duration'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                DurationUnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_DurationUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DurationUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Entry_DurationUnit'), foreignKeyName: 'DurationUnitId' },
                Time2: { datatype: 'datetime', control: 'date', label: () => trx.instant('Entry_Time2'), granularity: DateGranularity.days },
                ExternalReference: { datatype: 'string', control: 'text', label: () => trx.instant('Entry_ExternalReference') },
                ReferenceSourceId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_ReferenceSource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ReferenceSource: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Entry_ReferenceSource'), foreignKeyName: 'ReferenceSourceId' },
                InternalReference: { datatype: 'string', control: 'text', label: () => trx.instant('Entry_InternalReference') },
                NotedAgentName: { datatype: 'string', control: 'text', label: () => trx.instant('Entry_NotedAgentName') },
                NotedAmount: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entry_NotedAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                NotedDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entry_NotedDate'), granularity: DateGranularity.days },
                BaseQuantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('DetailsEntry_BaseQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, noSeparator: false },
                BaseUnitId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('DetailsEntry_BaseUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: true },
                BaseUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('DetailsEntry_BaseUnit'), foreignKeyName: 'BaseUnitId' },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
