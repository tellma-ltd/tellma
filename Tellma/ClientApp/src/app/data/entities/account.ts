// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export interface AccountForSave extends EntityWithKey {
    CenterId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    AccountTypeId?: string;
    IsCurrent?: boolean;
    LegacyClassificationId?: number;
    LegacyTypeId?: string;
    AgentDefinitionId?: string;
    HasResource?: boolean;
    IsRelated?: boolean;
    HasExternalReference?: boolean;
    HasAdditionalReference?: boolean;
    HasNotedAgentId?: boolean;
    HasNotedAgentName?: boolean;
    HasNotedAmount?: boolean;
    HasNotedDate?: boolean;
    AgentId?: number;
    ResourceId?: number;
    CurrencyId?: string;
    Identifier?: string;
    EntryTypeId?: number;
}

export interface Account extends AccountForSave {
    IsDeprecated?: boolean;
    IsActive?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: EntityDescriptor;

export function metadata_Account(wss: WorkspaceService, trx: TranslateService, _: string): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings || ws.definitions !== _definitions) {
        _settings = ws.settings;
        _definitions = ws.definitions;
        const entityDesc: EntityDescriptor = {
            collection: 'Account',
            titleSingular: () => trx.instant('Account'),
            titlePlural: () => trx.instant('Accounts'),
            select: _select,
            apiEndpoint: 'accounts',
            screenUrl: 'accounts',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CenterId: { control: 'number', label: () => `${trx.instant('Account_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Account_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: () => trx.instant('Code') },
                AccountTypeId: { control: 'number', label: () => `${trx.instant('Account_Type')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0  },
                AccountType: { control: 'navigation', label: () => trx.instant('Account_Type'), type: 'AccountType', foreignKeyName: 'AccountTypeId' },
                IsCurrent: { control: 'boolean', label: () => trx.instant('Account_IsCurrent') },
                LegacyClassificationId: { control: 'number', label: () => `${trx.instant('Account_LegacyClassification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LegacyClassification: { control: 'navigation', label: () => trx.instant('Account_LegacyClassification'), type: 'LegacyClassification', foreignKeyName: 'LegacyClassificationId' },
                LegacyTypeId: { control: 'text', label: () => `${trx.instant('Account_LegacyType')} (${trx.instant('Id')})` },
                LegacyType: { control: 'navigation', label: () => trx.instant('Account_LegacyType'), type: 'LegacyType', foreignKeyName: 'LegacyTypeId' },

                // AgentDefinitionId: { control: 'text', label: () => `${trx.instant('Account_AgentDefinition')} (${trx.instant('Id')})` },
                // AgentDefinition: { control: 'navigation', label: () => trx.instant('Account_AgentDefinition'), type: 'AgentDefinition', foreignKeyName: 'AgentDefinitionId' },
                AgentDefinitionId: {
                    control: 'choice',
                    label: () => trx.instant('Account_AgentDefinition'),
                    choices: Object.keys(ws.definitions.Agents),
                    format: (defId: string) => ws.getMultilingualValueImmediate(ws.definitions.Agents[defId], 'TitlePlural')
                },

                HasResource: { control: 'boolean', label: () => trx.instant('Account_HasResource') },
                IsRelated: { control: 'boolean', label: () => trx.instant('Account_IsRelated') },
                HasExternalReference: { control: 'boolean', label: () => trx.instant('Account_HasExternalReference') },
                HasAdditionalReference: { control: 'boolean', label: () => trx.instant('Account_HasAdditionalReference') },
                HasNotedAgentId: { control: 'boolean', label: () => trx.instant('Account_HasNotedAgentId') },
                HasNotedAgentName: { control: 'boolean', label: () => trx.instant('Account_HasNotedAgentName') },
                HasNotedAmount: { control: 'boolean', label: () => trx.instant('Account_HasNotedAmount') },
                HasNotedDate: { control: 'boolean', label: () => trx.instant('Account_HasNotedDate') },
                AgentId: { control: 'number', label: () => `${trx.instant('Account_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent: { control: 'navigation', label: () => trx.instant('Account_Agent'), type: 'Agent', foreignKeyName: 'AgentId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Account_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Account_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Account_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Account_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                Identifier: { control: 'text', label: () => trx.instant('Account_Identifier') },
                EntryTypeId: { control: 'number', label: () => `${trx.instant('Account_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { control: 'navigation', label: () => trx.instant('Account_EntryType'), type: 'EntryType', foreignKeyName: 'EntryTypeId' },
                IsDeprecated: { control: 'boolean', label: () => trx.instant('Account_IsDeprecated') },
                CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
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
