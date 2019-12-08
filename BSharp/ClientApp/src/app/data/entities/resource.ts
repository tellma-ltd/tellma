// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export class ResourceForSave extends EntityWithKey {
    OperatingSegmentId: number;
    ResourceClassificationId: number;
    Name: string;
    Name2: string;
    Name3: string;
    Identifier: string;
    Code: string;
    CurrencyId: string;
    MonetaryValue: number;
    CountUnitId: number;
    Count: number;
    MassUnitId: number;
    Mass: number;
    VolumeUnitId: number;
    Volume: number;
    TimeUnitId: number;
    Time: number;
    Description: string;
    Description2: string;
    Description3: string;
    AvailableSince: string;
    AvailableTill: string;
    Lookup1Id: number;
    Lookup2Id: number;
    // Lookup3Id: number;
    // Lookup4Id: number;
    // Lookup5Id: number;
}

export class Resource extends ResourceForSave {
    DefinitionId: string;
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}


const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: string]: EntityDescriptor } = {};
let _definitionIds: string[];

export function metadata_Resource(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
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

        const entityDesc: EntityDescriptor = {
            collection: 'Resource',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitleSingular') || trx.instant('Resource'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitlePlural') || trx.instant('Resources'),
            select: _select,
            apiEndpoint: !!definitionId ? `resources/${definitionId}` : 'resources',
            screenUrl: !!definitionId ? `resources/${definitionId}` : 'resources',
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { control: 'text', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})` },
                Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'ResourceDefinition', foreignKeyName: 'DefinitionId' },
                OperatingSegmentId: { control: 'number', label: () => `${trx.instant('OperatingSegment')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                OperatingSegment: { control: 'navigation', label: () => trx.instant('OperatingSegment'), type: 'ResponsibilityCenter', foreignKeyName: 'OperatingSegmentId' },
                ResourceClassificationId: { control: 'number', label: () => `${trx.instant('Resource_Classification')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceClassification: { control: 'navigation', label: () => trx.instant('Resource_Classification'), type: 'ResourceClassification', definition: definitionId, foreignKeyName: 'ResourceClassificationId' },
                Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Identifier: { control: 'text', label: () => trx.instant('Resource_Identifier') },
                Code: { control: 'text', label: () => trx.instant('Code') },
                CurrencyId: { control: 'text', label: () => `${trx.instant('Resource_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: () => trx.instant('Resource_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                MonetaryValue: { control: 'number', label: () => trx.instant('Resource_MonetaryValue'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
                CountUnitId: { control: 'number', label: () => `${trx.instant('Resource_CountUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CountUnit: { control: 'navigation', label: () => trx.instant('Resource_CountUnit'), type: 'MeasurementUnit', foreignKeyName: 'CountUnitId' },
                Count: { control: 'number', label: () => trx.instant('Resource_Count'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
                MassUnitId: { control: 'number', label: () => `${trx.instant('Resource_MassUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MassUnit: { control: 'navigation', label: () => trx.instant('Resource_MassUnit'), type: 'MeasurementUnit', foreignKeyName: 'MassUnitId' },
                Mass: { control: 'number', label: () => trx.instant('Resource_Mass'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
                VolumeUnitId: { control: 'number', label: () => `${trx.instant('Resource_VolumeUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                VolumeUnit: { control: 'navigation', label: () => trx.instant('Resource_VolumeUnit'), type: 'MeasurementUnit', foreignKeyName: 'VolumeUnitId' },
                Volume: { control: 'number', label: () => trx.instant('Resource_Volume'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
                TimeUnitId: { control: 'number', label: () => `${trx.instant('Resource_TimeUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                TimeUnit: { control: 'navigation', label: () => trx.instant('Resource_TimeUnit'), type: 'MeasurementUnit', foreignKeyName: 'TimeUnitId' },
                Time: { control: 'number', label: () => trx.instant('Resource_Time'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
                Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                AvailableSince: { control: 'date', label: () => trx.instant('Resource_AvailableSince') },
                AvailableTill: { control: 'date', label: () => trx.instant('Resource_AvailableTill') },
                Lookup1Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup1: { control: 'navigation', label: () => trx.instant('Resource_Lookup1'), type: 'Lookup', foreignKeyName: 'Lookup1Id' },
                Lookup2Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup2: { control: 'navigation', label: () => trx.instant('Resource_Lookup2'), type: 'Lookup', foreignKeyName: 'Lookup2Id' },
                // Lookup3Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Lookup3: { control: 'navigation', label: () => trx.instant('Resource_Lookup3'), type: 'Lookup', foreignKeyName: 'Lookup3Id' },
                // Lookup4Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Lookup4: { control: 'navigation', label: () => trx.instant('Resource_Lookup4'), type: 'Lookup', foreignKeyName: 'Lookup4Id' },
                // Lookup5Id: { control: 'number', label: () => `${trx.instant('Resource_Lookup5')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // Lookup5: { control: 'navigation', label: () => trx.instant('Resource_Lookup5'), type: 'Lookup', foreignKeyName: 'Lookup5Id' },

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

            // Simple properties Visibility
            if (!definition.DescriptionVisibility) {
                delete entityDesc.properties.Description;
                delete entityDesc.properties.Description2;
                delete entityDesc.properties.Description3;
            }

            // Simple properties Visibility + Label
            for (const propName of ['Identifier', 'MonetaryValue', 'Count', 'Mass', 'Volume', 'Time', 'AvailableSince', 'AvailableTill']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();
                }
            }

            // Navigation properties
            for (const propName of ['OperatingSegment', 'Currency', 'CountUnit', 'MassUnit', 'VolumeUnit', 'TimeUnit']) {
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
            for (const propName of ['1', '2' /*, '3', '4', '5' */].map(pf => 'Lookup' + pf)) {
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
