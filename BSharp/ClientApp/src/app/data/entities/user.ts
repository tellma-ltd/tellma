// tslint:disable:variable-name
import { RoleMembership, RoleMembershipForSave } from './role-membership';
import { EntityDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';

export class UserForSave<TRoleMembership = RoleMembershipForSave> extends EntityForSave {
  Email: string;
  Roles: TRoleMembership[];
}

export class User extends UserForSave<RoleMembership> {
  Name: string;
  Name2: string;
  Name3: string;
  ImageId: string;
  ExternalId: string;
  LastAccess: string;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_User(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;
    _cache = {
      collection: 'User',
      titleSingular: () => trx.instant('User'),
      titlePlural: () => trx.instant('Users'),
      select: _select,
      apiEndpoint: 'users',
      screenUrl: 'users',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: UserForSave) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Email: { control: 'text', label: () => trx.instant('User_Email') },
        Agent: { control: 'navigation', label: () => trx.instant('User_Agent'), type: 'Agent', foreignKeyName: 'Id' },
        State: {
          control: 'state',
          label: () => trx.instant('State'),
          choices: ['New', 'Confirmed'],
          format: (c: string) => {
            switch (c) {
              case 'New': return trx.instant('User_New');
              case 'Confirmed': return trx.instant('User_Confirmed');
              default: return c;
            }
          },
          color: (c: string) => {
            switch (c) {
              case 'New': return '#6c757d';
              case 'Confirmed': return '#28a745';
              default: return c;
            }
          }
        },
        LastAccess: { control: 'datetime', label: () => trx.instant('User_LastActivity') },
        CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
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
