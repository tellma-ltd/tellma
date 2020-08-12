// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { LineDefinitionEntryForSave, LineDefinitionEntry } from './line-definition-entry';
import { LineDefinitionColumnForSave, LineDefinitionColumn } from './line-definition-column';
import { LineDefinitionStateReasonForSave, LineDefinitionStateReason } from './line-definition-state-reason';
import { LineDefinitionGenerateParameterForSave, LineDefinitionGenerateParameter } from './line-definition-state-generate-parameter';
import { WorkflowForSave, Workflow } from './workflow';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export interface LineDefinitionForSave<
        TEntry = LineDefinitionEntryForSave,
        TColumn = LineDefinitionColumnForSave,
        TStateReason = LineDefinitionStateReasonForSave,
        TGenerateParameter = LineDefinitionGenerateParameterForSave,
        TWorkflow = WorkflowForSave
    > extends EntityForSave {
    Code?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;
    AllowSelectiveSigning?: boolean;
    ViewDefaultsToForm?: boolean;
    GenerateScript?: string;
    GenerateLabel?: string;
    GenerateLabel2?: string;
    GenerateLabel3?: string;
    Script?: string;
    Entries?: TEntry[];
    Columns?: TColumn[];
    StateReasons?: TStateReason[];
    GenerateParameters?: TGenerateParameter[];
    Workflows?: TWorkflow[];
}

export interface LineDefinition extends LineDefinitionForSave<
    LineDefinitionEntry,
    LineDefinitionColumn,
    LineDefinitionStateReason,
    LineDefinitionGenerateParameter,
    Workflow
> {
    SavedById?: number;
}

const _select = ['', '2', '3'].map(pf => 'TitlePlural' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_LineDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'LineDefinition',
            titleSingular: () => trx.instant('LineDefinition'),
            titlePlural: () => trx.instant('LineDefinitions'),
            select: _select,
            apiEndpoint: 'line-definitions',
            masterScreenUrl: 'line-definitions',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null,
            format: (item: LineDefinition) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { control: 'text', label: () => trx.instant('Code') },
                Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                TitleSingular: { control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },
                AllowSelectiveSigning: { control: 'boolean', label: () => trx.instant('LineDefinition_AllowSelectiveSigning') },
                ViewDefaultsToForm: { control: 'boolean', label: () => trx.instant('LineDefinition_ViewDefaultsToForm') },
                GenerateScript: { control: 'text', label: () => trx.instant('LineDefinition_GenerateScript') },
                GenerateLabel: { control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.primaryPostfix },
                GenerateLabel2: { control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.secondaryPostfix },
                GenerateLabel3: { control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.ternaryPostfix },
                Script: { control: 'text', label: () => trx.instant('LineDefinition_Script') },
                SavedById: { control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SavedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'SavedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Description2;
            delete entityDesc.properties.TitleSingular2;
            delete entityDesc.properties.TitlePlural2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Description3;
            delete entityDesc.properties.TitleSingular3;
            delete entityDesc.properties.TitlePlural3;
        }

        _cache = entityDesc;
    }

    return _cache;
}
