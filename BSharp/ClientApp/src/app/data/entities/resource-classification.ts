// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionsForClient } from '../dto/definitions-for-client';

export class ResourceClassificationForSave extends EntityForSave {
  Name: string;
  Name2: string;
  Name3: string;
  Code: string;
  ParentId: number;
  IsLeaf: boolean;
}

export class ResourceClassification extends ResourceClassificationForSave {
  ResourceDefinitionId: string;
  Level: number;
  ChildCount: number;
  ActiveChildCount: number;
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

export function metadata_ResourceClassification(ws: TenantWorkspace, trx: TranslateService, definitionId: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang || ws.settings !== _settings || ws.definitions !== _definitions) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _definitions = ws.definitions;

    // clear the cache
    _cache = {};
  }

  definitionId = definitionId || '_'; // undefined
  if (!_cache[definitionId]) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    const entityDesc: EntityDescriptor = {
      titleSingular: '???',
      titlePlural: '???',
      select: _select,
      apiEndpoint: 'resource-classifications/' + (definitionId || ''),
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },
        IsLeaf: { control: 'boolean', label: trx.instant('ResourceClassification_IsLeaf'), format: (b) => !!b ? '✔️' : '❌' },

        // tree stuff
        Parent: {
          control: 'navigation', label: trx.instant('TreeParent'), type: 'ResourceClassification',
          foreignKeyName: 'ParentId'
        },
        ChildCount: {
          control: 'number', label: trx.instant('TreeChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
          alignment: 'right'
        },
        ActiveChildCount: {
          control: 'number', label: trx.instant('TreeActiveChildCount'), minDecimalPlaces: 0,
          maxDecimalPlaces: 0, alignment: 'right'
        },
        Level: {
          control: 'number', label: trx.instant('TreeLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 0,
          alignment: 'right'
        },

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

    const definition = _definitions.Resources[definitionId];
    if (!definition) {
        // Programmer mistake
        console.error(`defintionId '${definitionId}' doesn't exist`);
    } else {
      // Singular will be called "Classification" for short
      entityDesc.titleSingular = trx.instant('Classification') || '???';

      // Plural will be something like "Raw Materials - Classifications"
      const resourceTitlePlural = ws.getMultilingualValueImmediate(definition, 'TitlePlural');
      if (!!resourceTitlePlural) {
        entityDesc.titlePlural = `${resourceTitlePlural} - ${trx.instant('Classifications')}`;
      }
    }

    _cache[definitionId] = entityDesc;
  }

  return _cache[definitionId];
}
