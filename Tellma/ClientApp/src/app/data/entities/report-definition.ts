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
export type ChartType = 'Card' | 'BarsVertical' | 'BarsVerticalGrouped' | 'BarsVerticalStacked' |
    'BarsVerticalNormalized' | 'BarsHorizontal' | 'BarsHorizontalGrouped' | 'BarsHorizontalStacked' |
    'BarsHorizontalNormalized' | 'Line' | 'Area' | 'AreaStacked' | 'AreaNormalized' | 'Pie' | 'Doughnut' |
    'HeatMap' | 'TreeMap' | 'NumberCards' | 'Gauge' | 'Radar';

export interface ReportDefinitionForSave<
    TParameter = ReportDefinitionParameterForSave,
    TRow = ReportDefinitionRowForSave,
    TColumn = ReportDefinitionColumnForSave,
    TMeasure = ReportDefinitionMeasureForSave,
    TSelect = ReportDefinitionSelectForSave> extends EntityForSave {

    Title?: string;
    Title2?: string;
    Title3?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    Type?: ReportType; // summary or details
    Chart?: ChartType;
    DefaultsToChart?: boolean;
    ChartOptions?: string; // JSON
    Collection?: Collection;
    DefinitionId?: number;
    Filter?: string;
    Having?: string;
    OrderBy?: string;
    Top?: number;
    ShowColumnsTotal?: boolean;
    ColumnsTotalLabel?: string;
    ColumnsTotalLabel2?: string;
    ColumnsTotalLabel3?: string;
    ShowRowsTotal?: boolean;
    RowsTotalLabel?: string;
    RowsTotalLabel2?: string;
    RowsTotalLabel3?: string;
    IsCustomDrilldown?: boolean;
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
    ReportDefinitionParameter, ReportDefinitionRow, ReportDefinitionColumn, ReportDefinitionMeasure, ReportDefinitionSelect> {
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

export interface ReportDefinitionParameterForSave extends EntityForSave {
    Key?: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: Visibility;
    DefaultExpression?: string;
    Control?: Control;
    ControlOptions?: string; // JSON
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionParameter extends ReportDefinitionParameterForSave {
    ReportDefinitionId?: number;
}

export interface ReportDefinitionSelectForSave extends EntityForSave {
    Expression?: string;
    Localize?: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    Control?: Control;
    ControlOptions?: string; // JSON
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionSelect extends ReportDefinitionSelectForSave {
    ReportDefinitionId?: number;
}

export interface ReportDefinitionDimension<TAttribute> extends EntityForSave {
    KeyExpression?: string;
    DisplayExpression?: string;
    Localize?: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    AutoExpandLevel?: number;
    ShowAsTree?: boolean;
    Attributes?: TAttribute[];
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionColumnForSave extends ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave> {
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionColumn extends ReportDefinitionDimension<ReportDefinitionDimensionAttribute> {
    ReportDefinitionId?: number;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionRowForSave extends ReportDefinitionDimension<ReportDefinitionDimensionAttributeForSave> {
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionRow extends ReportDefinitionDimension<ReportDefinitionDimensionAttribute> {
    ReportDefinitionId?: number;
}

export interface ReportDefinitionDimensionAttributeForSave extends EntityForSave {
    Expression?: string;
    Localize?: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;

}

export interface ReportDefinitionDimensionAttribute extends ReportDefinitionDimensionAttributeForSave {
    ReportDefinitionDimensionId?: number;
}


export interface ReportDefinitionMeasureForSave extends EntityForSave {
    Expression?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    Control?: Control;
    ControlOptions?: string; // JSON
    DangerWhen?: string;
    WarningWhen?: string;
    SuccessWhen?: string;
}

// tslint:disable-next-line:no-empty-interface
export interface ReportDefinitionMeasure extends ReportDefinitionMeasureForSave {
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
            formatFromVals: (vals: any[]) => (ws.localize(vals[0], vals[1], vals[2]) || trx.instant('Untitled')),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
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
                    format: (c: string) => !!c ? trx.instant(`ReportDefinition_Chart_${c}`) : c
                },
                DefaultsToChart: { datatype: 'bit', control: 'check', label: () => trx.instant('ReportDefinition_DefaultsToChart') },
                ChartOptions: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_ChartOptions') },
                Collection: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_Collection') },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('ReportDefinition_DefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Filter: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_Filter') },
                Having: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_Having') },
                OrderBy: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_OrderBy') },
                Top: { datatype: 'numeric', control: 'number', label: () => trx.instant('ReportDefinition_Top'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
                ShowColumnsTotal: { datatype: 'bit', control: 'check', label: () => trx.instant('ReportDefinition_ShowColumnsTotal') },
                ColumnsTotalLabel: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_ColumnsTotalLabel') + ws.primaryPostfix },
                ColumnsTotalLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_ColumnsTotalLabel') + ws.secondaryPostfix },
                ColumnsTotalLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_ColumnsTotalLabel') + ws.ternaryPostfix },
                ShowRowsTotal: { datatype: 'bit', control: 'check', label: () => trx.instant('ReportDefinition_ShowRowsTotal') },
                RowsTotalLabel: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_RowsTotalLabel') + ws.primaryPostfix },
                RowsTotalLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_RowsTotalLabel') + ws.secondaryPostfix },
                RowsTotalLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('ReportDefinition_RowsTotalLabel') + ws.ternaryPostfix },
                IsCustomDrilldown: { datatype: 'bit', control: 'check', label: () => trx.instant('ReportDefinition_IsCustomDrilldown') },
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),
                ShowInMainMenu: { datatype: 'bit', control: 'check', label: () => trx.instant('ReportDefinition_ShowInMainMenu') },
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
