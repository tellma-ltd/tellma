// tslint:disable:variable-name
import { RoleMembership, RoleMembershipForSave } from './role-membership';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { supportedCultures } from '../supported-cultures';

export interface UserForSave<TRoleMembership = RoleMembershipForSave> extends EntityForSave {
  Image?: string;
  Name?: string;
  Name2?: string;
  Name3?: string;
  Email?: string;
  PreferredLanguage?: string;
  ContactEmail?: string;
  ContactMobile?: string;
  NormalizedContactMobile?: string;
  PreferredChannel?: string;
  EmailNewInboxItem?: boolean;
  SmsNewInboxItem?: boolean;
  PushNewInboxItem?: boolean;
  Roles?: TRoleMembership[];
}

export interface User extends UserForSave<RoleMembership> {
  PushEnabled?: boolean;
  ImageId?: string;
  IsActive?: boolean;
  ExternalId?: string;
  State?: 'New' | 'Confirmed';
  LastAccess?: string;
  CreatedAt?: string;
  CreatedById?: number | string;
  ModifiedAt?: string;
  ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_User(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    _settings = ws.settings;
    const companyLanguages = [ws.settings.PrimaryLanguageId];
    if (ws.settings.SecondaryLanguageId) {
      companyLanguages.push(ws.settings.SecondaryLanguageId);
    }
    if (ws.settings.TernaryLanguageId) {
      companyLanguages.push(ws.settings.TernaryLanguageId);
    }
    _cache = {
      collection: 'User',
      titleSingular: () => trx.instant('User'),
      titlePlural: () => trx.instant('Users'),
      select: _select,
      apiEndpoint: 'users',
      masterScreenUrl: 'users',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: UserForSave) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { datatype: 'integral', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Email: { datatype: 'string', control: 'text', label: () => trx.instant('User_Email') },
        PreferredLanguage: {
          datatype: 'string',
          control: 'choice',
          label: () => trx.instant('User_PreferredLanguage'),
          choices: companyLanguages,
          format: (c: string) => supportedCultures[c]
        },
        ContactEmail: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_ContactEmail') },
        ContactMobile: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_ContactMobile') },
        NormalizedContactMobile: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_NormalizedContactMobile') },
        PushEnabled: { datatype: 'bit', control: 'check', label: () => trx.instant('User_PushEnabled') },
        EmailNewInboxItem: { datatype: 'bit', control: 'check', label: () => trx.instant('User_EmailNewInboxItem') },
        SmsNewInboxItem: { datatype: 'bit', control: 'check', label: () => trx.instant('User_SmsNewInboxItem') },
        PushNewInboxItem: { datatype: 'bit', control: 'check', label: () => trx.instant('User_PushNewInboxItem') },
        State: {
          datatype: 'string',
          control: 'choice',
          label: () => trx.instant('State'),
          choices: ['New', 'Confirmed'],
          format: (c: string) => {
            switch (c) {
              case 'Invited': return trx.instant('User_Invited');
              case 'Member': return trx.instant('User_Member');
              default: return c;
            }
          },
          color: (c: string) => {
            switch (c) {
              case 'Invited': return '#6c757d';
              case 'Member': return '#28a745';
              default: return c;
            }
          }
        },
        LastAccess: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('User_LastActivity') },
        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
        ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
        ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
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
