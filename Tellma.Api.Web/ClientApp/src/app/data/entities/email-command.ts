// tslint:disable:variable-name
// tslint:disable:max-line-length
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from '../workspace.service';
import { EntityForSave } from './base/entity-for-save';
import { EntityDescriptor } from './base/metadata';
import { TimeGranularity } from './base/metadata-types';

export interface EmailCommand extends EntityForSave {
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

export function metadata_EmailCommand(_: WorkspaceService, trx: TranslateService): EntityDescriptor {

  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (!_cache) {
    _cache = {
      collection: 'EmailCommand',
      titleSingular: () => trx.instant('EmailCommand'),
      titlePlural: () => trx.instant('EmailCommands'),
      select: ['Caption'],
      apiEndpoint: 'email-commands',
      masterScreenUrl: 'email-commands',
      inactiveFilter: null,
      orderby: () => ['Caption'],
      format: (item: EmailCommand) => item.Caption,
      formatFromVals: (vals: any[]) => vals[0],
      properties: {
        Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        TemplateId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Template: { datatype: 'entity', control: 'EmailTemplate', label: () => trx.instant('NotificationCommand_Template'), foreignKeyName: 'TemplateId' },
        EntityId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },

        Successes: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_Successes'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        Errors: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_Errors'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
        Total: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationCommand_Total'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },

        Caption: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationCommand_Caption') },
        CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
        CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
      }
    };
  }

  return _cache;
}
