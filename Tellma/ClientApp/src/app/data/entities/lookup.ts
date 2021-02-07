// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export interface LookupForSave extends EntityForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
}

export interface Lookup extends LookupForSave {
    DefinitionId?: number;
    SortKey?: number;
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

export function metadata_Lookup(wss: WorkspaceService, trx: TranslateService, definitionId: number): EntityDescriptor {
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
            _definitionIds = Object.keys(ws.definitions.Lookups).map(e => +e);
        }

        const entityDesc: EntityDescriptor = {
            collection: 'Lookup',
            definitionId,
            definitionIds: _definitionIds,
            titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Lookups[definitionId], 'TitleSingular'),
            titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Lookups[definitionId], 'TitlePlural'),
            select: _select,
            apiEndpoint: !!definitionId ? `lookups/${definitionId}` : 'lookups',
            masterScreenUrl: !!definitionId ? `lookups/${definitionId}` : 'lookups',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] :
                ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: 'IsActive eq true',
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Definition: { datatype: 'entity', control: 'LookupDefinition', label: () => trx.instant('Definition'), foreignKeyName: 'DefinitionId' },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                SortKey: { datatype: 'numeric', control: 'number', label: () => trx.instant('SortKey'), minDecimalPlaces: 2, maxDecimalPlaces: 2, noSeparator: false },

                IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
        }

        const definition = _definitions.Lookups[definitionId];
        if (!definition) {
            if (!!definitionId) {
                // Programmer mistake
                console.error(`defintionId '${definitionId}' doesn't exist`);
            } else {
                entityDesc.titleSingular = () => trx.instant('Lookup');
                entityDesc.titlePlural = () => trx.instant('Lookups');
            }
        } else {
            // Definition specific adjustments
            if (definition.State === 'Archived') {
                entityDesc.isArchived = true;
            }
        }

        _cache[key] = entityDesc;
    }

    return _cache[key];
}
