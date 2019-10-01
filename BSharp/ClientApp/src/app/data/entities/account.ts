// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export class AccountForSave extends EntityWithKey {
    AccountTypeId: string;
    AccountClassificationId: number;
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    PartyReference: string;
    ResponsibilityCenterId: number;
    CustodianId: number;
    ResourceId: number;
    LocationId: number;
}

export class Account extends AccountForSave {
    AccountDefinitionId: string;
    IsDeprecated: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: string]: EntityDescriptor } = {};

export function metadata_Account(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (trx.currentLang !== _currentLang || ws.settings !== _settings || ws.definitions !== _definitions) {
        _currentLang = trx.currentLang;
        _settings = ws.settings;
        _definitions = ws.definitions;

        // clear the cache
        _cache = {};
    }

    definitionId = definitionId || '_'; // undefined
    if (!_cache[definitionId]) {
        const entityDesc: EntityDescriptor = {
            titleSingular: ws.getMultilingualValueImmediate(ws.definitions.Accounts[definitionId], 'TitleSingular') || '???',
            titlePlural: ws.getMultilingualValueImmediate(ws.definitions.Accounts[definitionId], 'TitlePlural') || '???',
            select: _select,
            apiEndpoint: 'accounts/' + (definitionId || ''),
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            definitionFunc: (e: Account) => e.AccountDefinitionId,
            selectForDefinition: 'AccountDefinitionId',
            properties: {
                Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AccountTypeId: { control: 'text', label: trx.instant('Account_Type') },
                AccountType: { control: 'navigation', label: trx.instant('Account_Type'), type: 'AccountType', definition: definitionId, foreignKeyName: 'AccountTypeId' },
                AccountClassificationId: { control: 'number', label: trx.instant('Account_Classification'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AccountClassification: { control: 'navigation', label: trx.instant('Account_Classification'), type: 'AccountClassification', definition: definitionId, foreignKeyName: 'AccountClassificationId' },
                Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: trx.instant('Code') },

                PartyReference: { control: 'text', label: trx.instant('Account_PartyReference') },
                ResponsibilityCenterId: { control: 'text', label: `${trx.instant('Account_ResponsibilityCenter')} (${trx.instant('Id')})` },
                ResponsibilityCenter: { control: 'navigation', label: trx.instant('Account_ResponsibilityCenter'), type: 'ResponsibilityCenter', foreignKeyName: 'ResponsibilityCenterId' },
                CustodianId: { control: 'number', label: `${trx.instant('Account_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Custodian: { control: 'navigation', label: trx.instant('Account_Custodian'), type: 'Agent', foreignKeyName: 'CustodianId' },
                ResourceId: { control: 'number', label: `${trx.instant('Account_Resource')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource: { control: 'navigation', label: trx.instant('Account_Resource'), type: 'Resource', foreignKeyName: 'ResourceId' },
                LocationId: { control: 'number', label: `${trx.instant('Account_Location')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Location: { control: 'navigation', label: trx.instant('Account_Location'), type: 'Location', foreignKeyName: 'LocationId' },

                IsDeprecated: { control: 'boolean', label: trx.instant('Account_IsDeprecated') },
                CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
        }

        // Adjust according to definitions
        const definition = _definitions.Accounts[definitionId];
        if (!definition) {
            if (definitionId !== '<generic>') {
                // Programmer mistake
                console.error(`defintionId '${definitionId}' doesn't exist`);
            }
        } else {
            entityDesc.titleSingular = ws.getMultilingualValueImmediate(ws.definitions.Accounts[definitionId], 'TitleSingular') || '???';
            entityDesc.titlePlural = ws.getMultilingualValueImmediate(ws.definitions.Accounts[definitionId], 'TitlePlural') || '???';


            for (const propName of ['PartyReference']) {
                if (!definition[propName + '_Visibility']) {
                    delete entityDesc.properties[propName];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.label = ws.getMultilingualValueImmediate(definition, propName + '_Label') || propDesc.label;
                }
            }

            for (const propName of ['ResponsibilityCenter', 'Custodian', 'Resource', 'Location']) {
                if (!definition[propName + '_Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.label = ws.getMultilingualValueImmediate(definition, propName + '_Label') || propDesc.label;

                    // Specify the definitions
                    const defList = definition[propName + '_DefinitionList'] as string;
                    if (!!defList) {
                        propDesc.definitions = defList.split(',');
                        if (propDesc.definitions.length === 1) {
                            propDesc.definition = propDesc.definitions[0];
                        }
                    }
                }
            }
        }

        _cache[definitionId] = entityDesc;
    }

    return _cache[definitionId];
}
