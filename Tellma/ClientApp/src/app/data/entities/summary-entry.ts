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
    ParticipantId?: number;
    ResourceId?: number;
    UnitId?: number;
    CustodianId?: number;
    CustodyId?: number;
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
                { key: 'FromDate', isRequired: false, desc: { datatype: 'date', control: 'date', label: () => trx.instant('FromDate') } },
                { key: 'ToDate', isRequired: false, desc: { datatype: 'date', control: 'date', label: () => trx.instant('ToDate') } },
            ],
            orderby: () => ['AccountId'],
            inactiveFilter: null,
            format: (__: Entity) => '',
            properties: {
                AccountId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Account')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Account: { datatype: 'entity', control: 'Account', label: () => trx.instant('Entry_Account'), foreignKeyName: 'AccountId' },
                CenterId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { datatype: 'entity', control: 'Center', label: () => trx.instant('Entry_Center'), foreignKeyName: 'CenterId' },
                CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entry_Currency')} (${trx.instant('Id')})` },
                Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('Entry_Currency'), foreignKeyName: 'CurrencyId' },
                ParticipantId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Participant')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Participant: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Entry_Participant'), foreignKeyName: 'ParticipantId' },
                ResourceId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { datatype: 'entity', control: 'Resource', label: () => trx.instant('Entry_Resource'), foreignKeyName: 'ResourceId' },
                UnitId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Entry_Unit'), foreignKeyName: 'UnitId' },
                CustodianId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custodian: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Entry_Custodian'), foreignKeyName: 'CustodianId' },
                CustodyId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_Custody')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custody: { datatype: 'entity', control: 'Custody', label: () => trx.instant('Entry_Custody'), foreignKeyName: 'CustodyId' },
                EntryTypeId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entry_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { datatype: 'entity', control: 'EntryType', label: () => trx.instant('Entry_EntryType'), foreignKeyName: 'EntryTypeId' },

                // MonetaryValue
                OpeningMonetaryValue: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_OpeningMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MonetaryValueIn: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_MonetaryValueIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MonetaryValueOut: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_MonetaryValueOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingMonetaryValue: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_ClosingMonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Quantity
                OpeningQuantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_OpeningQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                QuantityIn: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_QuantityIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                QuantityOut: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_QuantityOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingQuantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_ClosingQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Mass
                OpeningMass: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_OpeningMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MassIn: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_MassIn'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                MassOut: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_MassOut'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ClosingMass: { datatype: 'numeric', control: 'number', label: () => trx.instant('SummaryEntry_ClosingMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },

                // Value
                Opening: {
                    datatype: 'numeric',
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Opening')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Debit: {
                    datatype: 'numeric',
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Debit')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Credit: {
                    datatype: 'numeric',
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Credit')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
                Closing: {
                    datatype: 'numeric',
                    control: 'number', label: () => `${trx.instant('SummaryEntry_Closing')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, maxDecimalPlaces: ws.settings.FunctionalCurrencyDecimals, alignment: 'right'
                },
            }
        };

        _cache = entityDesc;
    }

    return _cache;
}
