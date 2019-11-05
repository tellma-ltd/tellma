// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export class LookupForSave extends EntityForSave {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
}

export class Lookup extends LookupForSave {
    LookupDefinitionId: string;
    SortKey: number;
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

export function metadata_Lookup(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
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
            collection: 'Lookup',
            definitionId,
            titleSingular: ws.getMultilingualValueImmediate(ws.definitions.Lookups[definitionId], 'TitleSingular'),
            titlePlural: ws.getMultilingualValueImmediate(ws.definitions.Lookups[definitionId], 'TitlePlural'),
            select: _select,
            apiEndpoint: 'lookups/' + (definitionId || ''),
            screenUrl: !!definitionId ? 'lookups/' + definitionId : null,
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            definitionFunc: (e: Lookup) => e.LookupDefinitionId,
            selectForDefinition: 'LookupDefinitionId',
            properties: {
                Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: trx.instant('Code') },
                SortKey: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 2, maxDecimalPlaces: 2 },
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

        const definition = _definitions.Lookups[definitionId];
        if (!definition) {
            if (definitionId !== '<generic>') {
                // Programmer mistake
                console.error(`defintionId '${definitionId}' doesn't exist`);
            }
        } else {
            // Definition specific adjustments
        }

        _cache[definitionId] = entityDesc;
    }

    return _cache[definitionId];
}
