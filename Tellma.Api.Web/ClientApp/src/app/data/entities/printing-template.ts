// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityWithKey } from './base/entity-with-key';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Collection, Control, EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityForSave } from './base/entity-for-save';
import { TimeGranularity } from './base/metadata-types';
import { MainMenuIcon, mainMenuIconPropDescriptor, MainMenuSection, mainMenuSectionPropDescriptor, mainMenuSortKeyPropDescriptor } from './base/definition-common';

export type TemplateUsage = 'FromSearchAndDetails' | 'FromDetails' | 'FromReport' | 'Standalone';

export interface PrintingTemplateForSave<TParameter = PrintingTemplateParameterForSave, TRole = PrintingTemplateRoleForSave> extends EntityForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    Code?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;

    Context?: string;
    Usage?: TemplateUsage;
    Collection?: Collection;
    DefinitionId?: number;
    ReportDefinitionId?: number;
    SupportsPrimaryLanguage?: boolean;
    SupportsSecondaryLanguage?: boolean;
    SupportsTernaryLanguage?: boolean;
    DownloadName?: string;
    Body?: string;
    IsDeployed?: boolean;
    MainMenuSection?: MainMenuSection;
    MainMenuIcon?: MainMenuIcon;
    MainMenuSortKey?: number;
    Parameters?: TParameter[];
    Roles?: TRole[];
}

export interface PrintingTemplate extends PrintingTemplateForSave<PrintingTemplateParameter, PrintingTemplateRole> {
    ShowInMainMenu?: boolean;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}

export interface PrintingTemplateParameterForSave extends EntityForSave {
    Key?: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    IsRequired?: boolean;
    Control?: Control;
    ControlOptions?: string; // JSON
}

export interface PrintingTemplateParameter extends PrintingTemplateParameterForSave {
    PrintingTemplateId?: number;
}

export interface PrintingTemplateRoleForSave extends EntityForSave {
    RoleId?: number;
}

export interface PrintingTemplateRole extends PrintingTemplateRoleForSave {
    PrintingTemplateId?: number;
}

const _select = ['', '2', '3'].map(pf => 'Name' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_PrintingTemplate(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'PrintingTemplate',
            titleSingular: () => trx.instant('PrintingTemplate'),
            titlePlural: () => trx.instant('PrintingTemplates'),
            select: _select,
            apiEndpoint: 'printing-templates',
            masterScreenUrl: 'printing-templates',
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
                Context: { datatype: 'string', control: 'text', label: () => trx.instant('PrintingTemplate_Context') },
                Usage: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('Template_Usage'),
                    choices: ['FromSearchAndDetails', 'FromDetails', 'FromReport', 'Standalone'],
                    format: (c: number | string) => {
                        return !!c ? 'Template_Usage_' + c : '';
                    }
                },
                Collection: { datatype: 'string', control: 'text', label: () => trx.instant('Template_Collection') },
                DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Template_DefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                PrintingDefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Template_ReportDefinitionId')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                PrintingDefinition: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Template_ReportDefinitionId'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SupportsPrimaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('PrintingTemplate_Supports') + ws.primaryPostfix },
                SupportsSecondaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('PrintingTemplate_Supports') + ws.secondaryPostfix },
                SupportsTernaryLanguage: { datatype: 'bit', control: 'check', label: () => trx.instant('PrintingTemplate_Supports') + ws.ternaryPostfix },
                DownloadName: { datatype: 'string', control: 'text', label: () => trx.instant('PrintingTemplate_DownloadName') },
                Body: { datatype: 'string', control: 'text', label: () => trx.instant('Template_Body') },
                IsDeployed: { datatype: 'bit', control: 'check', label: () => trx.instant('Template_IsDeployed') },
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),
                ShowInMainMenu: { datatype: 'bit', control: 'check', label: () => trx.instant('Definition_ShowInMainMenu') },
                CreatedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('CreatedAt'), granularity: TimeGranularity.minutes },
                CreatedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('CreatedBy'), foreignKeyName: 'CreatedById' },
                ModifiedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
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
