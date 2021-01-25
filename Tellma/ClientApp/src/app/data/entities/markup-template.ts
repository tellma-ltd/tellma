// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityForSave } from './base/entity-for-save';
import { supportedCultures } from '../supported-cultures';

export type MarkupTemplateUsage = 'QueryByFilter' | 'QueryById';

export interface MarkupTemplateForSave extends EntityForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;

    Usage?: MarkupTemplateUsage;
    Collection?: string;
    DefinitionId?: number;
    MarkupLanguage?: string;
    SupportsPrimaryLanguage?: boolean;
    SupportsSecondaryLanguage?: boolean;
    SupportsTernaryLanguage?: boolean;
    DownloadName?: string;
    Body?: string;
    IsDeployed?: boolean;
}

export interface MarkupTemplate extends MarkupTemplateForSave {
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_MarkupTemplate(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'MarkupTemplate',
            titleSingular: () => trx.instant('MarkupTemplate'),
            titlePlural: () => trx.instant('MarkupTemplates'),
            select: _select,
            apiEndpoint: 'markup-templates',
            masterScreenUrl: 'markup-templates',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Name: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.primaryPostfix },
                Name2: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.secondaryPostfix },
                Name3: { datatype: 'string', control: 'text', label: () => trx.instant('Name') + ws.ternaryPostfix },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                Usage: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('MarkupTemplate_Usage'),
                    choices: ['QueryByFilter', 'QueryById'],
                    format: (c: number | string) => {
                        return !!c ? 'MarkupTemplate_Usage_' + c : '';
                    }
                },
                Collection: { datatype: 'string', control: 'text', label: () => trx.instant('MarkupTemplate_Collection') },
                DefinitionId: { datatype: 'numeric', control: 'number', label: () => trx.instant('MarkupTemplate_DefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MarkupLanguage: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('MarkupTemplate_MarkupLanguage'),
                    choices: ['text/html'],
                    format: (c: number | string) => {
                        switch (c) {
                            case 'text/html': return 'HTML';
                            default: return '';
                        }
                    }
                },
                SupportsPrimaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('MarkupTemplate_Supports') + ws.primaryPostfix },
                SupportsSecondaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('MarkupTemplate_Supports') + ws.secondaryPostfix },
                SupportsTernaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('MarkupTemplate_Supports') + ws.ternaryPostfix },
                DownloadName: { datatype: 'string', control: 'text', label: () => trx.instant('MarkupTemplate_DownloadName') },
                Body: { datatype: 'string', control: 'text', label: () => trx.instant('MarkupTemplate_Body') },
                IsDeployed: { datatype: 'bit', control: 'check', label: () => trx.instant('MarkupTemplate_IsDeployed') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt') },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt') },
                ModifiedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'ModifiedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
            delete entityDesc.properties.Description2;
            delete entityDesc.properties.SupportsSecondaryLanguage;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
            delete entityDesc.properties.Description3;
            delete entityDesc.properties.SupportsTernaryLanguage;
        }

        if (!entityDesc.properties.SupportsSecondaryLanguage && !entityDesc.properties.SupportsTernaryLanguage) {
            // Meaningless on its own
            delete entityDesc.properties.SupportsPrimaryLanguage;
        }

        _cache = entityDesc;
    }

    return _cache;
}
