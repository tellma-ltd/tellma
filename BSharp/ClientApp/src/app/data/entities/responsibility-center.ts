// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';

export class ResponsibilityCenterForSave extends EntityForSave {
    ParentId: number;
    ResponsibilityCenterId: string;
    Name: string;
    Name2: string;
    Name3: string;
    ManagerId: number;
    IsOperatingSegment: boolean;
    Code: string;
    IsLeaf: boolean;
}

export class ResponsibilityCenter extends ResponsibilityCenterForSave {
    Level: number;
    ActiveChildCount: number;
    ChildCount: number;
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_ResponsibilityCenter(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;

        // clear the cache
        _cache = null;
    }

    if (!_cache) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'ResponsibilityCenter',
            titleSingular: () => trx.instant('ResponsibilityCenter'),
            titlePlural: () => trx.instant('ResponsibilityCenters'),
            select: _select,
            apiEndpoint: 'responsibility-centers',
            screenUrl: 'responsibility-centers',
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ParentId: {
                    control: 'number', label: () => `${trx.instant('TreeParent')} (${trx.instant('Id')})`,
                    minDecimalPlaces: 0, maxDecimalPlaces: 0
                },
                ResponsibilityTypeId: {
                    control: 'choice',
                    label: () => trx.instant('ResponsibilityCenter_ResponsibilityType'),
                    choices: ['Investment', 'Profit', 'Revenue', 'Cost'],
                    format: (c: string) => trx.instant(`ResponsibilityCenter_ResponsibilityType_${c}`)
                },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                ManagerId: {
                    control: 'number',
                    label: () => `${trx.instant('ResponsibilityCenter_Manager')} (${trx.instant('Id')})`,
                    minDecimalPlaces: 0,
                    maxDecimalPlaces: 0
                },
                IsOperatingSegment: { control: 'boolean', label: () => trx.instant('ResponsibilityCenter_IsOperatingSegment') },
                Code: { control: 'text', label: () => trx.instant('Code') },
                IsLeaf: { control: 'boolean', label: () => trx.instant('IsLeaf') },

                Manager: {
                    control: 'navigation', label: () => trx.instant('ResponsibilityCenter_Manager'), type: 'Agent',
                    foreignKeyName: 'ManagerId'
                },

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
                Parent: {
                    control: 'navigation', label: () => trx.instant('TreeParent'), type: 'ResponsibilityCenter',
                    foreignKeyName: 'ParentId'
                },

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
