// tslint:disable:max-line-length
// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { AccountTypeCustodyDefinitionForSave, AccountTypeCustodyDefinition } from './account-type-custody-definition';
import { AccountTypeResourceDefinitionForSave, AccountTypeResourceDefinition } from './account-type-resource-definition';

export type RequiredAssignment = 'A' | 'E';
export type OptionalAssignment = 'N' | 'A' | 'E';
export type EntryAssignment = 'N' | 'E';

export interface AccountTypeForSave<TCustodyDef = AccountTypeCustodyDefinitionForSave,
  TResourceDef = AccountTypeResourceDefinitionForSave> extends EntityForSave {
  ParentId?: number;
  Name?: string;
  Name2?: string;
  Name3?: string;
  Description?: string;
  Description2?: string;
  Description3?: string;
  Code?: string;
  Concept?: string;
  IsAssignable?: boolean;
  StandardAndPure?: boolean;
  CustodianDefinitionId?: number;
  ParticipantDefinitionId?: number;
  EntryTypeParentId?: number;
  NotedRelationDefinitionId?: number;
  IsMonetary?: boolean;
  Time1Label?: string;
  Time1Label2?: string;
  Time1Label3?: string;
  Time2Label?: string;
  Time2Label2?: string;
  Time2Label3?: string;
  ExternalReferenceLabel?: string;
  ExternalReferenceLabel2?: string;
  ExternalReferenceLabel3?: string;
  AdditionalReferenceLabel?: string;
  AdditionalReferenceLabel2?: string;
  AdditionalReferenceLabel3?: string;
  NotedAgentNameLabel?: string;
  NotedAgentNameLabel2?: string;
  NotedAgentNameLabel3?: string;
  NotedAmountLabel?: string;
  NotedAmountLabel2?: string;
  NotedAmountLabel3?: string;
  NotedDateLabel?: string;
  NotedDateLabel2?: string;
  NotedDateLabel3?: string;

  CustodyDefinitions?: TCustodyDef[];
  ResourceDefinitions?: TResourceDef[];
}

export interface AccountType extends AccountTypeForSave<AccountTypeCustodyDefinition, AccountTypeResourceDefinition> {
  Path?: string;
  Level?: number;
  ActiveChildCount?: number;
  ChildCount?: number;
  IsActive?: boolean;
  IsSystem?: boolean;
  SavedById?: number | string;

  CustodyDefinitionsCount?: number;
  ResourceDefinitionsCount?: number;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _definitions: DefinitionsForClient;
let _cache: EntityDescriptor = null;

export function metadata_AccountType(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _settings || ws.definitions !== _definitions) {
    _settings = ws.settings;
    _definitions = ws.definitions;

    // clear the cache
    _cache = null;
  }

