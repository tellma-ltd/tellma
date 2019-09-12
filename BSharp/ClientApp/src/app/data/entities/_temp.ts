// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from './settings';

export class VoucherBookletForSave extends EntityWithKey {
    Name: string;
    Name2: string;
    Name3: string;
    VoucherTypeId: number | string;
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

export class ResourceForSave extends EntityWithKey {
    Name: string;
    Name2: string;
    Name3: string;
    Code: string;
    ResourceType: string;
    ResourceClassificationId: number | string;
    IsBatch: boolean;
    UnitId: number | string;
    UnitMonetaryValue: number;
    CurrencyId: number | string;
    UnitMass: number;
    MassUnitId: number | string;
    UnitVolume: number;
    VolumeUnitId: number | string;
    UnitArea: number;
    AreaUnitId: number | string;
    UnitLength: number;
    LengthUnitId: number | string;
    UnitTime: number;
    TimeUnitId: number | string;
    UnitCount: number;
    CountUnitId: number | string;
    SystemCode: string;
    Memo: string;
    CustomsReference: string;
    UniversalProductCode: string;
    PreferredSupplierId: number | string;
    ExpenseAccountId: number | string;
    RevenueAccountId: number | string;
    ProductCategoryId: number | string;
}

export class Resource extends ResourceForSave {
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

let _resourceLang: string;
let _resourceSettings: SettingsForClient;
let _resourceCache: EntityDescriptor;

export function metadata_Resource(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (trx.currentLang !== _resourceLang || ws.settings !== _resourceSettings) {
        _resourceLang = trx.currentLang;
        _resourceSettings = ws.settings;
        _resourceCache = {
            select: _select,
            apiEndpoint: 'resources',
            orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { control: 'text', label: trx.instant('Name') + ws.primaryPostfix },
                Name2: { control: 'text', label: trx.instant('Name') + ws.secondaryPostfix },
                Name3: { control: 'text', label: trx.instant('Name') + ws.ternaryPostfix },
                Code: { control: 'text', label: trx.instant('Code') },

                // Temp
                ResourceType: { control: 'text', label: 'Resource Type' },
                ResourceClassificationId: {
                    control: 'number', label: 'Resource Classification Id', minDecimalPlaces: 0, maxDecimalPlaces: 0
                },

                // ResourceClassification: TODO
                IsBatch: { control: 'boolean', label: 'Is Batch' },
                UnitId: { control: 'number', label: 'Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Unit: { control: 'navigation', label: 'Unit', type: 'MeasurementUnit', foreignKeyName: 'UnitId' },
                CurrencyId: { control: 'number', label: 'Currency Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Currency: { control: 'navigation', label: 'Currency', type: 'MeasurementUnit', foreignKeyName: 'CurrencyId' },
                UnitMass: { control: 'number', label: 'Unit Mass', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MassUnitId: { control: 'number', label: 'Mass Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MassUnit: { control: 'navigation', label: 'Mass Unit', type: 'MeasurementUnit', foreignKeyName: 'MassUnitId' },
                UnitVolume: { control: 'number', label: 'Unit Volume', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                VolumeUnitId: { control: 'number', label: 'Volume Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                VolumeUnit: { control: 'navigation', label: 'Volume Unit', type: 'MeasurementUnit', foreignKeyName: 'VolumeUnitId' },
                UnitArea: { control: 'number', label: 'Unit Area', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AreaUnitId: { control: 'number', label: 'Area Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AreaUnit: { control: 'navigation', label: 'Area Unit', type: 'MeasurementUnit', foreignKeyName: 'AreaUnitId' },
                UnitLength: { control: 'number', label: 'Unit Length', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LengthUnitId: { control: 'number', label: 'Length Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                LengthUnit: { control: 'navigation', label: 'Length Unit', type: 'MeasurementUnit', foreignKeyName: 'LengthUnitId' },
                UnitTime: { control: 'number', label: 'Unit Time', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                TimeUnitId: { control: 'number', label: 'Time Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                TimeUnit: { control: 'navigation', label: 'Time Unit', type: 'MeasurementUnit', foreignKeyName: 'TimeUnitId' },
                UnitCount: { control: 'number', label: 'Unit Count', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CountUnitId: { control: 'number', label: 'Count Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                CountUnit: { control: 'navigation', label: 'Count Unit', type: 'MeasurementUnit', foreignKeyName: 'CountUnitId' },
                SystemCode: { control: 'text', label: 'System Code' },
                Memo: { control: 'text', label: 'Memo' },
                CustomsReference: { control: 'text', label: 'Customs Reference' },
                UniversalProductCode: { control: 'text', label: 'Universal Product Code' },
                ProductCategoryId: { control: 'number', label: 'Product Category Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ProductCategory: {
                    control: 'navigation', label: 'Product Category', type: 'ProductCategory', foreignKeyName: 'ProductCategoryId'
                },
                // TODO: PreferredSupplierId, ExpenseAccountId, RevenueAccountId

                // End Temp

                IsActive: { control: 'boolean', label: trx.instant('IsActive') },
                CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };
    }

    return _resourceCache;
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
        ParentId : { control: 'number', label: 'Area Unit Id', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
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
