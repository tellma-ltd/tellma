// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';
import { AdminSettingsForClient } from '../dto/admin-settings-for-client';
import { AdminPermissionForSave, AdminPermission } from './admin-permission';
import { TimeGranularity } from './base/metadata-types';

export interface AdminUserForSave<TPermission = AdminPermissionForSave> extends EntityForSave {
    Name?: string;
    Email?: string;
    ClientId?: string;
    IsService?: boolean;
    Permissions?: TPermission[];
}

export interface AdminUser extends AdminUserForSave<AdminPermission> {
    IsActive?: boolean;
    ExternalId?: string;
    InvitedAt?: string;
    State?: 0 | 1 | 2;
    LastAccess?: string;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

let _settings: AdminSettingsForClient;
let _cache: EntityDescriptor;

export function metadata_AdminUser(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.admin;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        _cache = {
            collection: 'AdminUser',
            titleSingular: () => trx.instant('AdminUser'),
            titlePlural: () => trx.instant('AdminUsers'),
            select: ['Name'],
            apiEndpoint: 'admin-users',
            masterScreenUrl: 'admin-users',
            orderby: () => ['Name'],
            inactiveFilter: 'IsActive eq true',
            format: (item: AdminUserForSave) => item.Name,
            formatFromVals: (vals: any[]) => vals[0],
            isAdmin: true,
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') },
                Email: { datatype: 'string', control: 'text', label: () => trx.instant('User_Email') },
                ClientId: { datatype: 'string', control: 'text', label: () => trx.instant('User_ClientId') },
                IsService: { datatype: 'bit', control: 'check', label: () => trx.instant('User_IsService') },
                InvitedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('User_InvitedAt'), granularity: TimeGranularity.minutes },
                State: {
                  datatype: 'string',
                  control: 'choice',
                  label: () => trx.instant('State'),
                  choices: [0, 1, 2],
                  format: (c: number) => {
                    switch (c) {
                      case 0: return trx.instant('User_New');
                      case 1: return trx.instant('User_Invited');
                      case 2: return trx.instant('User_Member');
                      default: return c;
                    }
                  },
                  color: (c: number) => {
                    switch (c) {
                      case 0: return '#6c757d'; // grey
                      case 1: return '#6c757d'; // grey
                      case 2: return '#28a745'; // green
                      default: return '#000000'; // black
                    }
                  }
                },
                LastAccess: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('User_LastActivity'), granularity: TimeGranularity.minutes },
                IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };
    }

    return _cache;
}
