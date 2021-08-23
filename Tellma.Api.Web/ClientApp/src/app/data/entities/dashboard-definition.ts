// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';
import { MainMenuSection, MainMenuIcon, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor } from './base/definition-common';
import { TimeGranularity } from './base/metadata-types';

export interface DashboardDefinitionForSave<
    TWidget = DashboardDefinitionWidgetForSave,
    TRole = DashboardDefinitionRoleForSave> extends EntityForSave {

    Code?: string;
    Title?: string;
    Title2?: string;
    Title3?: string;
    AutoRefreshPeriodInMinutes?: number;
    MainMenuSection?: MainMenuSection;
    MainMenuIcon?: MainMenuIcon;
    MainMenuSortKey?: number;

    Widgets?: TWidget[];
    Roles?: TRole[];
}

export interface DashboardDefinition extends DashboardDefinitionForSave<
        DashboardDefinitionWidget, DashboardDefinitionRole> {
    ShowInMainMenu?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

export interface DashboardDefinitionWidgetForSave extends EntityForSave {
    ReportDefinitionId?: number;
    OffsetX?: number;
    OffsetY?: number;
    Width?: number;
    Height?: number;
    Title?: string;
    Title2?: string;
    Title3?: string;
    AutoRefreshPeriodInMinutes?: number;
}

// tslint:disable-next-line:no-empty-interface
export interface DashboardDefinitionWidget extends DashboardDefinitionWidgetForSave {
    DashboardDefinitionId?: number;
}

export interface DashboardDefinitionRoleForSave extends EntityForSave {
    RoleId?: number;
}

// tslint:disable-next-line:no-empty-interface
export interface DashboardDefinitionRole extends DashboardDefinitionRoleForSave {
    DashboardDefinitionId?: number;
}


const _select = ['', '2', '3'].map(pf => 'Title' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_DashboardDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        _cache = {
            collection: 'DashboardDefinition',
            titleSingular: () => trx.instant('DashboardDefinition'),
            titlePlural: () => trx.instant('DashboardDefinitions'),
            apiEndpoint: 'dashboard-definitions',
            masterScreenUrl: 'dashboard-definitions',
            select: _select,
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null,
            format: (item: EntityWithKey) => (ws.getMultilingualValueImmediate(item, _select[0]) || trx.instant('Untitled')),
            formatFromVals: (vals: any[]) => (ws.localize(vals[0], vals[1], vals[2]) || trx.instant('Untitled')),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                Title: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.primaryPostfix },
                Title2: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.secondaryPostfix },
                Title3: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.ternaryPostfix },
                AutoRefreshPeriodInMinutes: { datatype: 'numeric', control: 'number', label: () => trx.instant('DashboardDefinition_AutoRefreshPeriodInMinutes'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),
                ShowInMainMenu: { datatype: 'bit', control: 'check', label: () => trx.instant('Definition_ShowInMainMenu') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete _cache.properties.Title2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete _cache.properties.Title3;
        }
    }

    return _cache;
}
