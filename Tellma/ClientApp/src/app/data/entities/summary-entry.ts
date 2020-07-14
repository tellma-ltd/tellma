// tslint:disable:variable-name
// tslint:disable:max-line-length
import { Entity } from './base/entity';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export interface SummaryEntry extends Entity {
    AccountId?: number;
    CenterId?: number;
    CurrencyId?: string;
    ResourceId?: number;
    UnitId?: number;
    ContractId?: number;
    EntryTypeId?: number;

    // Quantity
    OpeningQuantity?: number;
    QuantityIn?: number;
    QuantityOut?: number;
    ClosingQuantity?: number;

    // Mass
    OpeningMass?: number;
    MassIn?: number;
    MassOut?: number;
    ClosingMass?: number;

    // Monetary Value
    OpeningMonetaryValue?: number;
    MonetaryValueIn?: number;
    MonetaryValueOut?: number;
    ClosingMonetaryValue?: number;

    // Value
    Opening?: number;
    Debit?: number;
    Credit?: number;
    Closing?: number;
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_SummaryEntry(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'SummaryEntry',
            titleSingular: () => trx.instant('SummaryEntry'),
            titlePlural: () => trx.instant('SummaryEntries'),
            select: [],
            apiEndpoint: 'summary-entries',
            parameters: [
                { key: 'FromDate', isRequired: true, desc: { control: 'date', label: () => trx.instant('FromDate') } },
                { key: 'ToDate', isRequired: true, desc: { control: 'date', label: () => trx.instant('ToDate') } },
            ],
            masterScreenUrl: 'summary-entries',
            orderby: () => ['AccountId'],
            inactiveFilter: null,
            format: (__: Entity) => '',
            properties: {
                AccountId: { control: 'number', label: () => `${trx.instant('Entry_Account')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Account: { control: 'navigation', label: () => trx.instant('Entry_Account'), type: 'Account', foreignKeyName: 'AccountId' },
                CenterId: { control: 'number', label: () => `${trx.instant('Entry_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Entry_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Entry_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Entry_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                UnitId: { control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { control: 'navigation', label: () => trx.instant('Entry_Unit'), type: 'Unit', foreignKeyName: 'UnitId' },
                ContractId: { control: 'number', label: () => `${trx.instant('Entry_Contract')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Contract: { control: 'navigation', label: () => trx.instant('Entry_Contract'), type: 'Contract', foreignKeyName: 'ContractId' },
                EntryTypeId: { control: 'number', label: () => `${trx.instant('Entry_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { control: 'navigation', label: () => trx.instant('Entry_EntryType'), type: 'EntryType', foreignKeyName: 'EntryTypeId' },

                // MonetaryValue
                OpeningMonetaryValue: { control: 'number', label: () => trx.instant('SummaryEntry_OpeningMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MonetaryValueIn: { control: 'number', label: () => trx.instant('SummaryEntry_MonetaryValueIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MonetaryValueOut: { control: 'number', label: () => trx.instant('SummaryEntry_MonetaryValueOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingMonetaryValue: { control: 'number', label: () => trx.instant('SummaryEntry_ClosingMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Quantity
                OpeningQuantity: { control: 'number', label: () => trx.instant('SummaryEntry_OpeningQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                QuantityIn: { control: 'number', label: () => trx.instant('SummaryEntry_QuantityIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                QuantityOut: { control: 'number', label: () => trx.instant('SummaryEntry_QuantityOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingQuantity: { control: 'number', label: () => trx.instant('SummaryEntry_ClosingQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Mass
                OpeningMass: { control: 'number', label: () => trx.instant('SummaryEntry_OpeningMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MassIn: { control: 'number', label: () => trx.instant('SummaryEntry_MassIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MassOut: { control: 'number', label: () => trx.instant('SummaryEntry_MassOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingMass: { control: 'number', label: () => trx.instant('SummaryEntry_ClosingMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Value
                Opening: {
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Opening')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Debit: {
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Debit')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Credit: {
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Credit')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Closing: {
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Closing')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
