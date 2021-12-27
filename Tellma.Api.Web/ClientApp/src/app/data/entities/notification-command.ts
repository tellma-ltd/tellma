// tslint:disable:variable-name
// tslint:disable:max-line-length
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from '../workspace.service';
import { EntityForSave } from './base/entity-for-save';
import { EntityWithKey } from './base/entity-with-key';
import { EntityDescriptor } from './base/metadata';
import { TimeGranularity } from './base/metadata-types';

export interface NotificationCommand extends EntityForSave {
    TemplateId?: number;
    EntityId?: number;
    EmailSuccesses?: number;
    EmailErrors?: number;
    EmailTotal?: number;
    SmsSuccesses?: number;
    SmsErrors?: number;
    SmsTotal?: number;
    Caption?: string;
    CreatedAt?: string;
    CreatedById?: string;
}

let _cache: EntityDescriptor;

export function metadata_NotificationCommand(_: WorkspaceService, trx: TranslateService): EntityDescriptor {

  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (!_cache) {
    _cache = {
      collection: 'NotificationCommand',
      titleSingular: () => trx.instant('NotificationCommand'),
      titlePlural: () => trx.instant('NotificationCommands'),
      select: ['Caption'],
      apiEndpoint: 'notification-commands',
      masterScreenUrl: 'notification-commands',
      inactiveFilter: null,
      orderby: () => ['Caption'],
      format: (item: NotificationCommand) => item.Caption,
      formatFromVals: (vals: any[]) => vals[0],
      properties: {
        Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        TemplateId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Template: { datatype: 'entity', control: 'NotificationTemplate', label: () => trx.instant('NotificationCommand_Template'), foreignKeyName: 'TemplateId' },
        EntityId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },

        EmailSuccesses: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_EmailSuccesses'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        EmailErrors: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_EmailErrors'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        EmailTotal: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_EmailTotal'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        SmsSuccesses: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_SmsSuccesses'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        SmsErrors: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_SmsErrors'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        SmsTotal: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_SmsTotal'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },

        Caption: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationCommand_Caption') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
      }
    };
  }

  return _cache;
}
