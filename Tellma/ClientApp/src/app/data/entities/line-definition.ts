// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { LineDefinitionEntryForSave, LineDefinitionEntry } from './line-definition-entry';
import { LineDefinitionColumnForSave, LineDefinitionColumn } from './line-definition-column';
import { LineDefinitionStateReasonForSave, LineDefinitionStateReason } from './line-definition-state-reason';
import { LineDefinitionGenerateParameterForSave, LineDefinitionGenerateParameter } from './line-definition-generate-parameter';
import { WorkflowForSave, Workflow } from './workflow';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityDescriptor } from './base/metadata';
import { SettingsForClient } from '../dto/settings-for-client';

export type ExistingItemHandling = 'AddNewLine'| 'IncrementQuantity'| 'ThrowError'| 'DoNothing';
const existingItemHandlingChoices: ExistingItemHandling[] = ['AddNewLine', 'IncrementQuantity', 'ThrowError', 'DoNothing'];

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

    // New barcode stuff
    BarcodeColumnIndex?: number;
    BarcodeProperty?: string;
    BarcodeExistingItemHandling?: ExistingItemHandling;
    BarcodeBeepsEnabled?: boolean;

    GenerateLabel?: string;
    GenerateLabel2?: string;
    GenerateLabel3?: string;
    GenerateScript?: string;
    PreprocessScript?: string;
    ValidateScript?: string;
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
    Workflow> {
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
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                Description: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { datatype: 'string', control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                TitleSingular: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },
                AllowSelectiveSigning: { datatype: 'bit', control: 'check', label: () => trx.instant('LineDefinition_AllowSelectiveSigning') },
                ViewDefaultsToForm: { datatype: 'bit', control: 'check', label: () => trx.instant('LineDefinition_ViewDefaultsToForm') },

                // New barcode stuff
                BarcodeColumnIndex: { datatype: 'numeric', control: 'number', label: () => trx.instant('LineDefinition_BarcodeColumnIndex'), minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: true },
                BarcodeProperty: { datatype: 'string', control: 'text', label: () => trx.instant('LineDefinition_BarcodeProperty') },
                BarcodeExistingItemHandling: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('LineDefinition_BarcodeExistingItemHandling'),
                    choices: existingItemHandlingChoices,
                    format: (choice: string) => !!choice ? trx.instant('LineDefinition_Handling_' + choice) : ''
                },
                BarcodeBeepsEnabled: { datatype: 'bit', control: 'check', label: () => trx.instant('LineDefinition_BarcodeBeepsEnabled') },

                GenerateLabel: { datatype: 'string', control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.primaryPostfix },
                GenerateLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.secondaryPostfix },
                GenerateLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('LineDefinition_GenerateLabel') + ws.ternaryPostfix },
                GenerateScript: { datatype: 'string', control: 'text', label: () => trx.instant('LineDefinition_GenerateScript') },
                PreprocessScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_PreprocessScript') },
                ValidateScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_ValidateScript') },
                SavedById: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SavedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'SavedById' }
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Description2;
            delete entityDesc.properties.TitleSingular2;
            delete entityDesc.properties.TitlePlural2;
            delete entityDesc.properties.GenerateLabel2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Description3;
            delete entityDesc.properties.TitleSingular3;
            delete entityDesc.properties.TitlePlural3;
            delete entityDesc.properties.GenerateLabel3;
        }

        _cache = entityDesc;
    }

    return _cache;
}
