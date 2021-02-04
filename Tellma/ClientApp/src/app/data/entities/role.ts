// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { Permission, PermissionForSave } from './permission';
import { RoleMembershipForSave, RoleMembership } from './role-membership';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { SettingsForClient } from '../dto/settings-for-client';

export interface RoleForSave<TPermission = PermissionForSave,
    TRoleMembership = RoleMembershipForSave> extends EntityForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    IsPublic?: boolean;
    Permissions?: TPermission[];
    Members?: TRoleMembership[];
}

export interface Role extends RoleForSave<Permission, RoleMembership> {
    IsActive?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Role(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;
    _cache = {
      collection: 'Role',
      titleSingular: () => trx.instant('Role'),
      titlePlural:  () => trx.instant('Roles'),
      select: _select,
      apiEndpoint: 'roles',
      masterScreenUrl: 'roles',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
      properties: {
        Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        IsPublic: { datatype: 'bit', control: 'check', label: () => trx.instant('Role_IsPublic') },
        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
        SavedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'SavedById' }
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
