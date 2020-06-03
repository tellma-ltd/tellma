// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor, NavigationPropDescriptor, NumberPropDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { AgentRateForSave, AgentRate } from './agent-rate';

export interface AgentForSave<TAgentRate = AgentRateForSave> extends EntityWithKey {

  Name?: string;
  Name2?: string;
  Name3?: string;
  Code?: string;
  IsRelated?: boolean;
  TaxIdentificationNumber?: string;
  StartDate?: string;
  JobId?: number;
  BankAccountNumber?: number;
  UserId?: number;
  Image?: string;
  Rates?: TAgentRate[];
}

export interface Agent extends AgentForSave<AgentRate> {
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
let _cache: { [defId: string]: EntityDescriptor } = {};
let _definitionIds: string[];

export function metadata_Agent(wss: WorkspaceService, trx: TranslateService, definitionId: string): EntityDescriptor {
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
      _definitionIds = Object.keys(ws.definitions.Agents);
    }

    const entityDesc: EntityDescriptor = {
      collection: 'Agent',
      definitionId,
      definitionIds: _definitionIds,
      titleSingular: () => ws.getMultilingualValueImmediate(ws.definitions.Agents[definitionId], 'TitleSingular') || trx.instant('Agent'),
      titlePlural: () => ws.getMultilingualValueImmediate(ws.definitions.Agents[definitionId], 'TitlePlural') || trx.instant('Agents'),
      select: _select,
      apiEndpoint: !!definitionId ? `agents/${definitionId}` : 'agents',
      screenUrl: !!definitionId ? `agents/${definitionId}` : 'agents',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        DefinitionId: { control: 'number', label: () => `${trx.instant('Definition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Definition: { control: 'navigation', label: () => trx.instant('Definition'), type: 'AgentDefinition', foreignKeyName: 'DefinitionId' },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: () => trx.instant('Code') },
        TaxIdentificationNumber: { control: 'text', label: () => trx.instant('Agent_TaxIdentificationNumber') },
        StartDate: { control: 'date', label: () => trx.instant('Agent_StartDate') },
        JobId: { control: 'number', label: () => `${trx.instant('Agent_Job')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        BankAccountNumber: { control: 'text', label: () => trx.instant('Agent_BankAccountNumber') },
        UserId: { control: 'number', label: () => `${trx.instant('Agent_User')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        User: { control: 'navigation', label: () => trx.instant('Agent_User'), type: 'User', foreignKeyName: 'UserId' },
        IsRelated: { control: 'boolean', label: () => trx.instant('Agent_IsRelated') },
        IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete entityDesc.properties.Name2;
    }

    if (!ws.settings.TernaryLanguageId) {
      delete entityDesc.properties.Name3;
    }
    // Adjust according to definitions
    const definition = _definitions.Agents[definitionId];
    if (!definition) {
      if (!!definitionId) {
        // Programmer mistake
        console.error(`defintionId '${definitionId}' doesn't exist`);
      }
    } else {

      delete entityDesc.properties.DefinitionId;
      delete entityDesc.properties.Definition;

      // Simple properties whose label is overridden by the definition
      const simpleLabelProps = ['StartDate'];
      for (const propName of simpleLabelProps) {
        const propDesc = entityDesc.properties[propName];
        const defaultLabel = propDesc.label;
        propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();
      }

      // Simple properties whose visibility is overridden by the definition
      const simpleVisibilityProps = ['TaxIdentificationNumber', 'StartDate', 'BasicSalary', 'TransportationAllowance', 'OvertimeRate', 'BankAccountNumber'];
      for (const propName of simpleVisibilityProps) {
        if (!definition[propName + 'Visibility']) {
          delete entityDesc.properties[propName];
        }
      }

      // Navigation properties whose label is overriden by the definition
      for (const propName of []) {

        const propDesc = entityDesc.properties[propName] as NavigationPropDescriptor;
        const defaultLabel = propDesc.label;
        propDesc.label = () => ws.getMultilingualValueImmediate(definition, propName + 'Label') || defaultLabel();

        const idPropDesc = entityDesc.properties[propName + 'Id'] as NumberPropDescriptor;
        idPropDesc.label = () => `${propDesc.label()} (${trx.instant('Id')})`;
      }

      // Navigation properties whose visibility is overriden by the definition
      for (const propName of ['Job']) {
        if (!definition[propName + 'Visibility']) {
          delete entityDesc.properties[propName];
          delete entityDesc.properties[propName + 'Id'];
        }
      }
    }

    _cache[key] = entityDesc;
  }

  return _cache[key];
}
