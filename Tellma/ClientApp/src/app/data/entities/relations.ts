// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { RelationUserForSave, RelationUser } from './relation-user';
import { EntityForSave } from './base/entity-for-save';

export interface RelationForSave<TRelationUser = RelationUserForSave> extends EntityForSave {
  // Common with Resource
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
  Image?: string;

  AgentId?: number;
  TaxIdentificationNumber?: string;
  JobId?: number;
  BankAccountNumber?: number;
  Users?: TRelationUser[];
}

export interface Relation extends RelationForSave<RelationUser> {
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

export function metadata_Relation(wss: WorkspaceService, trx: TranslateService, definitionId: number): EntityDescriptor {
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
      _definitionIds = Object.keys(ws.definitions.Relations).map(e => +e);
    }

    const entityDesc: EntityDescriptor = {
      collection: 'Relation',
      definitionId,
      definitionIds: _definitionIds,
      titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Relations[definitionId], 'TitleSingular') || trx.instant('Relation'),
      titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Relations[definitionId], 'TitlePlural') || trx.instant('Relations'),
      select: _select,
      apiEndpoint: !!definitionId ? `relations/${definitionId}` : 'relations',
      masterScreenUrl: !!definitionId ? `relations/${definitionId}` : 'relations',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        DefinitionId: { control: 'text', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})` },
        Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'ResourceDefinition', foreignKeyName: 'DefinitionId' },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: () => trx.instant('Code') },
        CurrencyId: { control: 'text', label: () => `${trx.instant('Entity_Currency')} (${trx.instant('Id')})` },
        Currency: { control: 'navigation', label: () => trx.instant('Entity_Currency'), type: 'Currency', foreignKeyName: 'CurrencyId' },
        CenterId: { control: 'number', label: () => `${trx.instant('Entity_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Center: { control: 'navigation', label: () => trx.instant('Entity_Center'), type: 'Center', foreignKeyName: 'CenterId' },
        Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        LocationJson: { control: 'text', label: () => trx.instant('Entity_LocationJson') },

        FromDate: { control: 'date', label: () => trx.instant('Entity_FromDate') },
        ToDate: { control: 'date', label: () => trx.instant('Entity_ToDate') },
        Decimal1: { control: 'number', label: () => trx.instant('Entity_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
        Decimal2: { control: 'number', label: () => trx.instant('Entity_Decimal2'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
        Int1: { control: 'number', label: () => trx.instant('Entity_Int1'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Int2: { control: 'number', label: () => trx.instant('Entity_Int2'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup1Id: { control: 'number', label: () => `${trx.instant('Entity_Lookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup1: { control: 'navigation', label: () => trx.instant('Entity_Lookup1'), type: 'Lookup', foreignKeyName: 'Lookup1Id' },
        Lookup2Id: { control: 'number', label: () => `${trx.instant('Entity_Lookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup2: { control: 'navigation', label: () => trx.instant('Entity_Lookup2'), type: 'Lookup', foreignKeyName: 'Lookup2Id' },
        Lookup3Id: { control: 'number', label: () => `${trx.instant('Entity_Lookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup3: { control: 'navigation', label: () => trx.instant('Entity_Lookup3'), type: 'Lookup', foreignKeyName: 'Lookup3Id' },
        Lookup4Id: { control: 'number', label: () => `${trx.instant('Entity_Lookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup4: { control: 'navigation', label: () => trx.instant('Entity_Lookup4'), type: 'Lookup', foreignKeyName: 'Lookup4Id' },
        // Lookup5Id: { control: 'number', label: () => `${trx.instant('Entity_Lookup5')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        // Lookup5: { control: 'navigation', label: () => trx.instant('Entity_Lookup5'), type: 'Lookup', foreignKeyName: 'Lookup5Id' },
        Text1: { control: 'text', label: () => trx.instant('Entity_Text1') },
        Text2: { control: 'text', label: () => trx.instant('Entity_Text2') },

        // Relation Only

        AgentId: { control: 'number', label: () => `${trx.instant('Relation_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Agent: { control: 'navigation', label: () => trx.instant('Relation_Agent'), type: 'Agent', foreignKeyName: 'AgentId' },
        TaxIdentificationNumber: { control: 'text', label: () => trx.instant('Relation_TaxIdentificationNumber') },
        JobId: { control: 'number', label: () => `${trx.instant('Relation_Job')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        BankAccountNumber: { control: 'text', label: () => trx.instant('Relation_BankAccountNumber') },

        // Standard

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
    const definition = _definitions.Relations[definitionId];
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

      // Simple properties Visibility
      for (const propName of ['TaxIdentificationNumber', 'BankAccountNumber']) {
          if (!definition[propName + 'Visibility']) {
              delete entityDesc.properties[propName];
          }
      }

      // Simple properties Visibility + Label
      for (const propName of ['FromDate', 'ToDate', 'Decimal1', 'Decimal2', 'Int1', 'Int2', 'Text1', 'Text2', 'Identifier']) {
          if (!definition[propName + 'Visibility']) {
              delete entityDesc.properties[propName];
          } else {
              const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
              const defaultLabel = propDesc.label;
              propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();
          }
      }

      // Navigation properties
      for (const propName of ['Currency', 'Center', 'Agent'/*, 'Job'*/]) {
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
      for (const propName of ['1', '2', '3', '4', /*'5' */].map(pf => 'Lookup' + pf)) {
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
