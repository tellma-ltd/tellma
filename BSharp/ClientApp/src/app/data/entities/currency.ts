// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';

export class CurrencyForSave extends EntityForSave {
  Name: string;
  Name2: string;
  Name3: string;
  Description: string;
  Description2: string;
  Description3: string;
  E: number;
}

export class Currency extends CurrencyForSave {
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Currency(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang || ws.settings !== _settings) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _cache = {
      titleSingular: trx.instant('Currency'),
      titlePlural:  trx.instant('Currencies'),
      select: _select,
      apiEndpoint: 'currencies',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'text', label: trx.instant('Code') },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Description: { control: 'text', label: trx.instant('Description') + ws.primaryPostfix },
        Description2: { control: 'text', label: trx.instant('Description') + ws.secondaryPostfix },
        Description3: { control: 'text', label: trx.instant('Description') + ws.ternaryPostfix },
        E: {
          control: 'choice',
          label: trx.instant('Currency_DecimalPlaces'),
          choices: [0, 2, 3],
          format: (c: number | string) => (c === null || c === undefined) ? '' : c.toString()
        },
        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete _cache.properties.Name2;
      delete _cache.properties.Description2;
    }

    if (!ws.settings.TernaryLanguageId) {
      delete _cache.properties.Name3;
      delete _cache.properties.Description3;
    }
  }

  return _cache;
}
