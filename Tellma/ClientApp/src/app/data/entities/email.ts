import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';

export type EmailState = -4 | -3 | -2 | -1 | 0 | 1 | 2 | 3 | 4 | 5;
const emailStates = [-4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

export interface EmailForQuery extends EntityWithKey {
    ToEmail?: string;
    Subject?: string;
    Body?: string;
    State?: EmailState;
    ErrorMessage?: string;
    StateSince?: string;
    DeliveredAt?: string;
    OpenedAt?: string;
}

let _cache: EntityDescriptor;

export function metadata_Email(_: WorkspaceService, trx: TranslateService): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    const entityDesc: EntityDescriptor = {
        collection: 'EmailForQuery',
        titleSingular: () => trx.instant('Email'),
        titlePlural: () => trx.instant('Emails'),
        select: ['Subject', 'ToEmail'],
        apiEndpoint: 'emails',
        masterScreenUrl: 'emails',
        orderby: () => ['Subject'],
        inactiveFilter: null, // No inactive filter
        format: (item: EmailForQuery) => item.Subject,
        properties: {
            Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
            ToEmail: { control: 'text', label: () => trx.instant('Email_ToEmail') },
            Subject: { control: 'text', label: () => trx.instant('Email_Subject') },
            Body: { control: 'text', label: () => trx.instant('Email_Body') },
            State: {
                control: 'state',
                label: () => trx.instant('State'),
                choices: emailStates,
                format: (state: EmailState) => {
                    let prefix = '';
                    if (state < 0) {
                        prefix = 'minus_';
                    }

                    if (Math.abs(state) <= 2) {
                        return trx.instant('Notification_State_' + prefix + Math.abs(state));
                    } else if (!!state) {
                        return trx.instant('Email_State_' + prefix + Math.abs(state));
                    } else {
                        return '';
                    }
                },
                color: (state: EmailState) => {
                    if (state < 0) {
                        return '#dc3545'; // Red
                    } else if (state < 3) {
                        return '#6c757d'; // Grey
                    } else {
                        return '#28a745'; // Green
                    }
                }
            },
            ErrorMessage: { control: 'text', label: () => trx.instant('Email_ErrorMessage') },
            StateSince: { control: 'datetime', label: () => trx.instant('StateSince') },
            DeliveredAt: { control: 'datetime', label: () => trx.instant('Email_DeliveredAt') },
            OpenedAt: { control: 'datetime', label: () => trx.instant('Email_OpenedAt') },
        }
    };

    _cache = entityDesc;

    return _cache;
}
