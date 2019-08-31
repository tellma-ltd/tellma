import { EntityForSave } from './base/entity-for-save';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from './settings';
import { DtoDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';

export class MeasurementUnitForSave extends EntityForSave {
  Name: string;
  Name2: string;
  Code: string;
  UnitType: 'Pure' | 'Time' | 'Distance' | 'Count' | 'Mass' | 'Volume' | 'Money';
  UnitAmount: number;
  BaseAmount: number;
}

export class MeasurementUnit extends MeasurementUnitForSave {
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

// Choice list (Also repeated in measurement units master template)
export const MeasurementUnit_UnitType = {
  'Pure': 'MU_Pure',
  'Time': 'MU_Time',
  'Distance': 'MU_Distance',
  'Count': 'MU_Count',
  'Mass': 'MU_Mass',
  'Volume': 'MU_Volume',
  'Money': 'MU_Money'
};

const _select = ['', '2'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _cache: DtoDescriptor;

export function metadata_MeasurementUnit(ws: TenantWorkspace, trx: TranslateService, _subtype: string): DtoDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang || ws.settings !== _settings) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _cache = {
      select: _select,
      apiEndpoint: 'measurement-units',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },
        UnitType: {
          control: 'choice',
          label: trx.instant('MU_UnitType'),
          choices: ['Pure', 'Time', 'Distance', 'Count', 'Mass', 'Volume', 'Money'],
          format: (c: string) => {
            switch (c) {
              case 'Pure': return trx.instant('MU_Pure');
              case 'Time': return trx.instant('MU_Time');
              case 'Distance': return trx.instant('MU_Distance');
              case 'Count': return trx.instant('MU_Count');
              case 'Mass': return trx.instant('MU_Mass');
              case 'Volume': return trx.instant('MU_Volume');
              case 'Money': return trx.instant('MU_Money');
              default: return c;
            }
          }
        },
        UnitAmount: { control: 'number', label: trx.instant('MU_UnitAmount'),  minDecimalPlaces: 0, maxDecimalPlaces: 9 },
        BaseAmount: { control: 'number', label: trx.instant('MU_BaseAmount'), minDecimalPlaces: 0, maxDecimalPlaces: 9 },
        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };
  }

  return _cache;
}