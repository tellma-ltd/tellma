// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { ResourceUnitForSave, ResourceUnit } from './resource-unit';
import { EntityForSave } from './base/entity-for-save';
import { DateGranularity, TimeGranularity } from './base/metadata-types';

export interface ResourceForSave<TResourceUnit = ResourceUnitForSave> extends EntityForSave {
    // Common with Agent
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    CurrencyId?: string;
    CenterId?: number;
    Description?: string;
    Description2?: string;
    Description3?: string;
    LocationJson?: string;
    FromDate?: string;
    ToDate?: string;
    Date1?: string;
    Date2?: string;
    Date3?: string;
    Date4?: string;
    Decimal1?: number;
    Decimal2?: number;
    Decimal3?: number;
    Decimal4?: number;
    Int1?: number;
    Int2?: number;
    Lookup1Id?: number;
    Lookup2Id?: number;
    Lookup3Id?: number;
    Lookup4Id?: number;
    // Lookup5Id?: number;
    Text1?: string;
    Text2?: string;
    Image?: string;

    // Resource Only
    Identifier?: string;
    VatRate?: number;
    ReorderLevel?: number;
    EconomicOrderQuantity?: number;
    MonetaryValue?: number;
    UnitId?: number;
    UnitMass?: number;
    UnitMassUnitId?: number;
    Agent1Id?: number;
    Agent2Id?: number;
    Resource1Id?: number;
    Resource2Id?: number;
    Units?: TResourceUnit[];
}

export interface Resource extends ResourceForSave<ResourceUnit> {
    DefinitionId?: number;
    ImageId?: string;
    IsActive?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: { [defId: number]: EntityDescriptor } = {};
let _definitionIds: number[];

export function metadata_Resource(wss: WorkspaceService, trx: TranslateService, definitionId: number): EntityDescriptor {
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
            _definitionIds = Object.keys(ws.definitions.Resources).map(e => +e);
        }

        const entityDesc: EntityDescriptor = {
            collection: 'Resource',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitleSingular') || trx.instant('Resource'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Resources[definitionId], 'TitlePlural') || trx.instant('Resources'),
            select: _select,
            apiEndpoint: !!definitionId ? `resources/${definitionId}` : 'resources',
            masterScreenUrl: !!definitionId ? `resources/${definitionId}` : 'resources',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: 'IsActive eq true',
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Definition: { datatype: 'entity', control: 'ResourceDefinition', label: () => trx.instant('Definition'), foreignKeyName: 'DefinitionId' },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entity_Currency')} (${trx.instant('Id')})` },
                Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('Entity_Currency'), foreignKeyName: 'CurrencyId' },
                CenterId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Center: { datatype: 'entity', control: 'Center', label: () => trx.instant('Entity_Center'), foreignKeyName: 'CenterId' },
                Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                LocationJson: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_LocationJson') },

                FromDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_FromDate'), granularity: DateGranularity.days },
                ToDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_ToDate'), granularity: DateGranularity.days },
                Date1: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date1'), granularity: DateGranularity.days },
                Date2: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date2'), granularity: DateGranularity.days },
                Date3: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date3'), granularity: DateGranularity.days },
                Date4: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date4'), granularity: DateGranularity.days },
                Decimal1: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                Decimal2: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal2'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                Decimal3: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal3'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                Decimal4: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal4'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                Int1: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Int1'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
                Int2: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Int2'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
                Lookup1Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Lookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup1: { datatype: 'entity', control: 'Lookup', label: () => trx.instant('Entity_Lookup1'), foreignKeyName: 'Lookup1Id' },
                Lookup2Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Lookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup2: { datatype: 'entity', control: 'Lookup', label: () => trx.instant('Entity_Lookup2'), foreignKeyName: 'Lookup2Id' },
                Lookup3Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Lookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup3: { datatype: 'entity', control: 'Lookup', label: () => trx.instant('Entity_Lookup3'), foreignKeyName: 'Lookup3Id' },
                Lookup4Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Lookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Lookup4: { datatype: 'entity', control: 'Lookup', label: () => trx.instant('Entity_Lookup4'), foreignKeyName: 'Lookup4Id' },
                Text1: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text1') },
                Text2: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text2') },

