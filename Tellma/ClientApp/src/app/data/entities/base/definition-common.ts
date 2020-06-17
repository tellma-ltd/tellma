// tslint:disable:variable-name

import { TranslateService } from '@ngx-translate/core';
import { NumberPropDescriptor, ChoicePropDescriptor } from './metadata';
import { TenantWorkspace } from '../../workspace.service';

// tslint:disable:max-line-length
export type DefinitionVisibility = 'None' | 'Optional' | 'Required';
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
    return { control: 'number', label: () => trx.instant('MainMenuSortKey'), minDecimalPlaces: 2, maxDecimalPlaces: 2 };
}

export function mainMenuIconPropDescriptor(trx: TranslateService): ChoicePropDescriptor {
    return {
        control: 'choice',
        label: () => trx.instant('MainMenuIcon'),
        choices: [
            'list',
            'list-ul',
            'money-check',
            'money-check-alt',
            'hand-holding-usd',
            'sitemap',
            'chart-pie',
            'chart-line',
            'chart-bar',
            'chart-area',
            'bars',
            'coins',
            'landmark',
            'file-contract',
            'file-invoice-dollar',
            'money-bill-wave',
            'clipboard',
            'folder',
            'euro-sign',
            'truck',
            'user-friends',
            'exchange-alt',
            'lock',
            'laptop',
            'laptop-code',
            'microchip',
            'users-cog',
            'user-tie',
            'user-tag',
            'user-shield',
            'book',
            'project-diagram',
            'shopping-cart',
            'inbox',
            'share-square',
            'share',
            'print',
            'code',
            'font',
            'image',
            'portrait',
            'warehouse',
            'power-off',
            'car-side',
            'tint',
            'tint-slash',
            'file-import',
            'file-export',
            'pills',
            'user-check',
            'fax',
            'balance-scale',
            'hands-helping',
            'map',
            'palette',
            'copy',
            'scroll',
            'industry',
            'city',
            'tractor',
            'boxes',
            'car',
            'recycle',
            'id-badge',
            'funnel-dollar',
            'cash-register',
            'map-marker-alt',
            'newspaper',
            'user-clock',
            'anchor',
            'dolly-flatbed',
            'edit',
            'folder-minus',
            'folder-plus',
            'umbrella-beach',
            'spa',
            'user-minus',
            'trophy',
            'suitcase-rolling',
            'tasks',

            'camera',
            'camera-retro',
            'user',
            'ruler',
            'users',
            'cog',
            'tasks',
            'male',
            'female',
            'building',
            'info-circle',
            'tools',

            'thumbs-up',
            'thumbs-down',
            'paperclip',
            'file',
            'file-pdf',
            'file-word',
            'file-excel',
            'file-powerpoint',
            'file-alt',
            'file-archive',
            'file-image',
            'file-video',
            'file-audio',
            'ellipsis-v',
            'ellipsis-h',
            'archive',
            'sign-out-alt',
            'code-branch',
            'check',
            'plus',
            'angle-double-left',
            'angle-left',
            'angle-right',
            'th-large',
            'table',
            'pen',
            'trash',
            'save',
            'times',
            'download',
            'arrow-circle-right',
            'undo',
            'clipboard-check',
            'upload',
            'file-download',
            'filter',
            'calendar',
            'asterisk',

            'external-link-alt',
            'minus-square',
            'spinner',
            // 'arrow-right',
            // 'arrow-left',
            // 'chevron-right',
            'wifi',
            'sync-alt',
            'search',
            'cube',
            'cogs',
            'hands',
            'sign-in-alt',
            'exclamation-triangle',
            'home',
            'redo-alt'
        ],
        format: (c: string) => c,
    };
}
