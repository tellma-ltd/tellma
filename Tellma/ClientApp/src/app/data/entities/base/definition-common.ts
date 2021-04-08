// tslint:disable:variable-name
import { TranslateService } from '@ngx-translate/core';
import { NumberPropDescriptor, ChoicePropDescriptor, NavigationPropDescriptor } from './metadata';

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
        datatype: 'string',
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
        datatype: 'string',
        control: 'choice',
        label: () => trx.instant(name),
        choices: ['None', 'Single', 'Multiple'],
        format: (c: string) => trx.instant('Cardinality_' + c)
    };
}
/**
 * Returns the PropDescriptor of a definition state property (Hidden, Visible or Archived)
 */
export function statePropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        datatype: 'string',
        control: 'choice',
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

export function lookupDefinitionIdPropDescriptor(name: string, trx: TranslateService): NumberPropDescriptor {
    return {
        datatype: 'numeric',
        control: 'number',
        label: () => `${trx.instant('Field0Definition', { 0: trx.instant(name) })} (${trx.instant('Id')})`,
        minDecimalPlaces: 0,
        maxDecimalPlaces: 0,
        noSeparator: true
    };
}

export function lookupDefinitionPropDescriptor(name: string, fkName: string, trx: TranslateService): NavigationPropDescriptor {
    return {
        datatype: 'entity',
        control: 'LookupDefinition',
        label: () => trx.instant('Field0Definition', { 0: trx.instant(name) }),
        foreignKeyName: fkName
    };
}

export function mainMenuSectionPropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        datatype: 'string',
        control: 'choice',
        label: () => trx.instant('MainMenuSection'),
        choices: ['Mail', 'Financials', 'Cash', 'FixedAssets', 'Inventory', 'Production', 'Purchasing', 'Marketing', 'Sales', 'HumanCapital', 'Investments', 'Maintenance', 'Administration', 'Security', 'Studio', 'Help'],
        format: (c: string) => trx.instant('Menu_' + c)
    };
}

export function mainMenuSortKeyPropDescriptor(trx: TranslateService): NumberPropDescriptor {
    return { datatype: 'numeric', control: 'number', label: () => trx.instant('MainMenuSortKey'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false };
}

export function mainMenuIconPropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        datatype: 'string',
        control: 'choice',
        label: () => trx.instant('MainMenuIcon'),
        choices: [
            'anchor',
            'angle-double-left',
            'angle-double-right',
            'angle-left',
            'angle-right',
            'angry',
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
            'chalkboard-teacher',
            'chart-area',
            'chart-bar',
            'chart-line',
            'chart-pie',
            'check',
            'city',
            'clipboard-check',
            'clipboard',
            'clipboard-list',
            'code-branch',
            'code',
            'cog',
            'cogs',
            'coins',
            'copy',
            'copyright',
            'cube',
            'compress',
            'dolly-flatbed',
            'door-closed',
            'download',
            'drafting-compass',
            'draw-polygon',
            'edit',
            'ellipsis-h',
            'ellipsis-v',
            'envelope',
            'euro-sign',
            'exchange-alt',
            'exclamation-triangle',
            'exclamation-circle',
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
            'fist-raised',
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
            'hand-point-right',
            'hand-rock',
            'hands-helping',
            'hands',
            'history',
            'holly-berry',
            'home',
            'hourglass-half',
            'id-badge',
            'image',
            'inbox',
            'indent',
            'industry',
            'info-circle',
            'kiss-wink-heart',
            'landmark',
            'laptop-code',
            'laptop',
            'lightbulb',
            'list-ul',
            'list',
            'lock',
            'magic',
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
            'pencil-ruler',
            'pills',
            'plane',
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
            'sms',
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
            'user-circle',
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
