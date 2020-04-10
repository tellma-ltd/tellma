// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { ResourceUnitForSave, ResourceUnit } from './resource-unit';

export interface ResourceForSave<TResourceUnit = ResourceUnitForSave> extends EntityWithKey {
    AssetTypeId?: number;
    RevenueTypeId?: number;
    ExpenseTypeId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    Identifier?: string;
    Code?: string;
    CurrencyId?: string;
    MonetaryValue?: number;
    Description?: string;
    Description2?: string;
    Description3?: string;
    ExpenseEntryTypeId?: number;
    CenterId?: number;
    ResidualMonetaryValue?: number;
    ResidualValue?: number;
    ReorderLevel?: number;
    EconomicOrderQuantity?: number;
    AvailableSince?: string;
    AvailableTill?: string;
    Decimal1?: number;
    Decimal2?: number;
    Int1?: number;
    Int2?: number;
    Lookup1Id?: number;
    Lookup2Id?: number;
    Lookup3Id?: number;
    Lookup4Id?: number;
    // Lookup5Id?: number;
    Text1?: string;
    Text2?: string;
    Units?: TResourceUnit[];
}

export interface Resource extends ResourceForSave<ResourceUnit> {
    DefinitionId?: string;
    IsActive?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}


const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: string]: EntityDescriptor } = {};
let _definitionIds: string[];

