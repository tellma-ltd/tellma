// tslint:disable:variable-name
import { TranslateService } from '@ngx-translate/core';
import { NumberPropDescriptor, ChoicePropDescriptor } from './metadata';
import { TenantWorkspace } from '../../workspace.service';

// tslint:disable:max-line-length
export type DefinitionVisibility = 'None' | 'Optional' | 'Required';
export type DefinitionCardinality = 'None' | 'Single' | 'Multiple';
export type DefinitionState = 'Draft' | 'Deployed' | 'Archived';
export type MainMenuSection = 'Financials' | 'Administration'; // TODO
export type MainMenuIcon = 'clipboard' | 'chart-pie'; // TODO

/**
 * Returns the PropDescriptor of a definition visibility property, many definition
 * visibility properties (e.g Currency Visibility) have very similar descriptions
 */
export function visibilityPropDescriptor(name: string, trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('Field0Visibility', { 0: trx.instant(name) }),
        choices: ['None', 'Optional', 'Required'],
        format: (c: string) => trx.instant('Visibility_' + c)
    };
}

export function lookupDefinitionIdPropDescriptor(name: string, ws: TenantWorkspace, trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('Field0Definition', { 0: trx.instant(name) }),
        choices: Object.keys(ws.definitions.Lookups).map(stringDefId => +stringDefId),
        format: (defId: string) => ws.getMultilingualValueImmediate(ws.definitions.Lookups[defId], 'TitlePlural')
    };
}

export function mainMenuSectionPropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('MainMenuSection'),
        choices: ['Mail', 'Financials', 'Cash', 'FixedAssets', 'Inventory', 'Production', 'Purchasing', 'Sales', 'HumanCapital', 'Investments', 'Maintenance', 'Administration', 'Security', 'Studio', 'Help'],
        format: (c: string) => trx.instant('Menu_' + c)
    };
}

export function mainMenuSortKeyPropDescriptor(trx: TranslateService): NumberPropDescriptor {
    return { control: 'number', label: () => trx.instant('MainMenuSortKey'), minDecimalPlaces: 0, maxDecimalPlaces: 0 };
}

export function mainMenuIconPropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('MainMenuIcon'),
        choices: [
          'anchor',
          'angle-double-left',
          'angle-left',
          'angle-right',
          'archive',
          'arrow-circle-right',
          'asterisk',
          'balance-scale',
          'bars',
          'book',
          'boxes',
          'building',
          'calendar',
          'camera-retro',
          'camera',
          'car-side',
          'car',
          'cash-register',
          'chart-area',
          'chart-bar',
          'chart-line',
          'chart-pie',
          'check',
          'city',
          'clipboard-check',
          'clipboard',
          'code-branch',
          'code',
          'cog',
          'cogs',
          'coins',
          'copy',
          'cube',
          'dolly-flatbed',
          'download',
          'edit',
          'ellipsis-h',
          'ellipsis-v',
          'euro-sign',
          'exchange-alt',
          'exclamation-triangle',
          'external-link-alt',
          'fax',
          'female',
          'file-alt',
          'file-archive',
          'file-audio',
          'file-contract',
          'file-download',
          'file-excel',
          'file-export',
          'file-image',
          'file-import',
          'file-invoice-dollar',
          'file-pdf',
          'file-powerpoint',
          'file-video',
          'file-word',
          'file',
          'filter',
          'folder-minus',
          'folder-plus',
          'folder',
          'font',
          'funnel-dollar',
          'grin-hearts',
          'hand-holding-usd',
          'hands-helping',
          'hands',
          'history',
          'home',
          'id-badge',
          'image',
          'inbox',
          'indent',
          'industry',
          'info-circle',
          'landmark',
          'laptop-code',
          'laptop',
          'list-ul',
          'list',
          'lock',
          'male',
          'map-marker-alt',
          'map',
          'microchip',
          'minus-square',
          'money-bill-wave',
          'money-check-alt',
          'money-check',
          'newspaper',
          'object-group',
          'palette',
          'pallet',
          'paperclip',
          'pen',
          'pills',
          'plus',
          'portrait',
          'power-off',
          'print',
          'project-diagram',
          'recycle',
          'redo-alt',
          'ruler',
          'save',
          'scroll',
          'search',
          'seedling',
          'share-square',
          'share',
          'ship',
          'shopping-cart',
          'sign-in-alt',
          'sign-out-alt',
          'sitemap',
          'spa',
          'spinner',
          'stream',
          'suitcase-rolling',
          'sync-alt',
          'table',
          'tasks',
          'th-large',
          'thumbs-down',
          'thumbs-up',
          'times',
          'tint-slash',
          'tint',
          'tools',
          'tractor',
          'trash',
          'tree',
          'trophy',
          'truck',
          'umbrella-beach',
          'undo',
          'upload',
          'user-check',
          'user-clock',
          'user-cog',
          'user-friends',
          'user-minus',
          'user-plus',
          'user-shield',
          'user-tag',
          'user-tie',
          'user',
          'users-cog',
          'users',
          'warehouse',
          'wifi'
        ],
        format: (c: string) => c,
    };
}
