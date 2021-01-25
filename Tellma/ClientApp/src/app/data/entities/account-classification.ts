// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';

export interface AccountClassificationForSave extends EntityForSave {
  ParentId?: number;
  Name?: string;
  Name2?: string;
  Name3?: string;
  Code?: string;
  AccountTypeParentId?: number;
}

export interface AccountClassification extends AccountClassificationForSave {
  Level?: number;
  ChildCount?: number;
  ActiveChildCount?: number;
  IsActive?: boolean;
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_AccountClassification(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;

    // clear the cache
    _cache = null;
  }

  if (!_cache) {
    _settings = ws.settings;
    const entityDesc: EntityDescriptor = {
      collection: 'AccountClassification',
      titleSingular: () => trx.instant('AccountClassification'),
      titlePlural: () => trx.instant('AccountClassifications'),
      select: _select,
      apiEndpoint: 'account-classifications',
      masterScreenUrl: 'account-classifications',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        AccountTypeParentId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('AccountClassification_AccountTypeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        AccountTypeParent: { datatype: 'entity', control: 'AccountType', label: () => trx.instant('AccountClassification_AccountTypeParent'), foreignKeyName: 'AccountTypeParentId' },

        // tree stuff
        Parent: {
          datatype: 'entity',
          control: 'AccountClassification', label: () => trx.instant('TreeParent'),
          foreignKeyName: 'ParentId'
        },
        ChildCount: {
          datatype: 'numeric',
          control: 'number', label: () => trx.instant('TreeChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
          alignment: 'right'
        },
        ActiveChildCount: {
          datatype: 'numeric',
          control: 'number', label: () => trx.instant('TreeActiveChildCount'), minDecimalPlaces: 0,
          maxDecimalPlaces: 0, alignment: 'right'
        },
        Level: {
          datatype: 'numeric',
          control: 'number', label: () => trx.instant('TreeLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
          alignment: 'right'
        },

        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
        ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
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
