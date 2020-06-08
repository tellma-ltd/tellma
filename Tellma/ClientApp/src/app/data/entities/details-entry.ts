
// tslint:disable:variable-name
// tslint:disable:max-line-length
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityWithKey } from './base/entity-with-key';

export interface DetailsEntry extends EntityWithKey {
    LineId?: number;
    CenterId?: number;
    Direction?: number;
    AccountId?: number;
    ContractId: number;
    EntryTypeId?: number;
    ResourceId?: number;
    DueDate?: string;
    MonetaryValue?: number;
    AlgebraicMonetaryValue?: number;
    CurrencyId?: number;
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
    ExternalReference?: string;
    AdditionalReference?: string;
    NotedContractId?: number;
    NotedAgentName?: string;
    NotedAmount?: number;
    NotedDate?: string;
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
            screenUrl: 'details-entries', // TODO
            orderby: () => ['Id'],
            format: (__: EntityWithKey) => '',
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LineId: { control: 'number', label: () => `${trx.instant('Entry_Line')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Line: { control: 'navigation', label: () => trx.instant('Entry_Line'), type: 'Line', foreignKeyName: 'LineId' },
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
                ContractId: { control: 'number', label: () => `${trx.instant('Entry_Contract')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Contract: { control: 'navigation', label: () => trx.instant('Entry_Contract'), type: 'Contract', foreignKeyName: 'ContractId' },
                EntryTypeId: { control: 'number', label: () => `${trx.instant('Entry_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { control: 'navigation', label: () => trx.instant('Entry_EntryType'), type: 'EntryType', foreignKeyName: 'EntryTypeId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Entry_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                DueDate: { control: 'date', label: () => trx.instant('Entry_DueDate') },
                MonetaryValue: { control: 'number', label: () => trx.instant('Entry_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                AlgebraicMonetaryValue: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
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
                Value: { control: 'number', label: () => trx.instant('Entry_Value'), minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right' },
                AlgebraicValue: { control: 'number', label: () => trx.instant('DetailsEntry_AlgebraicValue'), minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right' },
                ExternalReference: { control: 'text', label: () => trx.instant('Entry_ExternalReference') },
                AdditionalReference: { control: 'text', label: () => trx.instant('Entry_AdditionalReference') },
                NotedContractId: { control: 'number', label: () => `${trx.instant('Entry_NotedContract')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                NotedContract: { control: 'navigation', label: () => trx.instant('Entry_NotedContract'), type: 'Contract', foreignKeyName: 'ContractId' },
                NotedAgentName: { control: 'text', label: () => trx.instant('Entry_NotedAgentName') },
                NotedAmount: { control: 'number', label: () => trx.instant('Entry_NotedAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                NotedDate: { control: 'date', label: () => trx.instant('Entry_NotedDate') },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
