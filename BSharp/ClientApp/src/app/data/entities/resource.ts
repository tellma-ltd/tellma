// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export class ResourceForSave extends EntityWithKey {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    ResourceClassificationId: number;
    CurrencyId: number;
    MassUnitId: number;
    VolumeUnitId: number;
    AreaUnitId: number;
    LengthUnitId: number;
    TimeUnitId: number;
    CountUnitId: number;
    Memo: string;
    CustomsReference: string;
    ResourceLookup1Id: number;
    ResourceLookup2Id: number;
    ResourceLookup3Id: number;
    ResourceLookup4Id: number;
}

export class Resource extends ResourceForSave {
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}


const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _currentLang: string;
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: string]: EntityDescriptor } = {};

export function metadata_Resource(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (trx.currentLang !== _currentLang || ws.settings !== _settings || ws.definitions !== _definitions) {
        _currentLang = trx.currentLang;
        _settings = ws.settings;
        _definitions = ws.definitions;

        // clear the cache
        _cache = {};
    }

    if (!_cache[definitionId]) {
        const entityDesc: EntityDescriptor = {
            select: _select,
            apiEndpoint: 'resources/' + (definitionId || ''),
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: trx.instant('Code') },
                ResourceClassificationId: { control: 'number', label: trx.instant('Resource_Classification'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                // ResourceClassification: TODO
                CurrencyId: { control: 'text', label: `${trx.instant('Resource_Currency')} (${trx.instant('Id')})` },
                Currency: { control: 'navigation', label: trx.instant('Resource_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
                MassUnitId: { control: 'number', label: `${trx.instant('Resource_MassUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MassUnit: { control: 'navigation', label: trx.instant('Resource_MassUnit'), type: 'MeasurementUnit', foreignKeyName: 'MassUnitId' },
                VolumeUnitId: { control: 'number', label: `${trx.instant('Resource_VolumeUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                VolumeUnit: { control: 'navigation', label: trx.instant('Resource_VolumeUnit'), type: 'MeasurementUnit', foreignKeyName: 'VolumeUnitId' },
                AreaUnitId: { control: 'number', label: `${trx.instant('Resource_AreaUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AreaUnit: { control: 'navigation', label: trx.instant('Resource_AreaUnit'), type: 'MeasurementUnit', foreignKeyName: 'AreaUnitId' },
                LengthUnitId: { control: 'number', label: `${trx.instant('Resource_LengthUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LengthUnit: { control: 'navigation', label: trx.instant('Resource_LengthUnit'), type: 'MeasurementUnit', foreignKeyName: 'LengthUnitId' },
                TimeUnitId: { control: 'number', label: `${trx.instant('Resource_TimeUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                TimeUnit: { control: 'navigation', label: trx.instant('Resource_TimeUnit'), type: 'MeasurementUnit', foreignKeyName: 'TimeUnitId' },
                CountUnitId: { control: 'number', label: `${trx.instant('Resource_CountUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CountUnit: { control: 'navigation', label: trx.instant('Resource_CountUnit'), type: 'MeasurementUnit', foreignKeyName: 'CountUnitId' },
                Memo: { control: 'text', label: trx.instant('Memo') },
                CustomsReference: { control: 'text', label: trx.instant('Resource_CustomsReference') },
                ResourceLookup1Id: { control: 'number', label: `${trx.instant('Resource_ResourceLookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceLookup1: { control: 'navigation', label: trx.instant('Resource_ResourceLookup1'), type: 'ResourceLookup', foreignKeyName: 'ResourceLookup1Id' },
                ResourceLookup2Id: { control: 'number', label: `${trx.instant('Resource_ResourceLookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceLookup2: { control: 'navigation', label: trx.instant('Resource_ResourceLookup2'), type: 'ResourceLookup', foreignKeyName: 'ResourceLookup2Id' },
                ResourceLookup3Id: { control: 'number', label: `${trx.instant('Resource_ResourceLookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceLookup3: { control: 'navigation', label: trx.instant('Resource_ResourceLookup3'), type: 'ResourceLookup', foreignKeyName: 'ResourceLookup3Id' },
                ResourceLookup4Id: { control: 'number', label: `${trx.instant('Resource_ResourceLookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ResourceLookup4: { control: 'navigation', label: trx.instant('Resource_ResourceLookup4'), type: 'ResourceLookup', foreignKeyName: 'ResourceLookup4Id' },

                IsActive: { control: 'boolean', label: trx.instant('IsActive') },
                CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
        }

        // Adjust according to definitions
        const definition = _definitions.Resources[definitionId];
        if (!definition) {
            // Programmer mistake
            console.error(`defintionId '${definitionId}' doesn't exist`);
        } else {
            for (const propName of ['Memo', 'CustomsReference']) {
                if (!definition[propName + '_Visibility']) {
                    delete entityDesc.properties[propName];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.label = ws.getMultilingualValueImmediate(definition, propName + '_Label') || propDesc.label;
                }
            }

            for (const propName of ['MassUnit', 'VolumeUnit', 'AreaUnit', 'LengthUnit', 'TimeUnit', 'CountUnit']) {
                if (!definition[propName + '_Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.label = ws.getMultilingualValueImmediate(definition, propName + '_Label') || propDesc.label;
                }
            }

            for (const propName of ['1', '2', '3', '4'].map(pf => 'ResourceLookup' + pf)) {
                if (!definition[propName + '_Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.definition = definition[propName + '_DefinitionId'];
                    propDesc.label = ws.getMultilingualValueImmediate(definition, propName + '_Label') || propDesc.label;
                }
            }
        }

        _cache[definitionId] = entityDesc;
    }

    return _cache[definitionId];
}