                // Resource Only
                Identifier: { datatype: 'string', control: 'text', label: () => trx.instant('Resource_Identifier') },
                VatRate: { datatype: 'numeric', control: 'percent', label: () => trx.instant('Resource_VatRate'), minDecimalPlaces: 2, maxDecimalPlaces: 4, noSeparator: false },
                ReorderLevel: { datatype: 'numeric', control: 'number', label: () => trx.instant('Resource_ReorderLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                EconomicOrderQuantity: { datatype: 'numeric', control: 'number', label: () => trx.instant('Resource_EconomicOrderQuantity'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                MonetaryValue: { datatype: 'numeric', control: 'number', label: () => trx.instant('Resource_MonetaryValue'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },

                UnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_Unit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Resource_Unit'), foreignKeyName: 'UnitId' },
                UnitMass: { datatype: 'numeric', control: 'number', label: () => trx.instant('Resource_UnitMass'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
                UnitMassUnitId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_UnitMassUnit')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                UnitMassUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Resource_UnitMassUnit'), foreignKeyName: 'UnitMassUnitId' },

                Agent1Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_Agent1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent1: { datatype: 'entity', control: 'Agent', label: () => trx.instant('Resource_Agent1'), foreignKeyName: 'Agent1Id' },
                Agent2Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_Agent2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Agent2: { datatype: 'entity', control: 'Agent', label: () => trx.instant('Resource_Agent2'), foreignKeyName: 'Agent2Id' },

                Resource1Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_Resource1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource1: { datatype: 'entity', label: () => trx.instant('Resource_Resource1'), control: 'Resource', foreignKeyName: 'Resource1Id' },
                Resource2Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Resource_Resource2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource2: { datatype: 'entity', label: () => trx.instant('Resource_Resource2'), control: 'Resource', foreignKeyName: 'Resource2Id' },

                // Standard

                IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
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

            // Definition specific adjustments
            if (definition.State === 'Archived') {
                entityDesc.isArchived = true;
            }

            delete entityDesc.properties.DefinitionId;
            delete entityDesc.properties.Definition;

            // Description, special case
            if (!definition.DescriptionVisibility) {
                delete entityDesc.properties.Description;
                delete entityDesc.properties.Description2;
                delete entityDesc.properties.Description3;
            }

            // Location, special case
            if (!definition.LocationVisibility) {
                delete entityDesc.properties.LocationJson;
            }

            // Unit, special case
            if (!definition.UnitCardinality) {
                delete entityDesc.properties.UnitId;
                delete entityDesc.properties.Unit;
            }

            // UnitMassUnit, special case
            if (!definition.UnitMassVisibility) {
                delete entityDesc.properties.UnitMassUnitId;
                delete entityDesc.properties.UnitMassUnit;
            }

            // Simple properties Visibility
            for (const propName of ['ReorderLevel', 'EconomicOrderQuantity', 'MonetaryValue', 'UnitMass', 'VatRate']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                }
            }

            // Simple properties Visibility + Label
            for (const propName of ['FromDate', 'ToDate', 'Decimal1', 'Decimal2', 'Decimal3', 'Decimal4', 'Int1', 'Int2', 'Text1', 'Text2', 'Identifier']) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    const defaultLabel = propDesc.label;
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();
                }
            }

            // Navigation properties
            for (const propName of ['Currency', 'Center']) {
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

            // Navigation properties with label and definition Id
            for (const propName of ['1', '2', '3', '4'].map(pf => 'Lookup' + pf).concat(['Agent1', 'Agent2', 'Resource1', 'Resource2'])) {
                if (!definition[propName + 'Visibility']) {
                    delete entityDesc.properties[propName];
                    delete entityDesc.properties[propName + 'Id'];
                } else {
                    const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
                    propDesc.definitionId = definition[propName + 'DefinitionId'];

                    // Calculate the default label
                    let defaultLabel: () => string;
                    if (!!propDesc.definitionId) {
                        // If definitionId is specified, the default label is the singular title of the definition
                        const navDef = propName.startsWith('Lookup') ? ws.definitions.Lookups[propDesc.definitionId] :
                            propName.startsWith('Resource') ? ws.definitions.Resources[propDesc.definitionId] :
                                propName.startsWith('Agent') ? ws.definitions.Agents[propDesc.definitionId] : null;

                        if (!!navDef) {
                            defaultLabel = () => ws.getMultilingualValueImmediate(navDef, 'TitleSingular');
                        } else {
                            console.error(`Missing definitionId ${propDesc.definitionId} for ${propName}.`);
                            defaultLabel = propDesc.label;
                        }
                    } else {
                        // If definition is not specified, the default label is the generic name of the column (e.g. Lookup 1)
                        defaultLabel = propDesc.label;
                    }
                    propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

                    const idPropDesc = entityDesc.properties[propName + 'Id'] as NumberPropDescriptor;
                    idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;
                }
            }

            // // Agent1: special case:
            // if (!definition.Agent1Visibility) {
            //     delete entityDesc.properties.Agent1Id;
            //     delete entityDesc.properties.Agent1;
            // } else {
            //     const propDesc = entityDesc.properties.Agent1 as NavigationPropDescriptor;
            //     propDesc.definitionId = definition.Agent1DefinitionId;
            //     if (!!propDesc.definitionId) {
            //         const agent1Def = ws.definitions.Agents[propDesc.definitionId];
            //         if (!!agent1Def) {
            //             propDesc.label = () => ws.getMultilingualValueImmediate(agent1Def, 'TitleSingular');
            //         } else {
            //             console.error(`Missing Agent definitionId ${propDesc.definitionId} for Agent1.`);
            //         }
            //     }
            // }

            // // Agent2: special case:
            // if (!definition.Agent2Visibility) {
            //     delete entityDesc.properties.Agent2Id;
            //     delete entityDesc.properties.Agent2;
            // } else {
            //     const propDesc = entityDesc.properties.Agent2 as NavigationPropDescriptor;
            //     propDesc.definitionId = definition.Agent2DefinitionId;
            //     if (!!propDesc.definitionId) {
            //         const agent2Def = ws.definitions.Agents[propDesc.definitionId];
            //         if (!!agent2Def) {
            //             propDesc.label = () => ws.getMultilingualValueImmediate(agent2Def, 'TitleSingular');
            //         } else {
            //             console.error(`Missing Agent definitionId ${propDesc.definitionId} for Agent2.`);
            //         }
            //     }
            // }
        }

        _cache[key] = entityDesc;
    }

    return _cache[key];
}
