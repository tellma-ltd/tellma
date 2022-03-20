// tslint:disable:variable-name
// tslint:disable:max-line-length
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from '../workspace.service';
import { EntityForSave } from './base/entity-for-save';
import { EntityDescriptor } from './base/metadata';
import { TimeGranularity } from './base/metadata-types';

export interface MessageCommand extends EntityForSave {
    TemplateId?: number;
    EntityId?: number;
    Successes?: number;
    Errors?: number;
    Total?: number;
    Caption?: string;
    CreatedAt?: string;
    CreatedById?: string;
}

let _cache: EntityDescriptor;

export function metadata_MessageCommand(_: WorkspaceService, trx: TranslateService): EntityDescriptor {

  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (!_cache) {
    _cache = {
      collection: 'MessageCommand',
      titleSingular: () => trx.instant('MessageCommand'),
      titlePlural: () => trx.instant('MessageCommands'),
      select: ['Caption'],
      apiEndpoint: 'message-commands',
      masterScreenUrl: 'message-commands',
      inactiveFilter: null,
      orderby: () => ['Caption'],
      format: (item: MessageCommand) => item.Caption,
      formatFromVals: (vals: any[]) => vals[0],
      properties: {
        Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        TemplateId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
        Template: { datatype: 'entity', control: 'MessageTemplate', label: () => trx.instant('NotificationCommand_Template'), foreignKeyName: 'TemplateId' },
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
