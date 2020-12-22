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
import { RelationAttachment, RelationAttachmentForSave } from './relation-attachment';

export interface RelationForSave<TRelationUser = RelationUserForSave, TAttachment = RelationAttachmentForSave> extends EntityForSave {
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
  DateOfBirth?: string;
  ContactEmail?: string;
  ContactMobile?: string;
  NormalizedContactMobile?: string;
  ContactAddress?: string;
  Date1?: string;
  Date2?: string;
  Date3?: string;
  Date4?: string;
  Decimal1?: number;
  Decimal2?: number;
  Int1?: number;
  Int2?: number;
  Lookup1Id?: number;
  Lookup2Id?: number;
  Lookup3Id?: number;
  Lookup4Id?: number;
  Lookup5Id?: number;
  Lookup6Id?: number;
  Lookup7Id?: number;
  Lookup8Id?: number;
  Text1?: string;
  Text2?: string;
  Text3?: string;
  Text4?: string;
  Image?: string;

  AgentId?: number;
  TaxIdentificationNumber?: string;
  JobId?: number;
  BankAccountNumber?: number;
  Users?: TRelationUser[];
  Attachments?: TAttachment[];
}

export interface Relation extends RelationForSave<RelationUser, RelationAttachment> {
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
        Id: { datatype: 'integral', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        DefinitionId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Definition: { datatype: 'entity', label: () => trx.instant('Definition'), control: 'RelationDefinition', foreignKeyName: 'DefinitionId' },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        CurrencyId: { datatype: 'string', control: 'text', label: () => `${trx.instant('Entity_Currency')} (${trx.instant('Id')})` },
        Currency: { datatype: 'entity', label: () => trx.instant('Entity_Currency'), control: 'Currency', foreignKeyName: 'CurrencyId' },
        CenterId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Center')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Center: { datatype: 'entity', label: () => trx.instant('Entity_Center'), control: 'Center', foreignKeyName: 'CenterId' },
        Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        LocationJson: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_LocationJson') },
        FromDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_FromDate') },
        ToDate: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_ToDate') },

        DateOfBirth: { datatype: 'date', control: 'date', label: () => trx.instant('Relation_DateOfBirth') },
        ContactEmail: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_ContactEmail') },
        ContactMobile: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_ContactMobile') },
        NormalizedContactMobile: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_NormalizedContactMobile') },
        ContactAddress: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_ContactAddress') },
        Date1: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date1') },
        Date2: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date2') },
        Date3: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date3') },
        Date4: { datatype: 'date', control: 'date', label: () => trx.instant('Entity_Date4') },
        Decimal1: { datatype: 'decimal', control: 'number', label: () => trx.instant('Entity_Decimal1'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
        Decimal2: { datatype: 'decimal', control: 'number', label: () => trx.instant('Entity_Decimal2'), minDecimalPlaces: 0, maxDecimalPlaces: 4 },
        Int1: { datatype: 'integral', control: 'number', label: () => trx.instant('Entity_Int1'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Int2: { datatype: 'integral', control: 'number', label: () => trx.instant('Entity_Int2'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup1Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup1: { datatype: 'entity', label: () => trx.instant('Entity_Lookup1'), control: 'Lookup', foreignKeyName: 'Lookup1Id' },
        Lookup2Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup2')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup2: { datatype: 'entity', label: () => trx.instant('Entity_Lookup2'), control: 'Lookup', foreignKeyName: 'Lookup2Id' },
        Lookup3Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup3')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup3: { datatype: 'entity', label: () => trx.instant('Entity_Lookup3'), control: 'Lookup', foreignKeyName: 'Lookup3Id' },
        Lookup4Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup4')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup4: { datatype: 'entity', label: () => trx.instant('Entity_Lookup4'), control: 'Lookup', foreignKeyName: 'Lookup4Id' },
        Lookup5Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup5')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup5: { datatype: 'entity', label: () => trx.instant('Entity_Lookup5'), control: 'Lookup', foreignKeyName: 'Lookup5Id' },
        Lookup6Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup6')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup6: { datatype: 'entity', label: () => trx.instant('Entity_Lookup6'), control: 'Lookup', foreignKeyName: 'Lookup6Id' },
        Lookup7Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup7')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup7: { datatype: 'entity', label: () => trx.instant('Entity_Lookup7'), control: 'Lookup', foreignKeyName: 'Lookup7Id' },
        Lookup8Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Entity_Lookup8')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Lookup8: { datatype: 'entity', label: () => trx.instant('Entity_Lookup8'), control: 'Lookup', foreignKeyName: 'Lookup8Id' },
        Text1: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text1') },
        Text2: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text2') },
        Text3: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text3') },
        Text4: { datatype: 'string', control: 'text', label: () => trx.instant('Entity_Text4') },

        // Relation Only

        AgentId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Relation_Agent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Agent: { datatype: 'entity', label: () => trx.instant('Relation_Agent'), control: 'Agent', foreignKeyName: 'AgentId' },
        TaxIdentificationNumber: { datatype: 'string', control: 'text', label: () => trx.instant('Relation_TaxIdentificationNumber') },
        JobId: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Relation_Job')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        BankAccountNumber: { datatype: 'string', control: 'text', label: () => trx.instant('Relation_BankAccountNumber') },
        Relation1Id: { datatype: 'integral', control: 'number', label: () => `${trx.instant('Relation_Relation1')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Relation1: { datatype: 'entity', label: () => trx.instant('Relation_Relation1'), control: 'Relation', foreignKeyName: 'Relation1Id' },

        // Standard

        IsActive: { datatype: 'boolean', control: 'boolean', label: () => trx.instant('IsActive') },
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
      for (const propName of ['TaxIdentificationNumber', 'BankAccountNumber', 'DateOfBirth', 'ContactEmail', 'ContactMobile', 'ContactAddress']) {
          if (!definition[propName + 'Visibility']) {
              delete entityDesc.properties[propName];
          }
      }

      // Special case
      if (!definition.ContactMobileVisibility) {
        delete entityDesc.properties.NormalizedContactMobile;
      }

      // Simple properties Visibility + Label
      for (const propName of ['FromDate', 'ToDate', 'Decimal1', 'Decimal2', 'Int1', 'Int2', 'Text1', 'Text2', 'Text3', 'Text4', 'Date1', 'Date2', 'Date3', 'Date4', 'Identifier']) {
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
      for (const propName of ['1', '2', '3', '4', '5', '6', '7', '8'].map(pf => 'Lookup' + pf).concat(['Relation1'])) {
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
    }

    _cache[key] = entityDesc;
  }

  return _cache[key];
}