  if (!_cache) {
    const entityDesc: EntityDescriptor = {
      collection: 'AccountType',
      titleSingular: () => trx.instant('AccountType'),
      titlePlural: () => trx.instant('AccountTypes'),
      select: _select,
      apiEndpoint: 'account-types',
      masterScreenUrl: 'account-types',
      orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      inactiveFilter: 'IsActive eq true',
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        IsMonetary: { control: 'boolean', label: () => trx.instant('AccountType_IsMonetary') },
        Code: { control: 'text', label: () => trx.instant('Code') },
        Concept: { control: 'text', label: () => trx.instant('AccountType_Concept') },
        IsAssignable: { control: 'boolean', label: () => trx.instant('IsAssignable') },
        StandardAndPure: { control: 'boolean', label: () => trx.instant('AccountType_StandardAndPure') },
        CustodianDefinitionId: { control: 'number', label: () => `${trx.instant('AccountType_CustodianDefinition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        CustodianDefinition: { control: 'navigation', label: () => trx.instant('AccountType_CustodianDefinition'), type: 'RelationDefinition', foreignKeyName: 'CustodianDefinitionId' },
        ParticipantDefinitionId: { control: 'number', label: () => `${trx.instant('AccountType_ParticipantDefinition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        ParticipantDefinition: { control: 'navigation', label: () => trx.instant('AccountType_ParticipantDefinition'), type: 'RelationDefinition', foreignKeyName: 'ParticipantDefinitionId' },
        EntryTypeParentId: { control: 'number', label: () => `${trx.instant('AccountType_EntryTypeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        EntryTypeParent: { control: 'navigation', label: () => trx.instant('AccountType_EntryTypeParent'), type: 'EntryType', foreignKeyName: 'EntryTypeParentId' },
        NotedRelationDefinitionId: { control: 'number', label: () => `${trx.instant('AccountType_NotedRelationDefinition')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        NotedRelationDefinition: { control: 'navigation', label: () => trx.instant('AccountType_NotedRelationDefinition'), type: 'RelationDefinition', foreignKeyName: 'NotedRelationDefinitionId' },
        Time1Label: { control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.primaryPostfix },
        Time1Label2: { control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.secondaryPostfix },
        Time1Label3: { control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.ternaryPostfix },
        Time2Label: { control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.primaryPostfix },
        Time2Label2: { control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.secondaryPostfix },
        Time2Label3: { control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.ternaryPostfix },
        ExternalReferenceLabel: { control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.primaryPostfix },
        ExternalReferenceLabel2: { control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.secondaryPostfix },
        ExternalReferenceLabel3: { control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.ternaryPostfix },
        AdditionalReferenceLabel: { control: 'text', label: () => trx.instant('AccountType_AdditionalReferenceLabel') + ws.primaryPostfix },
        AdditionalReferenceLabel2: { control: 'text', label: () => trx.instant('AccountType_AdditionalReferenceLabel') + ws.secondaryPostfix },
        AdditionalReferenceLabel3: { control: 'text', label: () => trx.instant('AccountType_AdditionalReferenceLabel') + ws.ternaryPostfix },
        NotedAgentNameLabel: { control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.primaryPostfix },
        NotedAgentNameLabel2: { control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.secondaryPostfix },
        NotedAgentNameLabel3: { control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.ternaryPostfix },
        NotedAmountLabel: { control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.primaryPostfix },
        NotedAmountLabel2: { control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.secondaryPostfix },
        NotedAmountLabel3: { control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.ternaryPostfix },
        NotedDateLabel: { control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.primaryPostfix },
        NotedDateLabel2: { control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.secondaryPostfix },
        NotedDateLabel3: { control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.ternaryPostfix },

        // tree stuff
        Path: { control: 'text', label: () => trx.instant('TreePath') },
        ParentId: { control: 'number', label: () => `${trx.instant('TreeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Parent: { control: 'navigation', label: () => trx.instant('TreeParent'), type: 'AccountType', foreignKeyName: 'ParentId' },
        ChildCount: { control: 'number', label: () => trx.instant('TreeChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        ActiveChildCount: { control: 'number', label: () => trx.instant('TreeActiveChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        Level: { control: 'number', label: () => trx.instant('TreeLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },

        IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
        IsSystem: { control: 'boolean', label: () => trx.instant('IsSystem') },

        // Audit info
        SavedById: { control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        SavedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'SavedById' }
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete entityDesc.properties.Name2;
      delete entityDesc.properties.Description2;
      delete entityDesc.properties.Time1Label2;
      delete entityDesc.properties.Time2Label2;
      delete entityDesc.properties.ExternalReferenceLabel2;
      delete entityDesc.properties.AdditionalReferenceLabel2;
      delete entityDesc.properties.NotedAgentNameLabel2;
      delete entityDesc.properties.NotedAmountLabel2;
      delete entityDesc.properties.NotedDateLabel2;
    }

    if (!ws.settings.TernaryLanguageId) {
      delete entityDesc.properties.Name3;
      delete entityDesc.properties.Description3;
      delete entityDesc.properties.Time1Label3;
      delete entityDesc.properties.Time2Label3;
      delete entityDesc.properties.ExternalReferenceLabel3;
      delete entityDesc.properties.AdditionalReferenceLabel3;
      delete entityDesc.properties.NotedAgentNameLabel3;
      delete entityDesc.properties.NotedAmountLabel3;
      delete entityDesc.properties.NotedDateLabel3;
    }

    _cache = entityDesc;
  }

  return _cache;
}
