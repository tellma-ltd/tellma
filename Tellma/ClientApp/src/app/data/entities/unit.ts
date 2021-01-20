// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';

export interface UnitForSave extends EntityForSave {
  UnitType?: 'Pure' | 'Time' | 'Distance' | 'Count' | 'Mass' | 'Volume' | 'MonetaryValue';
  Name?: string;
  Name2?: string;
  Name3?: string;
  Code?: string;
  Description?: string;
  Description2?: string;
  Description3?: string;
  UnitAmount?: number;
  BaseAmount?: number;
}

export interface Unit extends UnitForSave {
  IsActive?: boolean;
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Unit(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;
    _cache = {
      collection: 'Unit',
      titleSingular: () => trx.instant('Unit'),
      titlePlural: () => trx.instant('Units'),
      select: _select,
      apiEndpoint: 'units',
      masterScreenUrl: 'units',
      inactiveFilter: 'IsActive eq true',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { datatype: 'integral', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        UnitType: {
          datatype: 'string',
          control: 'choice',
          label: () => trx.instant('Unit_UnitType'),
          choices: ['Pure', 'Time', 'Distance', 'Count', 'Mass', 'Volume'],
          format: (c: string) => {
            switch (c) {
              case 'Pure': return trx.instant('Unit_Pure');
              case 'Time': return trx.instant('Unit_Time');
              case 'Distance': return trx.instant('Unit_Distance');
              case 'Count': return trx.instant('Unit_Count');
              case 'Mass': return trx.instant('Unit_Mass');
              case 'Volume': return trx.instant('Unit_Volume');
              default: return c;
            }
          }
        },
        UnitAmount: {
          datatype: 'decimal', control: 'number', label: () => trx.instant('Unit_UnitAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 9
        },
        BaseAmount: {
          datatype: 'decimal', control: 'number', label: () => trx.instant('Unit_BaseAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 9
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
