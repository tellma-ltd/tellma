// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export class VoucherBookletForSave extends EntityWithKey {
  Name: string;
  Name2: string;
  Name3: string;
  VoucherTypeId: string;
  StringPrefix: string;
  NumericLength: number;
  RangeStarts: number;
  RangeEnds: number;
}

export class VoucherBooklet extends VoucherBookletForSave {
  IsActive: boolean;
}

export class ResourcePickForSave extends EntityWithKey {
  Code: string;
  ResourceId: number | string;
  ProductionDate: string;
  ExpiryDate: string;
  MonetaryValue: number;
  Mass: number;
  Volume: number;
  Area: number;
  Length: number;
  Time: number;
  Count: number;
  Beneficiary: string;
  IssuingBankAccountId: number | string;
  IssuingBankId: number | string;
}

export class ResourcePick extends ResourcePickForSave {
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}


export class ResponsibilityCenterForSave extends EntityWithKey {
  ParentId: number | string;
  Name: string;
  Name2: string;
  Name3: string;
  Code: string;
}

export class ResponsibilityCenter extends ResponsibilityCenterForSave {
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

export class IfrsAccountClassification extends EntityWithKey {
  Level: number;
  ActiveChildCount: number;
  ChildCount: number;
  IsLeaf: boolean;
  Label: string;
  Label2: string;
  Label3: string;
  Documentation: string;
  Documentation2: string;
  Documentation3: string;
  EffectiveDate: string;
  ExpiryDate: string;
  IsActive: boolean;
  Node: string;
  ParentNode: string;
}

export class IfrsEntryClassification extends EntityWithKey {
  Level: number;
  ParentId: number | string;
  ActiveChildCount: number;
  ChildCount: number;
  IsLeaf: boolean;
  Label: string;
  Label2: string;
  Label3: string;
  Documentation: string;
  Documentation2: string;
  Documentation3: string;
  EffectiveDate: string;
  ExpiryDate: string;
  ForDebit: boolean;
  ForCredit: boolean;
  IsActive: boolean;
  Node: string;
  ParentNode: string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);

let _voucherBookletLang: string;
let _voucherBookletSettings: SettingsForClient;
let _voucherBookletCache: EntityDescriptor;

export function metadata_VoucherBooklet(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _voucherBookletLang || ws.settings !== _voucherBookletSettings) {
    _voucherBookletLang = trx.currentLang;
    _voucherBookletSettings = ws.settings;
    _voucherBookletCache = {
      titleSingular: 'Voucher Type',
      titlePlural: 'Voucher Types',
      select: ['VoucherTypeId'],
      apiEndpoint: 'voucher-booklets',
      orderby: ['VoucherTypeId'],
      format: (item: VoucherBookletForSave) => item.VoucherTypeId,
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },

        // Temp
        VoucherTypeId: { control: 'number', label: 'Voucher Type', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        StringPrefix: { control: 'text', label: 'String Prefix' },
        NumericLength: { control: 'number', label: 'Numeric Length', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        RangeStarts: { control: 'number', label: 'Range Starts', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        RangeEnds: { control: 'number', label: 'Range Ends', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        // End Temp

        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
      }
    };
  }

  return _voucherBookletCache;
}

let _resourcePickLang: string;
let _resourcePickSettings: SettingsForClient;
let _resourcePickCache: EntityDescriptor;

export function metadata_ResourcePick(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _resourcePickLang || ws.settings !== _resourcePickSettings) {
    _resourcePickLang = trx.currentLang;
    _resourcePickSettings = ws.settings;
    _resourcePickCache = {
      titleSingular: 'Resource Pick',
      titlePlural: 'Resource Picks',
      select: ['Code'],
      apiEndpoint: 'resource-picks',
      orderby: ['Code'],
      format: (item: ResourcePick) => item.Code,
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Code: { control: 'text', label: trx.instant('Code') },

        // Temp
        ResourceId: { control: 'number', label: 'Resource', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Resource: { control: 'navigation', label: 'Resource', type: 'Resource', foreignKeyName: 'ResourceId' },
        ProductionDate: { control: 'date', label: 'Production Date' },
        ExpiryDate: { control: 'date', label: 'Expiry Date' },
        MonetaryValue: { control: 'number', label: 'Monetary Value', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Mass: { control: 'number', label: 'Mass', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Volume: { control: 'number', label: 'Volume', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Area: { control: 'number', label: 'Area', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Length: { control: 'number', label: 'Length', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Time: { control: 'number', label: 'Time', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Count: { control: 'number', label: 'Count', minDecimalPlaces: 2, maxDecimalPlaces: 2, alignment: 'right' },
        Beneficiary: { control: 'text', label: 'Beneficiary' },
        IssuingBankAccountId: { control: 'number', label: 'Issuing Bank Account Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        IssuingBankId: { control: 'number', label: 'Issuing Bank Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        // End Temp

        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };
  }

  return _resourcePickCache;
}


let _responsibilityCenterLang: string;
let _responsibilityCenterSettings: SettingsForClient;
let _responsibilityCenterCache: EntityDescriptor;

export function metadata_ResponsibilityCenter(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _responsibilityCenterLang || ws.settings !== _responsibilityCenterSettings) {
    _responsibilityCenterLang = trx.currentLang;
    _responsibilityCenterSettings = ws.settings;
    _responsibilityCenterCache = {
      titleSingular: 'Responsibility Center',
      titlePlural: 'Responsibility Centers',
      select: _select,
      apiEndpoint: 'responsibility-centers',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },
        ParentId: { control: 'number', label: 'Area Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Parent: {
          control: 'navigation', label: trx.instant('TreeParent'), type: 'ResponsibilityCenter',
          foreignKeyName: 'ResponsibilityCenterId'
        },
        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };
  }

  return _responsibilityCenterCache;
}

const _label = ['', '2', '3'].map(pf => 'Label' + pf);
export function metadata_IfrsAccountClassification(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  return {
    titleSingular: 'IFRS Account Classification',
    titlePlural: 'IFRS Account Classifications',
    select: _label,
    apiEndpoint: 'ifrs-account-classifications',
    orderby: ws.isSecondaryLanguage ? [_label[1], _label[0]] : ws.isTernaryLanguage ? [_label[2], _label[0]] : [_label[0]],
    format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _label[0]),
    properties: {
      Id: { control: 'text', label: trx.instant('Code') },
      Label: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.primaryPostfix },
      Label2: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.secondaryPostfix },
      Label3: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.ternaryPostfix },
      Documentation: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.primaryPostfix },
      Documentation2: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.secondaryPostfix },
      Documentation3: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.ternaryPostfix },
      EffectiveDate: { control: 'date', label: trx.instant('IfrsConcepts_EffectiveDate') },
      ExpiryDate: { control: 'date', label: trx.instant('IfrsConcepts_ExpiryDate') },
      ForDebit: { control: 'boolean', label: trx.instant('IfrsNotes_ForDebit') },
      ForCredit: { control: 'boolean', label: trx.instant('IfrsNotes_ForCredit') },
      IsActive: { control: 'boolean', label: trx.instant('IsActive') },

      // tree stuff
      ChildCount: {
        control: 'number', label: trx.instant('TreeChildCount'), minDecimalPlaces: 0,
        maxDecimalPlaces: 0, alignment: 'right'
      },
      ActiveChildCount: {
        control: 'number', label: trx.instant('TreeActiveChildCount'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Level: {
        control: 'number', label: trx.instant('TreeLevel'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Parent: {
        control: 'navigation', label: trx.instant('TreeParent'),
        type: 'IfrsAccountClassification', foreignKeyName: 'ParentId'
      },
    }
  };
}

export function metadata_IfrsEntryClassification(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  return {
    titleSingular: 'IFRS Entry Classification',
    titlePlural: 'IFRS Entry Classification',
    select: _label,
    apiEndpoint: 'ifrs-entry-classifications',
    orderby: ws.isSecondaryLanguage ? [_label[1], _label[0]] : ws.isTernaryLanguage ? [_label[2], _label[0]] : [_label[0]],
    format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _label[0]),
    properties: {
      Id: { control: 'text', label: trx.instant('Code') },
      Label: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.primaryPostfix },
      Label2: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.secondaryPostfix },
      Label3: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.ternaryPostfix },
      Documentation: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.primaryPostfix },
      Documentation2: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.secondaryPostfix },
      Documentation3: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.ternaryPostfix },
      EffectiveDate: { control: 'date', label: trx.instant('IfrsConcepts_EffectiveDate') },
      ExpiryDate: { control: 'date', label: trx.instant('IfrsConcepts_ExpiryDate') },
      IsActive: { control: 'boolean', label: trx.instant('IsActive') },

      // tree stuff
      ChildCount: {
        control: 'number', label: trx.instant('TreeChildCount'), minDecimalPlaces: 0,
        maxDecimalPlaces: 0, alignment: 'right'
      },
      ActiveChildCount: {
        control: 'number', label: trx.instant('TreeActiveChildCount'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Level: {
        control: 'number', label: trx.instant('TreeLevel'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Parent: {
        control: 'navigation', label: trx.instant('TreeParent'),
        type: 'IfrsAccountClassification', foreignKeyName: 'ParentId'
      },
    }
  };
}


let _accountsLang: string;
let _accountsSettings: SettingsForClient;
let _accountsCache: EntityDescriptor;

export function metadata_Account(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _accountsLang || ws.settings !== _accountsSettings) {
    _accountsLang = trx.currentLang;
    _accountsSettings = ws.settings;
    _accountsCache = {
      titleSingular: trx.instant('Account'),
      titlePlural: trx.instant('Accounts'),
      select: _select,
      apiEndpoint: 'accounts',
      orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
      format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
      properties: {
        Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
        Code: { control: 'text', label: trx.instant('Code') },

        // Temp
        AccountClassificationId: { control: 'number', label: 'Resource', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        AccountClassification: {
          control: 'navigation', label: 'Classification', type: 'AccountClassification', foreignKeyName: 'AccountClassificationId'
        },
        IfrsAccountClassificationId: { control: 'text', label: 'IFRS Classification Id' },
        IfrsAccountClassification: {
          control: 'navigation', label: 'IFRS Classification',
          type: 'IfrsAccountClassification', foreignKeyName: 'IfrsAccountClassificationId'
        },
        PartyReference: { control: 'text', label: 'Party Reference' },
        IsMultiEntryClassification: { control: 'boolean', label: 'Is Multi-Entry Classification' },
        IfrsEntryClassificationId: { control: 'text', label: 'IFRS Entry Classification Id' },
        IfrsEntryClassification: {
          control: 'navigation', label: 'IFRS Entry Classification',
          type: 'IfrsEntryClassification', foreignKeyName: 'IfrsEntryClassificationId'
        },
        IsMultiAgent: { control: 'boolean', label: 'Is Multi-Agent' },
        AgentId: { control: 'number', label: 'Agent Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Agent: { control: 'navigation', label: 'Agent', type: 'Agent', foreignKeyName: 'AgentId' },
        IsMultiResponsibilityCenter: { control: 'boolean', label: 'Is Multi-Responsibility Center' },
        ResponsibilityCenterId: { control: 'number', label: 'Responsibility Center Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        ResponsibilityCenter: {
          control: 'navigation', label: 'Responsibility Center', type: 'ResponsibilityCenter', foreignKeyName: 'ResponsibilityCenterId'
        },
        IsMultiResource: { control: 'boolean', label: 'Is Multi-Resource' },
        ResourceId: { control: 'number', label: 'Resource Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Resource: { control: 'navigation', label: 'Resource', type: 'Resource', foreignKeyName: 'ResourceId' },
        ResponsibleActorId: { control: 'number', label: 'Responsible Actor Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        ResponsibleActor: {
          control: 'navigation', label: 'Responsible Actor', type: 'ResponsibleActor', foreignKeyName: 'ResponsibleActorId'
        },
        ResponsibleRoleId: { control: 'number', label: 'Responsible Role Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        ResponsibleRole: {
          control: 'navigation', label: 'Responsible Role', type: 'ResponsibleRole', foreignKeyName: 'ResponsibleRoleId'
        },
        CustodianActorId: { control: 'number', label: 'Custodian Actor Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        CustodianActor: {
          control: 'navigation', label: 'Custodian Actor', type: 'CustodianActor', foreignKeyName: 'CustodianActorId'
        },
        CustodianRoleId: { control: 'number', label: 'Custodian Role Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        CustodianRole: {
          control: 'navigation', label: 'Custodian Role', type: 'CustodianRole', foreignKeyName: 'CustodianRoleId'
        },

        // End Temp

        IsActive: { control: 'boolean', label: trx.instant('IsActive') },
        CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
        CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
        ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
        ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
      }
    };

    if (!ws.settings.SecondaryLanguageId) {
      delete _accountsCache.properties.Name2;
      delete _accountsCache.properties.Description2;
    }

    if (!ws.settings.TernaryLanguageId) {
      delete _accountsCache.properties.Name3;
      delete _accountsCache.properties.Description3;
    }
  }

  return _accountsCache;
}
