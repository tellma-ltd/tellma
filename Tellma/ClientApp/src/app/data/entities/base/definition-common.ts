// tslint:disable:variable-name
import { TranslateService } from '@ngx-translate/core';
import { NumberPropDescriptor, ChoicePropDescriptor, StatePropDescriptor } from './metadata';
import { TenantWorkspace } from '../../workspace.service';

// tslint:disable:max-line-length
export type DefinitionVisibility = 'None' | 'Optional' | 'Required';
export type DefinitionCardinality = 'None' | 'Single' | 'Multiple';
export type DefinitionState = 'Hidden' | 'Visible' | 'Archived';
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

/**
 * Returns the PropDescriptor of a definition cardinality property, many definition
 * cardinality properties (e.g Unit Cardinality) have very similar descriptions
 */
export function cardinalityPropDescriptor(name: string, trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant(name),
        choices: ['None', 'Single', 'Multiple'],
        format: (c: string) => trx.instant('Cardinality_' + c)
    };
}
/**
 * Returns the PropDescriptor of a definition state property (Hidden, Visible or Archived)
 */
export function statePropDescriptor(trx: TranslateService): StatePropDescriptor {
    return {
        control: 'state',
        label: () => trx.instant('Definition_State'),
        choices: ['Hidden', 'Visible', 'Archived'],
        format: (c: string) => trx.instant('Definition_State_' + c),
        color: (c: string) => {
            switch (c) {
                case 'Hidden': return '#6c757d';
                case 'Visible': return '#28a745';
                case 'Archived': return '#dc3545';
                default: return null;
            }
        }
    };
}

export function lookupDefinitionIdPropDescriptor(name: string, ws: TenantWorkspace, trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('Field0Definition', { 0: trx.instant(name) }),
        choices: Object.keys(ws.definitions.Lookups).map(stringDefId => +stringDefId),
        format: (defId: number) => ws.getMultilingualValueImmediate(ws.definitions.Lookups[defId], 'TitlePlural')
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
          'arrows-alt',
          'asterisk',
          'balance-scale',
          'barcode',
          'bars',
          'bolt',
          'book',
          'box',
          'boxes',
          'building',
          'calendar',
          'calendar-alt',
          'camera-retro',
          'camera',
          'campground',
          'car-side',
          'car',
          'carrot',
          'cart-arrow-down',
          'cash-register',
          'certificate',
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
          'compress',
          'dolly-flatbed',
          'door-closed',
          'download',
          'drafting-compass',
          'edit',
          'ellipsis-h',
          'ellipsis-v',
          'euro-sign',
          'exchange-alt',
          'exclamation-triangle',
          'expand',
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
          'gas-pump',
          'gifts',
          'grin-hearts',
          'hammer',
          'hand-holding-usd',
          'hands-helping',
          'hands',
          'history',
          'holly-berry',
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
          'network-wired',
          'newspaper',
          'object-group',
          'paint-roller',
          'palette',
          'pallet',
          'paperclip',
          'parachute-box',
          'pen',
          'pills',
          'plus',
          'portrait',
          'power-off',
          'print',
          'project-diagram',
          'puzzle-piece',
          'recycle',
          'redo-alt',
          'ruler',
          'save',
          'scroll',
          'search',
          'search-dollar',
          'seedling',
          'shapes',
          'share-square',
          'share',
          'shield-alt',
          'ship',
          'shopping-cart',
          'sign',
          'sign-in-alt',
          'sign-out-alt',
          'sitemap',
          'spa',
          'spinner',
          'store-slash',
          'stream',
          'suitcase-rolling',
          'sync-alt',
          'table',
          'tags',
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
          'university',
          'undo',
          'undo-alt',
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
          'utensils',
          'warehouse',
          'wifi'
        ],
        format: (c: string) => c,
    };
}