export function metadata_Resource(wss: WorkspaceService, trx: TranslateService, definitionId: string): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings || ws.definitions !== _definitions) {
        _settings = ws.settings;
        _definitions = ws.definitions;
        _definitionIds = null;

        // clear the cache
        _cache = {};
    }

    const key = definitionId || '-'; // undefined
    if (!_cache[key]) {
        if (!_definitionIds) {
            _definitionIds = Object.keys(ws.definitions.Resources);
        }

        const functionalE = ws.settings.FunctionalCurrencyDecimals;

        const entityDesc: EntityDescriptor = {
            collection: 'Resource',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitleSingular') || trx.instant('Resource'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitlePlural') || trx.instant('Resources'),
            select: _select,
            apiEndpoint: !!definitionId ? `resources/${definitionId}` : 'resources',
            screenUrl: !!definitionId ? `resources/${definitionId}` : 'resources',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { control: 'text', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})` },
                Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'ResourceDefinition', foreignKeyName: 'DefinitionId' },
                AssetTypeId: { control: 'number', label: () => `${trx.instant('Resource_AssetType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AssetType: { control: 'navigation', label: () => trx.instant('Resource_AssetType'), type: 'AccountType', definition: definitionId, foreignKeyName: 'AssetTypeId' },
                RevenueTypeId: { control: 'number', label: () => `${trx.instant('Resource_RevenueType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                RevenueType: { control: 'navigation', label: () => trx.instant('Resource_RevenueType'), type: 'AccountType', definition: definitionId, foreignKeyName: 'RevenueTypeId' },
                ExpenseTypeId: { control: 'number', label: () => `${trx.instant('Resource_ExpenseType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ExpenseType: { control: 'navigation', label: () => trx.instant('Resource_ExpenseType'), type: 'AccountType', definition: definitionId, foreignKeyName: 'ExpenseTypeId' },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Identifier: { control: 'text', label: () => trx.instant('Resource_Identifier') },
                Code: { control: 'text', label: () => trx.instant('Code') },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Resource_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Resource_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                MonetaryValue: { control: 'number', label: () => trx.instant('Resource_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                ExpenseEntryTypeId: { control: 'number', label: () => `${trx.instant('Resource_ExpenseEntryType')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ExpenseEntryType: { control: 'navigation', label: () => trx.instant('Resource_ExpenseEntryType'), type: 'EntryType', foreignKeyName: 'ExpenseEntryTypeId' },
                CenterId: { control: 'number', label: () => `${trx.instant('Resource_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { control: 'navigation', label: () => trx.instant('Resource_Center'), type: 'Center', foreignKeyName: 'CenterId' },
                ResidualMonetaryValue: { control: 'number', label: () => trx.instant('Entry_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, alignment: 'right' },
                ResidualValue: { control: 'number', label: () => `${trx.instant('Resource_ResidualValue')} (${ws.getMultilingualValueImmediate(ws.settings, 'FunctionalCurrencyName')})` , minDecimalPlaces: functionalE, maxDecimalPlaces: functionalE, alignment: 'right' },
                ReorderLevel: { control: 'number', label: () => trx.instant('Resource_ReorderLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                EconomicOrderQuantity: { control: 'number', label: () => trx.instant('Resource_EconomicOrderQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                AvailableSince: { control: 'date', label: () => trx.instant('Resource_AvailableSince') },
                AvailableTill: { control: 'date', label: () => trx.instant('Resource_AvailableTill') },
                Decimal1: { control: 'number', label: () => trx.instant('Resource_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                Decimal2: { control: 'number', label: () => trx.instant('Resource_Decimal2'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
                Int1: { control: 'number', label: () => trx.instant('Resource_Int1'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Int2: { control: 'number', label: () => trx.instant('Resource_Int2'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup1Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup1: { control: 'navigation', label: () => trx.instant('Resource_Lookup1'), type: 'Lookup', foreignKeyName: 'Lookup1Id' },
                Lookup2Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup2: { control: 'navigation', label: () => trx.instant('Resource_Lookup2'), type: 'Lookup', foreignKeyName: 'Lookup2Id' },
                Lookup3Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup3: { control: 'navigation', label: () => trx.instant('Resource_Lookup3'), type: 'Lookup', foreignKeyName: 'Lookup3Id' },
                Lookup4Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup4: { control: 'navigation', label: () => trx.instant('Resource_Lookup4'), type: 'Lookup', foreignKeyName: 'Lookup4Id' },
                // Lookup5Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup5')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Lookup5: { control: 'navigation', label: () => trx.instant('Resource_Lookup5'), type: 'Lookup', foreignKeyName: 'Lookup5Id' },
                Text1: { control: 'text', label: () => trx.instant('Resource_Text1') },
                Text2: { control: 'text', label: () => trx.instant('Resource_Text2') },
                IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
                CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
            delete entityDesc.properties.Description2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
            delete entityDesc.properties.Description3;
        }

        // Adjust according to definitions
        const definition = _definitions.Resources[definitionId];
        if (!definition) {
            if (!!definitionId) {
                // Programmer mistake
                console.error(`defintionId '${definitionId}' doesn't exist`);
            }
        } else {

            delete entityDesc.properties.DefinitionId;
            delete entityDesc.properties.Definition;

            // Description, special case
            if (!definition.DescriptionVisibility) {
                delete entityDesc.properties.Description;
                delete entityDesc.properties.Description2;
                delete entityDesc.properties.Description3;
            }

            // Simple properties Visibility
            for (const propName of ['ReorderLevel', 'EconomicOrderQuantity', 'ResidualMonetaryValue', 'ResidualValue']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                }
            }

            // Simple properties Visibility + Label
            for (const propName of ['Identifier', 'MonetaryValue', 'AvailableSince', 'AvailableTill', 'Decimal1', 'Decimal2', 'Int1', 'Int2', 'Text1', 'Text2']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();
                }
            }

            // Navigation properties
            for (const propName of ['Currency', 'ExpenseEntryType', 'Center', 'AssetType', 'RevenueType', 'ExpenseType']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    const idPropDesc = entityDesc.properties[propName + 'Id'] as NumberPropDescriptor;
                    idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;
                }
            }

            // Navigation properties with definition Id
            for (const propName of ['1', '2' , '3', '4', /*'5' */].map(pf => 'Lookup' + pf)) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.definition = definition[propName + 'DefinitionId'];
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    const idPropDesc = entityDesc.properties[propName + 'Id'] as NumberPropDescriptor;
                    idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;
                }
            }
        }

        _cache[key] = entityDesc;
    }

    return _cache[key];
}
