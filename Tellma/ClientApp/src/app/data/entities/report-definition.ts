// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { Collection, Control, EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionVisibility as Visibility, MainMenuSection, MainMenuIcon, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor } from './base/definition-common';

export type ReportOrderDirection = 'asc' | 'desc';
export type ReportType = 'Summary' | 'Details';
export type Aggregation = 'count' | 'sum' | 'avg' | 'max' | 'min';
export type ChartType = 'Card' | 'BarsVertical' | 'BarsVerticalGrouped' | 'BarsVerticalStacked' |
    'BarsVerticalNormalized' | 'BarsHorizontal' | 'BarsHorizontalGrouped' | 'BarsHorizontalStacked' |
    'BarsHorizontalNormalized' | 'Line' | 'Area' | 'AreaStacked' | 'AreaNormalized' | 'Pie' | 'Doughnut' |
    'HeatMap' | 'TreeMap' | 'NumberCards' | 'Gauge' | 'Radar';
export type Modifier = 'year' | 'quarter' | 'month' | 'dayofyear' | 'day' | 'week' | 'weekday';

export interface ReportDefinitionForSave<
    TParameter = ReportParameterDefinitionForSave,
    TRow = ReportRowDefinitionForSave,
    TColumn = ReportColumnDefinitionForSave,
    TMeasure = ReportMeasureDefinitionForSave,
    TSelect = ReportSelectDefinitionForSave> extends EntityForSave {

    Title?: string;
    Title2?: string;
    Title3?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    Type?: ReportType; // summary or details
    Chart?: ChartType;
    DefaultsToChart?: boolean; // ?
    Collection?: Collection;
    DefinitionId?: number;
    Filter?: string;
    OrderBy?: string;
    Top?: number;
    ShowColumnsTotal?: boolean;
    ShowRowsTotal?: boolean;
    MainMenuSection?: MainMenuSection;
    MainMenuIcon?: MainMenuIcon;
    MainMenuSortKey?: number;
    ShowInMainMenu?: boolean;

    Select?: TSelect[];
    Parameters?: TParameter[];
    Rows?: TRow[];
    Columns?: TColumn[];
    Measures?: TMeasure[];
}

export interface ReportDefinition extends ReportDefinitionForSave<
    ReportParameterDefinition, ReportRowDefinition, ReportColumnDefinition, ReportMeasureDefinition, ReportSelectDefinition> {
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

export interface ReportParameterDefinitionForSave extends EntityForSave {
    Key?: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: Visibility;
    Value?: string;
    Control?: Control;
    ControlOptions?: string; // JSON
}

// tslint:disable-next-line:no-empty-interface
export interface ReportParameterDefinition extends ReportParameterDefinitionForSave {
    ReportDefinitionId?: number;
}

export interface ReportSelectDefinitionForSave extends EntityForSave {
    Path?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportSelectDefinition extends ReportSelectDefinitionForSave {
    ReportDefinitionId?: number;
}

export interface ReportDimensionDefinition extends EntityForSave {
    Path?: string;
    Modifier?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    AutoExpand?: boolean;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportColumnDefinitionForSave extends ReportDimensionDefinition {
}

// tslint:disable-next-line:no-empty-interface
export interface ReportColumnDefinition extends ReportColumnDefinitionForSave {
    ReportDefinitionId?: number;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportRowDefinitionForSave extends ReportDimensionDefinition {

}

// tslint:disable-next-line:no-empty-interface
export interface ReportRowDefinition extends ReportRowDefinitionForSave {
    ReportDefinitionId?: number;
}

export interface ReportMeasureDefinitionForSave extends EntityForSave {
    Path?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    Aggregation?: Aggregation;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportMeasureDefinition extends ReportMeasureDefinitionForSave {
    ReportDefinitionId?: number;
}

const _select = ['', '2', '3'].map(pf => 'Title' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_ReportDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        _cache = {
            collection: 'ReportDefinition',
            titleSingular: () => trx.instant('ReportDefinition'),
            titlePlural: () => trx.instant('ReportDefinitions'),
            apiEndpoint: 'report-definitions',
            masterScreenUrl: 'report-definitions',
            select: _select,
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => (ws.getMultilingualValueImmediate(item, _select[0]) || trx.instant('Untitled')),
            properties: {
                Id: { datatype: 'integral', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Title: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.primaryPostfix },
                Title2: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.secondaryPostfix },
                Title3: { datatype: 'string', control: 'text', label: () => trx.instant('Title') + ws.ternaryPostfix },
                Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                Type: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('ReportDefinition_Type'),
                    choices: ['Summary', 'Details'],
                    format: (c: string) => trx.instant(`ReportDefinition_Type_${c}`)
                },
                Chart: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('ReportDefinition_Chart'),
                    choices: [ // Ordered by number of supported dimensions for ease of selection
                        // 0 Dimensions
                        'Card',
                        // 1 Dimension
                        'BarsVertical', 'BarsHorizontal', 'Pie', 'Doughnut', 'TreeMap', 'NumberCards', 'Gauge',
                        // 1 or 2 Dimensions
                        'Line', 'Area',
                        // 2 Dimensions
                        'BarsVerticalGrouped', 'BarsVerticalStacked', 'BarsVerticalNormalized', 'BarsHorizontalGrouped',
                        'BarsHorizontalStacked', 'BarsHorizontalNormalized', /* 'AreaStacked', 'AreaNormalized', 'Radar', */ 'HeatMap'],
                    format: (c: string) => trx.instant(`ReportDefinition_Chart_${c}`)
                },
                DefaultsToChart: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('ReportDefinition_DefaultsToChart') },
                Collection: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_Collection') },
                DefinitionId: { datatype: 'integral', control: 'number', label: () => trx.instant('ReportDefinition_DefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Filter: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_Filter') },
                OrderBy: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_OrderBy') },
                Top: { datatype: 'integral', control: 'number', label: () => trx.instant('ReportDefinition_Top'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ShowColumnsTotal: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('ReportDefinition_ShowColumnsTotal') },
                ShowRowsTotal: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('ReportDefinition_ShowRowsTotal') },
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),
                ShowInMainMenu: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('ReportDefinition_ShowInMainMenu') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete _cache.properties.Title2;
            delete _cache.properties.Description2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete _cache.properties.Title3;
            delete _cache.properties.Description3;
        }
    }

    return _cache;
}
