// tslint:disable:max-line-length
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { TimeGranularity } from './base/metadata-types';

export type MessageState = -4 | -3 | -2 | -1 | 0 | 1 | 2 | 3 | 4;
const messageStates = [-4, -3, -2, -1, 0, 1, 2, 3, 4];

export interface MessageForQuery extends EntityWithKey {
    PhoneNumber?: string;
    Content?: string;
    State?: MessageState;
    ErrorMessage?: string;
    CommandId?: number;
    StateSince?: string;
    CreatedAt?: string;
}

let _cache: EntityDescriptor;

export function metadata_Message(_: WorkspaceService, trx: TranslateService): EntityDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    const entityDesc: EntityDescriptor = {
        collection: 'MessageForQuery',
        titleSingular: () => trx.instant('Message'),
        titlePlural: () => trx.instant('Messages'),
        select: ['Content', 'PhoneNumber'],
        apiEndpoint: 'messages',
        masterScreenUrl: 'messages',
        orderby: () => ['Content'],
        inactiveFilter: null, // No inactive filter
        format: (item: MessageForQuery) => item.Content,
        formatFromVals: (vals: any[]) => vals[0],
        properties: {
            Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
            PhoneNumber: { datatype: 'string', control: 'text', label: () => trx.instant('Message_PhoneNumber') },
            Content: { datatype: 'string', control: 'text', label: () => trx.instant('Message_Content') },
            State: {
                datatype: 'numeric',
                control: 'choice',
                label: () => trx.instant('State'),
                choices: messageStates,
                format: (state: number) => {
                    let prefix = '';
                    if (state < 0) {
                        prefix = 'minus_';
                    }

                    if (Math.abs(state) <= 2) {
                        return trx.instant('Notification_State_' + prefix + Math.abs(state));
                    } else if (!!state) {
                        return trx.instant('Message_State_' + prefix + Math.abs(state));
                    } else {
                        return '';
                    }
                },
                color: (state: number) => {
                    if (state < 0) {
                        return '#dc3545'; // Red
                    } else if (state < 3) {
                        return '#6c757d'; // Grey
                    } else {
                        return '#28a745'; // Green
                    }
                }
            },
            ErrorMessage: { datatype: 'string', control: 'text', label: () => trx.instant('Message_ErrorMessage') },
            CommandId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Notification_Command')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
            Command: { datatype: 'entity', control: 'NotificationCommand', label: () => trx.instant('Notification_Command'), foreignKeyName: 'CommandId' },
            StateSince: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('StateSince'), granularity: TimeGranularity.minutes },
            CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
        }
    };

    _cache = entityDesc;

    return _cache;
}
