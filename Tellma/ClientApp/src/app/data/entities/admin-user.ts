// tslint:disable:variable-name
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityForSave } from './base/entity-for-save';
import { AdminSettingsForClient } from '../dto/admin-settings-for-client';
import { AdminPermissionForSave, AdminPermission } from './admin-permission';

export interface AdminUserForSave<TPermission = AdminPermissionForSave> extends EntityForSave {
    Name?: string;
    Email?: string;
    Permissions?: TPermission[];
}

export interface AdminUser extends AdminUserForSave<AdminPermission> {
    State: string;
    IsActive?: boolean;
    ExternalId?: string;
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
            isAdmin: true,
            properties: {
                Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') },
                Email: { datatype: 'string', control: 'text', label: () => trx.instant('User_Email') },
                State: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('State'),
                    choices: ['Invited', 'Member'],
                    format: (c: string) => {
                        switch (c) {
                            case 'Invited': return trx.instant('User_Invited');
                            case 'Member': return trx.instant('User_Member');
                            default: return c;
                        }
                    },
                    color: (c: string) => {
                        switch (c) {
                            case 'Invited': return '#6c757d';
                            case 'Member': return '#28a745';
                            default: return c;
                        }
                    }
                },
                LastAccess: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('User_LastActivity') },
                IsActive: { datatype: 'bit', control: 'check', label: () => trx.instant('IsActive') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };
    }

    return _cache;
}
