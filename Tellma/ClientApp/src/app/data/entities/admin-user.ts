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
            screenUrl: 'admin-users',
            orderby: () => ['Name'],
            format: (item: AdminUserForSave) => item.Name,
            isAdmin: true,
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { control: 'text', label: () => trx.instant('Name') },
                Email: { control: 'text', label: () => trx.instant('User_Email') },
                State: {
                    control: 'state',
                    label: () => trx.instant('State'),
                    choices: ['New', 'Confirmed'],
                    format: (c: string) => {
                        switch (c) {
                            case 'New': return trx.instant('User_New');
                            case 'Confirmed': return trx.instant('User_Confirmed');
                            default: return c;
                        }
                    },
                    color: (c: string) => {
                        switch (c) {
                            case 'New': return '#6c757d';
                            case 'Confirmed': return '#28a745';
                            default: return c;
                        }
                    }
                },
                LastAccess: { control: 'datetime', label: () => trx.instant('User_LastActivity') },
                IsActive: { control: 'boolean', label: () => trx.instant('IsActive') },
                CreatedAt: { control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { control: 'navigation', label: () => trx.instant('CreatedBy'), type: 'User', foreignKeyName: 'CreatedById' },
                ModifiedAt: { control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'ModifiedById' }
            }
        };
    }

    return _cache;
}
