// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';

export interface CurrencyForSave extends EntityForSave {
  Name?: string;
  Name2?: string;
  Name3?: string;
  Description?: string;
  Description2?: string;
  Description3?: string;
  NumericCode?: number;
  E?: number;
}

export interface Currency extends CurrencyForSave {
  IsActive?: boolean;
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Currency(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;
    _cache = {
      collection: 'Currency',
      titleSingular: () => trx.instant('Currency'),
      titlePlural: () => trx.instant('Currencies'),
      select: _select,
      apiEndpoint: 'currencies',
      masterScreenUrl: 'currencies',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        NumericCode: {
          datatype: 'numeric',
          control: 'number',
          label: () => trx.instant('Currency_NumericCode'),
          minDecimalPlaces: 0,
          maxDecimalPlaces: 0
        },
        E: {
          datatype: 'numeric',
          control: 'choice',
          label: () => trx.instant('Currency_DecimalPlaces'),
          choices: [0, 2, 3, 4],
          format: (c: number | string) => (c === null || c === undefined) ? '' : c.toString()
        },

        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
        ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
        ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
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
