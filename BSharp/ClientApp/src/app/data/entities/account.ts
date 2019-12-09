// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export class AccountForSave extends EntityWithKey {
    AccountTypeId: string;
    AccountClassificationId: number;
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    IsSmart: boolean;
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
                AccountTypeId: { control: 'text', label: () => trx.instant('Account_Type') },
                AccountType: { control: 'navigation', label: () => trx.instant('Account_Type'), type: 'AccountType', foreignKeyName: 'AccountTypeId' },
                AccountClassificationId: { control: 'number', label: () => trx.instant('Account_Classification'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AccountClassification: { control: 'navigation', label: () => trx.instant('Account_Classification'), type: 'AccountClassification', foreignKeyName: 'AccountClassificationId' },

                // PartyReference: { control: 'text', label: () => trx.instant('Account_PartyReference') },
                // ResponsibilityCenterId: { control: 'text', label: () => `${trx.instant('Account_ResponsibilityCenter')} (${trx.instant('Id')})` },
                // ResponsibilityCenter: { control: 'navigation', label: () => trx.instant('Account_ResponsibilityCenter'), type: 'ResponsibilityCenter', foreignKeyName: 'ResponsibilityCenterId' },
                // CustodianId: { control: 'number', label: () => `${trx.instant('Account_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Custodian: { control: 'navigation', label: () => trx.instant('Account_Custodian'), type: 'Agent', foreignKeyName: 'CustodianId' },
                // ResourceId: { control: 'number', label: () => `${trx.instant('Account_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Resource: { control: 'navigation', label: () => trx.instant('Account_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },

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
