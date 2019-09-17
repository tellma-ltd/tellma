// tslint:disable:variable-name
import { Permission, PermissionForSave } from './permission';
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';

export class ViewForSave<TPermission = PermissionForSave> extends EntityForSave {
    Permissions: TPermission[];
}

export class View extends ViewForSave<Permission> {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    IsActive: boolean;
    Actions: ViewAction[];
    // AllowedPermissionLevels: ('Read' | 'Update' | 'Create' | 'ReadCreate' | 'Sign')[];
}

export class ViewAction extends EntityForSave {
    ViewId: string;
    Action: string;
    SupportsCriteria: boolean;
    SupportsMask: boolean;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_View(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang || ws.settings !== _settings) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _cache = {
      select: _select,
      apiEndpoint: 'views',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },
        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete _cache.properties.Name2;
    }

    if (!ws.settings.TernaryLanguageId) {
      delete _cache.properties.Name3;
    }
  }

  return _cache;
}
