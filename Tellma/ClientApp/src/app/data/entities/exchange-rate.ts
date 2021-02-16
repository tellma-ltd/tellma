// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { formatDate } from '@angular/common';
import { DateGranularity, TimeGranularity } from './base/metadata-types';

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
            orderby: () => ['ValidAsOf', 'CurrencyId'],
            inactiveFilter: null,
            format: (item: ExchangeRate) => `${formatDate(item.ValidAsOf, 'yyyy-MM-dd', 'en-GB')}-${item.CurrencyId}`,
            formatFromVals: (vals: any[]) => `${formatDate(vals[0], 'yyyy-MM-dd', 'en-GB')} ${vals[1]}`,
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('ExchangeRate_Currency')} (${trx.instant('Id')})` },
                Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('ExchangeRate_Currency'), foreignKeyName: 'CurrencyId' },
                ValidAsOf: { datatype: 'date', control: 'date', label: () => trx.instant('ExchangeRate_ValidAsOf'), granularity: DateGranularity.days },
                ValidTill: { datatype: 'date', control: 'date', label: () => trx.instant('ExchangeRate_ValidTill'), granularity: DateGranularity.days },
                AmountInCurrency: { datatype: 'numeric', control: 'number', label: () => trx.instant('ExchangeRate_AmountInCurrency'), minDecimalPlaces: 0, maxDecimalPlaces: 6, isRightAligned: true, noSeparator: false },
                AmountInFunctional: {
                    datatype: 'numeric',
                    control: 'number',
                    label: () => `${trx.instant('ExchangeRate_AmountInFunctional')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})`,
                    minDecimalPlaces: ws.settings.FunctionalCurrencyDecimals,
                    maxDecimalPlaces: 6,
                    isRightAligned: true, noSeparator: false
                },
                Rate: { datatype: 'numeric', control: 'number', label: () => trx.instant('ExchangeRate_Rate'), minDecimalPlaces: 0, maxDecimalPlaces: 6, isRightAligned: true, noSeparator: false },

                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
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
