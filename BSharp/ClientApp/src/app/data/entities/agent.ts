// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { supportedCultures } from '../supported-cultures';

export class AgentForSave extends EntityWithKey {

  Name: string;
  Name2: string;
  Name3: string;
  Code: string;
  AgentType: string;
  IsRelated: boolean;
  PreferredLanguage: string;
  Image: string;
}

export class Agent extends AgentForSave {
  ImageId: string;
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

// export const Agent_AgentType = {
//   'Individual': 'Agent_AgentType_Individual',
//   'Organization': 'Agent_AgentType_Organization',
//   'System': 'Agent_AgentType_System'
// };

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Agent(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings) {
    const companyLanguages = [ws.settings.PrimaryLanguageId];
    if (ws.settings.SecondaryLanguageId) {
      companyLanguages.push(ws.settings.SecondaryLanguageId);
    }
    if (ws.settings.TernaryLanguageId) {
      companyLanguages.push(ws.settings.TernaryLanguageId);
    }
    _settings = ws.settings;
    _cache = {
      collection: 'Agent',
      titleSingular: () => trx.instant('Agent'),
      titlePlural:  () => trx.instant('Agents'),
      select: _select,
      apiEndpoint: 'agents',
      screenUrl: 'agents',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: () => trx.instant('Code') },
     //   User: { control: 'navigation', label: () => trx.instant('Agent_User'), type: 'User', foreignKeyName: 'Id' },
        AgentType: {
          control: 'choice',
          label: () => trx.instant('Agent_AgentType'),
          choices: ['Individual', 'Organization', 'System'],
          format: (c: string) => {
            switch (c) {
              case 'Individual': return trx.instant('Agent_AgentType_Individual');
              case 'Organization': return trx.instant('Agent_AgentType_Organization');
              case 'System': return trx.instant('Agent_AgentType_System');
              default: return c;
            }
          }
        },
        PreferredLanguage: {
          control: 'choice',
          label: () => trx.instant('Agent_PreferredLanguage'),
          choices: companyLanguages,
          format: (c: string) => supportedCultures[c]
        },
        IsRelated: { control: 'boolean', label: () => trx.instant('Agent_IsRelated') },
        IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
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
