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
      titleSingular: () =>  'Voucher Type',
      titlePlural: () =>  'Voucher Types',
      select: ['VoucherTypeId'],
      apiEndpoint: 'voucher-booklets',
      masterScreenUrl: 'voucher-booklets',
      orderby: () => ['VoucherTypeId'],
      inactiveFilter: 'IsActive eq true',
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
