import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { AdminSettingsForClient } from '../dto/admin-settings-for-client';

export interface IdentityServerUser extends EntityWithKey {
    Email?: string;
    EmailConfirmed?: boolean;
    PasswordSet?: boolean;
    TwoFactorEnabled?: boolean;
    LockoutEnd?: string;
}

let _settings: AdminSettingsForClient;
let _cache: EntityDescriptor;

export function metadata_IdentityServerUser(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.admin;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        _cache = {
            collection: 'IdentityServerUser',
            titleSingular: () => trx.instant('IdentityServerUser'),
            titlePlural: () => trx.instant('IdentityServerUsers'),
            select: ['Email'],
            apiEndpoint: 'identity-server-users',
            masterScreenUrl: 'identity-server-users',
            orderby: () => ['Email'],
            inactiveFilter: null,
            format: (item: IdentityServerUser) => item.Email,
            isAdmin: true,
            properties: {
                Id: { control: 'text', label: () => trx.instant('Id') },
                Email: { control: 'text', label: () => trx.instant('User_Email') },
                EmailConfirmed: { control: 'boolean', label: () => trx.instant('IdentityServerUser_EmailConfirmed') },
                PasswordSet: { control: 'boolean', label: () => trx.instant('IdentityServerUser_PasswordSet') },
                TwoFactorEnabled: { control: 'boolean', label: () => trx.instant('IdentityServerUser_TwoFactorEnabled') },
                LockoutEnd: { control: 'datetime', label: () => trx.instant('IdentityServerUser_LockoutEnd') },
            }
        };
    }

    return _cache;
}
