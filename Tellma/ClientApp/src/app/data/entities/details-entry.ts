// tslint:disable:variable-name
// tslint:disable:max-line-length
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityWithKey } from './base/entity-with-key';
import { Router } from '@angular/router';

export interface DetailsEntry extends EntityWithKey {
    LineId?: number;
    CenterId?: number;
    Direction?: number;
    AccountId?: number;
    CustodianId?: number;
    CustodyId: number;
    EntryTypeId?: number;
    ParticipantId?: number;
    ResourceId?: number;
    Quantity?: number;
    AlgebraicQuantity?: number;
    NegativeAlgebraicQuantity?: number;
    UnitId?: number;
    MonetaryValue?: number;
    AlgebraicMonetaryValue?: number;
    NegativeAlgebraicMonetaryValue?: number;
    CurrencyId?: string;
    Count?: number;
    AlgebraicCount?: number;
    Mass?: number;
    AlgebraicMass?: number;
    Volume?: number;
    AlgebraicVolume?: number;
    Time?: number;
    AlgebraicTime?: number;
    Value?: number;
    AlgebraicValue?: number;
    NegativeAlgebraicValue?: number;
    Time1?: string;
    Time2?: string;
    ExternalReference?: string;
    AdditionalReference?: string;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;

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
            // parameters: [
            //     { key: 'CountUnitId', isRequired: false, desc: { control: 'navigation', label: () => trx.instant('Resource_CountUnit'), type: 'Unit', foreignKeyName: 'CountUnitId' } },
            //     { key: 'MassUnitId', isRequired: false, desc: { control: 'navigation', label: () => trx.instant('Resource_MassUnit'), type: 'Unit', foreignKeyName: 'CountUnitId' } },
            //     { key: 'VolumeUnitId', isRequired: false, desc: { control: 'navigation', label: () => trx.instant('Resource_VolumeUnit'), type: 'Unit', foreignKeyName: 'CountUnitId' } },
            // ],
            masterScreenUrl: 'details-entries',
            navigateToDetailsSelect: ['Line/Document/DefinitionId'],
            navigateToDetails: (detailsEntry: DetailsEntry, router: Router, _: string) => {
                const line = ws.get('LineForQuery', detailsEntry.LineId);
                const docId = line.DocumentId;

                const definitionId = ws.Document[docId].DefinitionId;
                const extras = { state_key: 'from_entries' }; // fake state key to hide forward and backward navigation in details screen
                router.navigate(['app', wss.ws.tenantId + '', 'documents', definitionId, docId, extras]);
            },
            orderby: () => ['Id'],
            inactiveFilter: null,
            format: (__: EntityWithKey) => '',
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LineId: { control: 'number', label: () => `${trx.instant('Entry_Line')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Line: { control: 'navigation', label: () => trx.instant('Entry_Line'), type: 'LineForQuery', foreignKeyName: 'LineId' },
                CenterId: { control: 'number', label: () => `${trx.instant('Entry_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Entry_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                Direction: {
                    control: 'choice',
                    label: () => trx.instant('Entry_Direction'),
                    choices: [-1, 1],
                    format: (c: number) => {
                        switch (c) {
                            case 1: return trx.instant('Entry_Direction_Debit');
                            case -1: return trx.instant('Entry_Direction_Credit');
                            default: return '';
                        }
                    }
                },
                AccountId: { control: 'number', label: () => `${trx.instant('Entry_Account')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Account: { control: 'navigation', label: () => trx.instant('Entry_Account'), type: 'Account', foreignKeyName: 'AccountId' },
                CustodianId: { control: 'number', label: () => `${trx.instant('Entry_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custodian: { control: 'navigation', label: () => trx.instant('Entry_Custodian'), type: 'Relation', foreignKeyName: 'CustodianId' },
                CustodyId: { control: 'number', label: () => `${trx.instant('Entry_Custody')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custody: { control: 'navigation', label: () => trx.instant('Entry_Custody'), type: 'Custody', foreignKeyName: 'CustodyId' },
                EntryTypeId: { control: 'number', label: () => `${trx.instant('Entry_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { control: 'navigation', label: () => trx.instant('Entry_EntryType'), type: 'EntryType', foreignKeyName: 'EntryTypeId' },
                ParticipantId: { control: 'number', label: () => `${trx.instant('Entry_Participant')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Participant: { control: 'navigation', label: () => trx.instant('Entry_Participant'), type: 'Relation', foreignKeyName: 'ParticipantId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Entry_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                Quantity: { control: 'number', label: () => trx.instant('Entry_Quantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicQuantity: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                NegativeAlgebraicQuantity: { control: 'number', label: () => trx.instant('DetailsEntry_NegativeAlgebraicQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                UnitId: { control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { control: 'navigation', label: () => trx.instant('Entry_Unit'), type: 'Unit', foreignKeyName: 'UnitId' },
                MonetaryValue: { control: 'number', label: () => trx.instant('Entry_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicMonetaryValue: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                NegativeAlgebraicMonetaryValue: { control: 'number', label: () => trx.instant('DetailsEntry_NegativeAlgebraicMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Entry_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                Count: { control: 'number', label: () => trx.instant('DetailsEntry_Count'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicCount: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicCount'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                Mass: { control: 'number', label: () => trx.instant('DetailsEntry_Mass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicMass: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                Volume: { control: 'number', label: () => trx.instant('DetailsEntry_Volume'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicVolume: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicVolume'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                Time: { control: 'number', label: () => trx.instant('DetailsEntry_Time'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicTime: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicTime'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                Value: {
                    control: 'number',
                    label: () => `${trx.instant('Entry_Value')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    alignment: 'right'
                },
                AlgebraicValue: {
                    control: 'number',
                    label: () => `${trx.instant('DetailsEntry_AlgebraicValue')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    alignment: 'right'
                },
                NegativeAlgebraicValue: {
                    control: 'number',
                    label: () => `${trx.instant('DetailsEntry_NegativeAlgebraicValue')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    alignment: 'right'
                },
                Time1: { control: 'datetime', label: () => trx.instant('Entry_Time1') },
                Time2: { control: 'datetime', label: () => trx.instant('Entry_Time2') },
                ExternalReference: { control: 'text', label: () => trx.instant('Entry_ExternalReference') },
                AdditionalReference: { control: 'text', label: () => trx.instant('Entry_AdditionalReference') },
                NotedAgentName: { control: 'text', label: () => trx.instant('Entry_NotedAgentName') },
                NotedAmount: { control: 'number', label: () => trx.instant('Entry_NotedAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                NotedDate: { control: 'date', label: () => trx.instant('Entry_NotedDate') },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
