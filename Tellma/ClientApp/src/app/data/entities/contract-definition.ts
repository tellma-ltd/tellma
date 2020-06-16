// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor, ChoicePropDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionState, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor, visibilityPropDescriptor } from './base/definition-common';
import { DefinitionVisibility as Visibility } from './base/definition-common';

export interface ContractDefinitionForSave extends EntityForSave {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;
    AgentVisibility?: Visibility;
    CurrencyVisibility?: Visibility;
    TaxIdentificationNumberVisibility?: Visibility;
    ImageVisibility?: Visibility;
    StartDateVisibility?: Visibility;
    StartDateLabel?: string;
    StartDateLabel2?: string;
    StartDateLabel3?: string;
    JobVisibility?: Visibility;
    BankAccountNumberVisibility?: Visibility;
    UserVisibility?: Visibility;
    AllowMultipleUsers?: boolean;
    MainMenuIcon?: string;
    MainMenuSection?: string;
    MainMenuSortKey?: number;
}

export interface ContractDefinition extends ContractDefinitionForSave {
    State?: DefinitionState;
    SavedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'TitleSingular' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_ContractDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;

        // clear the cache
        _cache = null;
    }

    if (!_cache) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'ContractDefinition',
            titleSingular: () => trx.instant('ContractDefinition'),
            titlePlural: () => trx.instant('ContractDefinitions'),
            select: _select,
            apiEndpoint: 'contract-definitions',
            screenUrl: 'contract-definitions',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] :
                ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { control: 'text', label: () => trx.instant('Code') },
                TitleSingular: { control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },

                AgentVisibility: visibilityPropDescriptor('Contract_Agent', trx),
                CurrencyVisibility: visibilityPropDescriptor('Contract_Currency', trx),
                TaxIdentificationNumberVisibility: visibilityPropDescriptor('Contract_TaxIdentificationNumber', trx),
                ImageVisibility: visibilityPropDescriptor('Image', trx),
                StartDateVisibility: visibilityPropDescriptor('Contract_StartDate', trx),
                StartDateLabel: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Contract_StartDate') }) + ws.primaryPostfix },
                StartDateLabel2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Contract_StartDate') }) + ws.secondaryPostfix },
                StartDateLabel3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Contract_StartDate') }) + ws.ternaryPostfix },
                JobVisibility: visibilityPropDescriptor('Contract_Job', trx),
                BankAccountNumberVisibility: visibilityPropDescriptor('Contract_BankAccountNumber', trx),
                UserVisibility: visibilityPropDescriptor('Contract_User', trx),
                AllowMultipleUsers: { control: 'boolean', label: () => trx.instant('ContractDefinition_AllowMultipleUsers')  },

                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),

                // IsActive & Audit info
                SavedById: {control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SavedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'SavedById' }
            }
        };

        // Remove multi-lingual properties if the tenant doesn't define the language
        const multiLangProps = ['TitleSingular', 'TitlePlural', 'StartDateLabel'];
        for (const prop of multiLangProps) {
            if (!ws.settings.SecondaryLanguageId) {
                delete entityDesc.properties[prop + '2'];
            }
            if (!ws.settings.TernaryLanguageId) {
                delete entityDesc.properties[prop + '3'];
            }
        }

        _cache = entityDesc;
    }

    return _cache;
}
