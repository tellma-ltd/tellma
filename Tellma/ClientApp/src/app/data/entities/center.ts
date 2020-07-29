// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';

export interface CenterForSave extends EntityForSave {
    ParentId?: number;
    CenterType?: string;
    Name?: string;
    Name2?: string;
    Name3?: string;
    ManagerId?: number;
    Code?: string;
}

export interface Center extends CenterForSave {
    IsSegment?: boolean;
    Level?: number;
    ActiveChildCount?: number;
    ChildCount?: number;
    IsLeaf?: boolean;
    IsActive?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_Center(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
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
            collection: 'Center',
            titleSingular: () => trx.instant('Center'),
            titlePlural: () => trx.instant('Centers'),
            select: _select,
            apiEndpoint: 'centers',
            masterScreenUrl: 'centers',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] :
                ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: 'IsActive eq true',
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CenterType: {
                    control: 'choice',
                    label: () => trx.instant('Center_CenterType'),
                    choices: [
                        'Abstract',
                        'BusinessUnit',
                        'CostOfSales',
                        'SellingGeneralAndAdministration',
                        'SharedExpenseControl',
                        'CurrentInventoriesInTransitExpendituresControl',
                        'ConstructionInProgressExpendituresControl',
                        'WorkInProgressExpendituresControl'
                    ],
                    format: (c: string) => !!c ? trx.instant(`Center_CenterType_${c}`) : ''
                },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                ManagerId: { control: 'number', label: () => `${trx.instant('Center_Manager')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Manager: { control: 'navigation', label: () => trx.instant('Center_Manager'), type: 'Agent', foreignKeyName: 'ManagerId' },
                Code: { control: 'text', label: () => trx.instant('Code') },
                IsSegment: { control: 'boolean', label: () => trx.instant('Center_IsSegment')},

                // Tree stuff
                Level: {
                    control: 'number', label: () => trx.instant('TreeLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
                    alignment: 'right'
                },
                ActiveChildCount: {
                    control: 'number', label: () => trx.instant('TreeActiveChildCount'), minDecimalPlaces: 0,
                    maxDecimalPlaces: 0, alignment: 'right'
                },
                ChildCount: {
                    control: 'number', label: () => trx.instant('TreeChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
                    alignment: 'right'
                },
                ParentId: { control: 'number', label: () => `${trx.instant('TreeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Parent: {
                    control: 'navigation', label: () => trx.instant('TreeParent'), type: 'Center',
                    foreignKeyName: 'ParentId'
                },
                IsLeaf: { control: 'boolean', label: () => trx.instant('IsLeaf') },

                // IsActive & Audit info
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
