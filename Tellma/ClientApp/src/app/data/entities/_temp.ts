// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export interface VoucherBookletForSave extends EntityWithKey {
  Name?: string;
  Name2?: string;
  Name3?: string;
  VoucherTypeId?: string;
  StringPrefix?: string;
  NumericLength?: number;
  RangeStarts?: number;
  RangeEnds?: number;
}

export interface VoucherBooklet extends VoucherBookletForSave {
  IsActive?: boolean;
}

export interface IfrsAccountClassification extends EntityWithKey {
  Level?: number;
  ActiveChildCount?: number;
  ChildCount?: number;
  IsLeaf?: boolean;
  Label?: string;
  Label2?: string;
  Label3?: string;
  Documentation?: string;
  Documentation2?: string;
  Documentation3?: string;
  EffectiveDate?: string;
  ExpiryDate?: string;
  IsActive?: boolean;
  Node?: string;
  ParentNode?: string;
}

let _voucherBookletSettings: SettingsForClient;
let _voucherBookletCache: EntityDescriptor;

export function metadata_VoucherBooklet(ws: TenantWorkspace, trx: TranslateService, _subtype: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _voucherBookletSettings) {
    _voucherBookletSettings = ws.settings;
    _voucherBookletCache = {
      collection: 'VoucherBooklet',
      titleSingular: () =>  'Voucher Type',
      titlePlural: () =>  'Voucher Types',
      select: ['VoucherTypeId'],
      apiEndpoint: 'voucher-booklets',
      screenUrl: 'voucher-booklets',
      orderby: ['VoucherTypeId'],
      format: (item: VoucherBookletForSave) => item.VoucherTypeId,
      properties: {
        Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },

        // Temp
        VoucherTypeId: { control: 'number', label: () => 'Voucher Type', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        StringPrefix: { control: 'text', label: () => 'String Prefix' },
        NumericLength: { control: 'number', label: () => 'Numeric Length', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        RangeStarts: { control: 'number', label: () => 'Range Starts', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        RangeEnds: { control: 'number', label: () => 'Range Ends', minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right' },
        // End Temp

        IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
      }
    };
  }

  return _voucherBookletCache;
}

const _label = ['', '2', '3'].map(pf => 'Label' + pf);
export function metadata_IfrsAccountClassification(ws: TenantWorkspace, trx: TranslateService, _: string): EntityDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  return {
    collection: 'IfrsAccountClassification',
    titleSingular: () =>  'IFRS Account Classification',
    titlePlural: () =>  'IFRS Account Classifications',
    select: _label,
    apiEndpoint: 'ifrs-account-classifications',
    screenUrl: 'ifrs-account-classifications',
    orderby: ws.isSecondaryLanguage ? [_label[1], _label[0]] : ws.isTernaryLanguage ? [_label[2], _label[0]] : [_label[0]],
    format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _label[0]),
    properties: {
      Id: { control: 'text', label: () => trx.instant('Code') },
      Label: { control: 'text', label: () => trx.instant('IfrsConcepts_Label') + ws.primaryPostfix },
      Label2: { control: 'text', label: () => trx.instant('IfrsConcepts_Label') + ws.secondaryPostfix },
      Label3: { control: 'text', label: () => trx.instant('IfrsConcepts_Label') + ws.ternaryPostfix },
      Documentation: { control: 'text', label: () => trx.instant('IfrsConcepts_Documentation') + ws.primaryPostfix },
      Documentation2: { control: 'text', label: () => trx.instant('IfrsConcepts_Documentation') + ws.secondaryPostfix },
      Documentation3: { control: 'text', label: () => trx.instant('IfrsConcepts_Documentation') + ws.ternaryPostfix },
      EffectiveDate: { control: 'date', label: () => trx.instant('IfrsConcepts_EffectiveDate') },
      ExpiryDate: { control: 'date', label: () => trx.instant('IfrsConcepts_ExpiryDate') },
      ForDebit: { control: 'boolean', label: () => trx.instant('IfrsNotes_ForDebit') },
      ForCredit: { control: 'boolean', label: () => trx.instant('IfrsNotes_ForCredit') },
      IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },

      // tree stuff
      ChildCount: {
        control: 'number', label: () => trx.instant('TreeChildCount'), minDecimalPlaces: 0,
        maxDecimalPlaces: 0, alignment: 'right'
      },
      ActiveChildCount: {
        control: 'number', label: () => trx.instant('TreeActiveChildCount'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Level: {
        control: 'number', label: () => trx.instant('TreeLevel'),
        minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
      },
      Parent: {
        control: 'navigation', label: () => trx.instant('TreeParent'),
        type: 'IfrsAccountClassification', foreignKeyName: 'ParentId'
      },
    }
  };
}
