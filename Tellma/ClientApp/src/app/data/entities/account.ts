// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService, TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export interface AccountForSave extends EntityWithKey {
    CenterId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    AccountTypeId?: string;
    ClassificationId?: number;
    CustodianId?: number;
    CustodyDefinitionId?: number;
    CustodyId?: number;
    ParticipantId?: number;
    ResourceDefinitionId?: number;
    ResourceId?: number;
    CurrencyId?: string;
    EntryTypeId?: number;
}

export interface Account extends AccountForSave {
    IsActive?: boolean;
    IsBusinessUnit?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf).concat(['Code']);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

function format(item: Account, ws: TenantWorkspace) {
    let result = ws.getMultilingualValueImmediate(item, _select[0]);
    if (!!item.Code) {
        result = `${item.Code} - ${result}`;
    }

    return result;
}

export function metadata_Account(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'Account',
            titleSingular: () => trx.instant('Account'),
            titlePlural: () => trx.instant('Accounts'),
            select: _select,
            apiEndpoint: 'accounts',
            masterScreenUrl: 'accounts',
            orderby: () => ['Code'].concat(ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]]),
            inactiveFilter: 'IsActive eq true',
            format: (item: Account) => format(item, ws),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CenterId: { control: 'number', label: () => `${trx.instant('Account_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Account_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: () => trx.instant('Code') },
                AccountTypeId: { control: 'number', label: () => `${trx.instant('Account_Type')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AccountType: { control: 'navigation', label: () => trx.instant('Account_Type'), type: 'AccountType', foreignKeyName: 'AccountTypeId' },
                ClassificationId: { control: 'number', label: () => `${trx.instant('Account_Classification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Classification: { control: 'navigation', label: () => trx.instant('Account_Classification'), type: 'AccountClassification', foreignKeyName: 'ClassificationId' },
                CustodianId: { control: 'number', label: () => `${trx.instant('Account_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custodian: {control: 'navigation', label: () => trx.instant('Account_Custodian'), type: 'Relation', foreignKeyName: 'CustodianId'},
                CustodyDefinitionId: {
                    control: 'choice',
                    label: () => trx.instant('Account_CustodyDefinition'),
                    choices: Object.keys(ws.definitions.Custodies).map(stringDefId => +stringDefId),
                    format: (defId: string) => ws.getMultilingualValueImmediate(ws.definitions.Custodies[defId], 'TitlePlural')
                },
                CustodyDefinition: { control: 'navigation', label: () => trx.instant('Account_CustodyDefinition'), type: 'CustodyDefinition', foreignKeyName: 'CustodyDefinitionId' },
                CustodyId: { control: 'number', label: () => `${trx.instant('Account_Custody')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custody: { control: 'navigation', label: () => trx.instant('Account_Custody'), type: 'Custody', foreignKeyName: 'CustodyId' },
                ParticipantId: { control: 'number', label: () => `${trx.instant('Account_Participant')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Participant: {control: 'navigation', label: () => trx.instant('Account_Participant'), type: 'Relation', foreignKeyName: 'ParticipantId'},
                ResourceDefinitionId: {
                    control: 'choice',
                    label: () => trx.instant('Account_ResourceDefinition'),
                    choices: Object.keys(ws.definitions.Resources).map(stringDefId => +stringDefId),
                    format: (defId: string) => ws.getMultilingualValueImmediate(ws.definitions.Resources[defId], 'TitlePlural')
                },
                ResourceDefinition: { control: 'navigation', label: () => trx.instant('Account_ResourceDefinition'), type: 'ResourceDefinition', foreignKeyName: 'ResourceDefinitionId' },
                ResourceId: { control: 'number', label: () => `${trx.instant('Account_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: () => trx.instant('Account_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Account_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Account_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                EntryTypeId: { control: 'number', label: () => `${trx.instant('Account_EntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                EntryType: { control: 'navigation', label: () => trx.instant('Account_EntryType'), type: 'EntryType', foreignKeyName: 'EntryTypeId' },
                IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
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
