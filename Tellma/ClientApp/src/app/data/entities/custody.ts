// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { EntityForSave } from './base/entity-for-save';

export interface CustodyForSave extends EntityForSave {
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
  CustodianId?: number;
  Image?: string;

  ExternalReference?: number;
}

export interface Custody extends CustodyForSave {
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

export function metadata_Custody(wss: WorkspaceService, trx: TranslateService, definitionId: number): EntityDescriptor {
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
      _definitionIds = Object.keys(ws.definitions.Custodies).map(e => +e);
    }

    const entityDesc: EntityDescriptor = {
      collection: 'Custody',
      definitionId,
      definitionIds: _definitionIds,
      titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Custodies[definitionId], 'TitleSingular') || trx.instant('Custody'),
      titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Custodies[definitionId], 'TitlePlural') || trx.instant('Custodies'),
      select: _select,
      apiEndpoint: !!definitionId ? `custodies/${definitionId}` : 'custodies',
      masterScreenUrl: !!definitionId ? `custodies/${definitionId}` : 'custodies',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
      properties: {
        Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Definition: { datatype: 'entity', control: 'CustodyDefinition', label: () => trx.instant('Definition'), foreignKeyName: 'DefinitionId' },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entity_Currency')} (${trx.instant('Id')})` },
        Currency: { datatype: 'entity', control: 'Currency', label: () => trx.instant('Entity_Currency'), foreignKeyName: 'CurrencyId' },
        CenterId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Entity_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Center: { datatype: 'entity', control: 'Center', label: () => trx.instant('Entity_Center'),  foreignKeyName: 'CenterId' },
        Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        LocationJson: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_LocationJson') },

        FromDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_FromDate') },
        ToDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_ToDate') },
        Decimal1: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
        Decimal2: { datatype: 'numeric', control: 'number', label: () => trx.instant('Entity_Decimal2'), minDecimalPlaces: 0, maxDecimalPlaces: 4, noSeparator: false },
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

        // Custody Only

        CustodianId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Custody_Custodian')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Custodian: { datatype: 'entity', control: 'Relation', label: () => trx.instant('Custody_Custodian'), foreignKeyName: 'CustodianId' },

        ExternalReference: { datatype: 'string', control: 'text', label: () => trx.instant('Custody_ExternalReference') },

        // Standard
        IsActive: {datatype: 'bit',  control: 'check', label: () => trx.instant('IsActive') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
        ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
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
    const definition = _definitions.Custodies[definitionId];
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
      for (const propName of ['TaxIdentificationNumber']) {
          if (!definition[propName + 'Visibility']) {
              delete entityDesc.properties[propName];
          }
      }

      // Simple properties Visibility + Label
      for (const propName of ['FromDate', 'ToDate', 'Decimal1', 'Decimal2', 'Int1', 'Int2', 'Text1', 'Text2', 'Identifier', 'ExternalReference']) {
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

      // Navigation properties with definition Id
      for (const propName of ['1', '2', '3', '4'].map(pf => 'Lookup' + pf)) {
          if (!definition[propName + 'Visibility']) {
              delete entityDesc.properties[propName];
              delete entityDesc.properties[propName + 'Id'];
          } else {
              const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
              propDesc.definitionId = definition[propName + 'DefinitionId'];
              const defaultLabel = propDesc.label;
              propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

              const idPropDesc = entityDesc.properties[propName + 'Id'] as NumberPropDescriptor;
              idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;
          }
      }

      // Custodian: special case:
      if (!definition.CustodianVisibility) {
          delete entityDesc.properties.CustodianId;
          delete entityDesc.properties.Custodian;
      } else {
          const propDesc = entityDesc.properties.Custodian as NavigationPropDescriptor;
          propDesc.definitionId = definition.CustodianDefinitionId;
          if (!!propDesc.definitionId) {
              const custodianDef = ws.definitions.Relations[propDesc.definitionId];
              if (!!custodianDef) {
                  propDesc.label = () => ws.getMultilingualValueImmediate(custodianDef, 'TitleSingular');
              } else {
                  console.error(`Missing Relation definitionId ${propDesc.definitionId} for custodian`);
              }
          }
      }
    }

    _cache[key] = entityDesc;
  }

  return _cache[key];
}
