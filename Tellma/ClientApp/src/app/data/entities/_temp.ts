// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { TenantWorkspace, WorkspaceService } from '../workspace.service';
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

let _voucherBookletSettings: SettingsForClient;
let _voucherBookletCache: EntityDescriptor;

export function metadata_VoucherBooklet(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
  const ws = wss.currentTenant;
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (ws.settings !== _voucherBookletSettings) {
    _voucherBookletSettings = ws.settings;
    _voucherBookletCache = {
      collection: 'VoucherBooklet',
      titleSingular: () => 'Voucher Type',
      titlePlural: () => 'Voucher Types',
      select: ['VoucherTypeId'],
      apiEndpoint: 'voucher-booklets',
      masterScreenUrl: 'voucher-booklets',
      orderby: () => ['VoucherTypeId'],
      inactiveFilter: 'IsActive eq true',
      format: (item: VoucherBookletForSave) => item.VoucherTypeId,
      formatFromVals: (vals: any[]) => vals[0],
      properties: {
        Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
        Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
        Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },

        // Temp
        VoucherTypeId: { datatype: 'numeric', control: 'number', label: () => 'Voucher Type', minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        StringPrefix: { datatype: 'string', control: 'text', label: () => 'String Prefix' },
        NumericLength: {
          datatype: 'numeric',
          control: 'number', label: () => 'Numeric Length', minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true
        },
        RangeStarts: {
          datatype: 'numeric',
          control: 'number', label: () => 'Range Starts', minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true
        },
        RangeEnds: {
          datatype: 'numeric',
          control: 'number', label: () => 'Range Ends', minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: true
        },
        // End Temp

        IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
      }
    };
  }

  return _voucherBookletCache;
}
