import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { Permission, PermissionForSave } from './permission';
import { RoleMembershipForSave, RoleMembership } from './role-membership';
import { RequiredSignatureForSave, RequiredSignature } from './required-signature';
import { SettingsForClient } from './settings';
import { DtoDescriptor } from './metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { DtoKeyBase } from './dto-key-base';

export class RoleForSave<TPermission = PermissionForSave, TRequiredSignature = RequiredSignatureForSave,
    TRoleMembership = RoleMembershipForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Code: string;
    IsPublic: boolean;
    Permissions: TPermission[];
    Signatures: TRequiredSignature[];
    Members: TRoleMembership[];
}

export class Role extends RoleForSave<Permission, RequiredSignature, RoleMembership> {
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _cache: DtoDescriptor;

export function metadata_Role(ws: TenantWorkspace, trx: TranslateService, _subtype: string): DtoDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang && ws.settings !== _settings) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _cache = {
      select: _select,
      orderby: ws.isSecondaryLanguage ? `${_select[1]},${_select[0]}` : ws.isTernaryLanguage ? `${_select[2]},${_select[0]}` : _select[0],
      format: (item: DtoKeyBase) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },
        IsPublic: { control: 'boolean', label: trx.instant('Role_IsPublic') },
        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'LocalUser', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'LocalUser', foreignKeyName: 'ModifiedById' }
      }
    };
  }

  return _cache;
}
