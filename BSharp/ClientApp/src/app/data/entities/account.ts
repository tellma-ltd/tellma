// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export class AccountForSave extends EntityWithKey {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    IsSmart: boolean;
    AccountTypeId: string;
    AccountClassificationId: number;
    CurrencyId: string;

    ResponsibilityCenterId: number;
    ContractType: string;
    AgentDefinitionId: string;
    ResourceClassificationId: number;
    IsCurrent: boolean;
    AgentId: number;
    ResourceId: number;
    Identifier: string;
    EntryClassificationId: number;
}

export class Account extends AccountForSave {
    IsDeprecated: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_Account(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'Account',
            titleSingular: () => trx.instant('Account'),
            titlePlural: () => trx.instant('Accounts'),
            select: _select,
            apiEndpoint: 'accounts',
            screenUrl: 'accounts',
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: () => trx.instant('Code') },
                IsSmart: { control: 'boolean', label: () => trx.instant('Account_IsSmart') },
                AccountTypeId: { control: 'text', label: () => `${trx.instant('Account_Type')} (${trx.instant('Id')})` },
                AccountType: { control: 'navigation', label: () => trx.instant('Account_Type'), type: 'AccountType', foreignKeyName: 'AccountTypeId' },
                AccountClassificationId: { control: 'number', label: () => `${trx.instant('Account_Classification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AccountClassification: { control: 'navigation', label: () => trx.instant('Account_Classification'), type: 'AccountClassification', foreignKeyName: 'AccountClassificationId' },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Account_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Account_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                ResponsibilityCenterId: { control: 'number', label: () => `${trx.instant('Account_ResponsibilityCenter')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResponsibilityCenter: { control: 'navigation', label: () => trx.instant('Account_ResponsibilityCenter'), type: 'ResponsibilityCenter', foreignKeyName: 'ResponsibilityCenterId' },
                ContractType: {
                    control: 'choice',
                    label: () => trx.instant('Account_ContractType'),
                    choices: ['OnHand', 'InTransit', 'Receivable', 'Deposit', 'Loan', 'AccruedIncome',
                        'Equity', 'AccruedExpense', 'Payable', 'Retention', 'Borrowing', 'Revenue', 'Expense'],
                    format: (c: string) => trx.instant('Account_ContractType_' + c)
                },
                AgentDefinitionId: { control: 'text', label: () => `${trx.instant('Account_AgentDefinition')} (${trx.instant('Id')})` },
                AgentDefinition: { control: 'navigation', label: () => trx.instant('Account_AgentDefinition'), type: 'AgentDefinition', foreignKeyName: 'AgentDefinitionId' },
                ResourceClassificationId: { control: 'number', label: () => `${trx.instant('Account_ResourceClassification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceClassification: { control: 'navigation', label: () => trx.instant('Account_ResourceClassification'), type: 'ResourceClassification', foreignKeyName: 'ResourceClassificationId' },
                IsCurrent: { control: 'boolean', label: () => trx.instant('Account_IsCurrent') },
                AgentId: { control: 'number', label: () => `${trx.instant('Account_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent: { control: 'navigation', label: () => trx.instant('Account_Agent'), type: 'Agent', foreignKeyName: 'AgentId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Account_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Account_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                Identifier: { control: 'text', label: () => trx.instant('Account_Identifier') },
                EntryClassificationId: { control: 'number', label: () => `${trx.instant('Account_EntryClassification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryClassification: { control: 'navigation', label: () => trx.instant('Account_EntryClassification'), type: 'EntryClassification', foreignKeyName: 'EntryClassificationId' },
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
