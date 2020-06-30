// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';

export interface ExchangeRateForSave extends EntityWithKey {
    CurrencyId?: string;
    ValidAsOf?: string;
    AmountInCurrency?: number;
    AmountInFunctional?: number;
}

export interface ExchangeRate extends ExchangeRateForSave {
    Rate?: number;
    ValidTill?: string;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_ExchangeRate(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'ExchangeRate',
            titleSingular: () => trx.instant('ExchangeRate'),
            titlePlural: () => trx.instant('ExchangeRates'),
            select: ['ValidAsOf', 'CurrencyId'],
            apiEndpoint: 'exchange-rates',
            masterScreenUrl: 'exchange-rates',
            orderby: () => ['ValidAsOf desc', 'CurrencyId'],
            inactiveFilter: null,
            format: (item: ExchangeRate) => `${!!item.ValidAsOf} ${ws.getMultilingualValue('Currency', item.CurrencyId, 'Name')}`,
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CurrencyId: { control: 'text', label: () => `${trx.instant('ExchangeRate_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('ExchangeRate_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                ValidAsOf: { control: 'date', label: () => trx.instant('ExchangeRate_ValidAsOf') },
                ValidTill: { control: 'date', label: () => trx.instant('ExchangeRate_ValidTill') },
                AmountInCurrency: { control: 'number', label: () => trx.instant('ExchangeRate_AmountInCurrency'), minDecimalPlaces: 0, maxDecimalPlaces: 6, alignment: 'right' },
                AmountInFunctional: {
                    control: 'number',
                    label: () => `${trx.instant('ExchangeRate_AmountInFunctional')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: 6,
                    alignment: 'right'
                },
                Rate: { control: 'number', label: () => trx.instant('ExchangeRate_Rate'), minDecimalPlaces: 0, maxDecimalPlaces: 6, alignment: 'right' },
                CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
        }

        _cache = entityDesc;
    }

    return _cache;
}
