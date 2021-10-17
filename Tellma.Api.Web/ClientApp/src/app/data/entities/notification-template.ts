// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Collection, Control, EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityForSave } from './base/entity-for-save';
import { TimeGranularity } from './base/metadata-types';

export type TemplateUsage = 'FromSearchAndDetails' | 'FromDetails';
export type Channel = 'Email' | 'Sms';
export type Trigger = 'Automatic' | 'Manual';
export type Cardinality = 'Single' | 'Bulk';
export type AddressType = 'User' | 'Text';

export interface NotificationTemplateForSave<
        TParameter = NotificationTemplateParameterForSave,
        TAttachment = NotificationTemplateAttachmentForSave,
        TSubscriber = NotificationTemplateSubscriberForSave> extends EntityForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    Channel?: Channel;
    Trigger?: Trigger;
    Cardinality?: Cardinality;
    ListExpression?: string;
    Schedule?: string;
    ConditionExpression?: string;
    MaximumRenotify?: number;
    Usage?: TemplateUsage;
    Collection?: Collection;
    DefinitionId?: number;
    ReportDefinitionId?: number;
    Subject?: string;
    Body?: string;
    AddressExpression?: string;
    Caption?: string;
    IsDeployed?: boolean;
    Parameters?: TParameter[];
    Attachments?: TAttachment[];
    Subscribers?: TSubscriber[];
}

export interface NotificationTemplate extends NotificationTemplateForSave<
        NotificationTemplateParameter, NotificationTemplateAttachment, NotificationTemplateSubscriber> {
    ShowInMainMenu?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

export interface NotificationTemplateParameterForSave extends EntityForSave {
    Key?: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    IsRequired?: boolean;
    Control?: Control;
    ControlOptions?: string; // JSON
}

export interface NotificationTemplateParameter extends NotificationTemplateParameterForSave {
    NotificationTemplateId?: number;
}

export interface NotificationTemplateAttachmentForSave extends EntityForSave {
    ContextOverride?: string;
    DownloadNameOverride?: string;
    PrintingTemplateId?: number;
}

export interface NotificationTemplateAttachment extends NotificationTemplateAttachmentForSave {
    NotificationTemplateId?: number;
}

export interface NotificationTemplateSubscriberForSave extends EntityForSave {
    AddressType?: AddressType;
    UserId?: number;
    Email?: string;
    Phone?: string;
}

export interface NotificationTemplateSubscriber extends NotificationTemplateSubscriberForSave {
    NotificationTemplateId?: number;
    LastNotificationCount?: number;
    LastNotificationHash?: string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_NotificationTemplate(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'NotificationTemplate',
            titleSingular: () => trx.instant('NotificationTemplate'),
            titlePlural: () => trx.instant('NotificationTemplates'),
            select: _select,
            apiEndpoint: 'notification-templates',
            masterScreenUrl: 'notification-templates',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                Channel: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('NotificationTemplate_Channel'),
                    choices: ['Email', 'Sms'],
                    format: (c: number | string) => {
                        return !!c ? trx.instant('Channel_' + c) : '';
                    }
                },
                Trigger: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('NotificationTemplate_Trigger'),
                    choices: ['Automatic', 'Manual'],
                    format: (c: number | string) => {
                        return !!c ? trx.instant('Trigger_' + c) : '';
                    }
                },
                Cardinality: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('NotificationTemplate_Cardinality'),
                    choices: ['Single', 'Bulk'],
                    format: (c: number | string) => {
                        return !!c ? trx.instant('Cardinality_' + c) : '';
                    }
                },
                ListExpression: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_ListExpression') },
                Schedule: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_Schedule') },
                ConditionExpression: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_ConditionExpression') },
                MaximumRenotify: { datatype: 'numeric', control: 'number', label: () => trx.instant('NotificationTemplate_MaximumRenotify'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: false },
                Usage: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('Template_Usage'),
                    choices: ['FromSearchAndDetails', 'FromDetails'],
                    format: (c: number | string) => {
                        return !!c ? 'Template_Usage_' + c : '';
                    }
                },
                Collection: { datatype: 'string', control: 'text', label: () => trx.instant('Template_Collection') },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Template_DefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ReportDefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Template_ReportDefinitionId')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                ReportDefinition: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Template_ReportDefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Subject: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_Subject') },
                Body: { datatype: 'string', control: 'text', label: () => trx.instant('Template_Body') },
                AddressExpression: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_AddressExpression') },
                Caption: { datatype: 'string', control: 'text', label: () => trx.instant('NotificationTemplate_Caption') },
                IsDeployed: { datatype: 'bit', control: 'check', label: () => trx.instant('Template_IsDeployed') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
            delete entityDesc.properties.Description2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
            delete entityDesc.properties.Description3;
        }

        _cache = entityDesc;
    }

    return _cache;
}
