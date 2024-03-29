// tslint:disable:max-line-length
// tslint:disable:variable-name
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionsForClient } from '../dto/definitions-for-client';
import { AccountTypeResourceDefinitionForSave, AccountTypeResourceDefinition } from './account-type-resource-definition';
import { AccountTypeAgentDefinition, AccountTypeAgentDefinitionForSave } from './account-type-agent-definition';
import { AccountTypeNotedAgentDefinition, AccountTypeNotedAgentDefinitionForSave } from './account-type-noted-agent-definition';
import { AccountTypeNotedResourceDefinition, AccountTypeNotedResourceDefinitionForSave } from './account-type-noted-resource-definition';
import { TimeGranularity } from './base/metadata-types';

export type RequiredAssignment = 'A' | 'E';
export type OptionalAssignment = 'N' | 'A' | 'E';
export type EntryAssignment = 'N' | 'E';

export interface AccountTypeForSave<
  TAgentDef = AccountTypeAgentDefinitionForSave,
  TResourceDef = AccountTypeResourceDefinitionForSave,
  TNotedAgentDef = AccountTypeNotedAgentDefinitionForSave,
  TNotedResourceDef = AccountTypeNotedResourceDefinitionForSave> extends EntityForSave {
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
  EntryTypeParentId?: number;
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
  ReferenceSourceLabel?: string;
  ReferenceSourceLabel2?: string;
  ReferenceSourceLabel3?: string;
  InternalReferenceLabel?: string;
  InternalReferenceLabel2?: string;
  InternalReferenceLabel3?: string;
  NotedAgentNameLabel?: string;
  NotedAgentNameLabel2?: string;
  NotedAgentNameLabel3?: string;
  NotedAmountLabel?: string;
  NotedAmountLabel2?: string;
  NotedAmountLabel3?: string;
  NotedDateLabel?: string;
  NotedDateLabel2?: string;
  NotedDateLabel3?: string;

  AgentDefinitions?: TAgentDef[];
  ResourceDefinitions?: TResourceDef[];
  NotedAgentDefinitions?: TNotedAgentDef[];
  NotedResourceDefinitions?: TNotedResourceDef[];
}

export interface AccountType extends AccountTypeForSave<AccountTypeAgentDefinition, AccountTypeResourceDefinition, AccountTypeNotedAgentDefinition, AccountTypeNotedResourceDefinition> {
  Path?: string;
  Level?: number;
  ActiveChildCount?: number;
  ChildCount?: number;
  IsActive?: boolean;
  IsSystem?: boolean;
  SavedById?: number | string;
  SavedAt?: string;
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
      formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
      properties: {
        Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
        Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
        Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
        Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
        IsMonetary: { datatype: 'bit', control: 'check', label: () => trx.instant('AccountType_IsMonetary') },
        Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
        Concept: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Concept') },
        IsAssignable: { datatype: 'bit', control: 'check', label: () => trx.instant('IsAssignable') },
        StandardAndPure: { datatype: 'bit', control: 'check', label: () => trx.instant('AccountType_StandardAndPure') },
        EntryTypeParentId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('AccountType_EntryTypeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        EntryTypeParent: { datatype: 'entity', control: 'EntryType', label: () => trx.instant('AccountType_EntryTypeParent'), foreignKeyName: 'EntryTypeParentId' },
        Time1Label: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.primaryPostfix },
        Time1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.secondaryPostfix },
        Time1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time1Label') + ws.ternaryPostfix },
        Time2Label: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.primaryPostfix },
        Time2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.secondaryPostfix },
        Time2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_Time2Label') + ws.ternaryPostfix },
        ExternalReferenceLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.primaryPostfix },
        ExternalReferenceLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.secondaryPostfix },
        ExternalReferenceLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ExternalReferenceLabel') + ws.ternaryPostfix },
        ReferenceSourceLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ReferenceSourceLabel') + ws.primaryPostfix },
        ReferenceSourceLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ReferenceSourceLabel') + ws.secondaryPostfix },
        ReferenceSourceLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_ReferenceSourceLabel') + ws.ternaryPostfix },
        InternalReferenceLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_InternalReferenceLabel') + ws.primaryPostfix },
        InternalReferenceLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_InternalReferenceLabel') + ws.secondaryPostfix },
        InternalReferenceLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_InternalReferenceLabel') + ws.ternaryPostfix },
        NotedAgentNameLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.primaryPostfix },
        NotedAgentNameLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.secondaryPostfix },
        NotedAgentNameLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAgentNameLabel') + ws.ternaryPostfix },
        NotedAmountLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.primaryPostfix },
        NotedAmountLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.secondaryPostfix },
        NotedAmountLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedAmountLabel') + ws.ternaryPostfix },
        NotedDateLabel: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.primaryPostfix },
        NotedDateLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.secondaryPostfix },
        NotedDateLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('AccountType_NotedDateLabel') + ws.ternaryPostfix },

        // tree stuff
        Path: { datatype: 'string', control: 'text', label: () => trx.instant('TreePath') },
        ParentId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('TreeParent')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Parent: { datatype: 'entity', control: 'AccountType', label: () => trx.instant('TreeParent'), foreignKeyName: 'ParentId' },
        ChildCount: { datatype: 'numeric', control: 'number', label: () => trx.instant('TreeChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true, noSeparator: false },
        ActiveChildCount: { datatype: 'numeric', control: 'number', label: () => trx.instant('TreeActiveChildCount'), minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true, noSeparator: false },
        Level: { datatype: 'numeric', control: 'number', label: () => trx.instant('TreeLevel'), minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true, noSeparator: false },

        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
        IsSystem: { datatype: 'bit', control: 'check', label: () => trx.instant('IsSystem') },

        // Audit info
        SavedById: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        SavedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'SavedById' },
        SavedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete entityDesc.properties.Name2;
      delete entityDesc.properties.Description2;
      delete entityDesc.properties.Time1Label2;
      delete entityDesc.properties.Time2Label2;
      delete entityDesc.properties.ExternalReferenceLabel2;
      delete entityDesc.properties.ReferenceSourceLabel2;
      delete entityDesc.properties.InternalReferenceLabel2;
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
      delete entityDesc.properties.ReferenceSourceLabel3;
      delete entityDesc.properties.InternalReferenceLabel3;
      delete entityDesc.properties.NotedAgentNameLabel3;
      delete entityDesc.properties.NotedAmountLabel3;
      delete entityDesc.properties.NotedDateLabel3;
    }

    _cache = entityDesc;
  }

  return _cache;
}
