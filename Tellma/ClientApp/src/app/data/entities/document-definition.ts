// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionState, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor, visibilityPropDescriptor, DefinitionCardinality, lookupDefinitionIdPropDescriptor, cardinalityPropDescriptor, statePropDescriptor } from './base/definition-common';
import { DefinitionVisibility as Visibility } from './base/definition-common';
import { DocumentDefinitionLineDefinitionForSave, DocumentDefinitionLineDefinition } from './document-definition-line-definition';
import { DocumentDefinitionMarkupTemplateForSave, DocumentDefinitionMarkupTemplate } from './document-definition-markup-template';

export type DefinitionDocumentType = 0 | 1 | 2 | 3;

export interface DocumentDefinitionForSave<TLineDefinition = DocumentDefinitionLineDefinitionForSave,
    TMarkupTemplate = DocumentDefinitionMarkupTemplateForSave> extends EntityForSave {
    Code?: string;
    IsOriginalDocument?: boolean;
    DocumentType?: DefinitionDocumentType;
    Description?: string;
    Description2?: string;
    Description3?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;

    Prefix?: string;
    CodeWidth?: number;
    MemoVisibility?: Visibility;
    ClearanceVisibility?: Visibility;

    // Main Menu

    MainMenuIcon?: string;
    MainMenuSection?: string;
    MainMenuSortKey?: number;

    LineDefinitions?: TLineDefinition[];
    MarkupTemplates?: TMarkupTemplate[];
}

export interface DocumentDefinition extends DocumentDefinitionForSave<DocumentDefinitionLineDefinition, DocumentDefinitionMarkupTemplate> {
    State?: DefinitionState;
    SavedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'TitleSingular' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_DocumentDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;

        // clear the cache
        _cache = null;
    }

    if (!_cache) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {
            collection: 'DocumentDefinition',
            titleSingular: () => trx.instant('DocumentDefinition'),
            titlePlural: () => trx.instant('DocumentDefinitions'),
            select: _select,
            apiEndpoint: 'document-definitions',
            masterScreenUrl: 'document-definitions',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] :
                ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { control: 'text', label: () => trx.instant('Code') },
                IsOriginalDocument: { control: 'boolean', label: () => trx.instant('DocumentDefinition_IsOriginalDocument') },
                DocumentType: {
                    control: 'choice',
                    label: () => trx.instant('DocumentDefinition_DocumentType'),
                    choices: [0, 1, 2, 3],
                    format: (type: number) => trx.instant('DocumentDefinition_DocumentType_' + type)
                },
                Description: { control: 'text', label: () => trx.instant('Description') + ws.primaryPostfix },
                Description2: { control: 'text', label: () => trx.instant('Description') + ws.secondaryPostfix },
                Description3: { control: 'text', label: () => trx.instant('Description') + ws.ternaryPostfix },
                TitleSingular: { control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },

                Prefix: { control: 'text', label: () => trx.instant('DocumentDefinition_Prefix') },
                CodeWidth: { control: 'number', label: () => trx.instant('DocumentDefinition_CodeWidth'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                MemoVisibility: visibilityPropDescriptor('Memo', trx),
                ClearanceVisibility: visibilityPropDescriptor('Document_Clearance', trx),

                State: statePropDescriptor(trx),
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),

                // IsActive & Audit info
                SavedById: { control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SavedBy: { control: 'navigation', label: () => trx.instant('ModifiedBy'), type: 'User', foreignKeyName: 'SavedById' }
            }
        };

        // Remove multi-lingual properties if the tenant doesn't define the language
        const multiLangProps = ['TitleSingular', 'TitlePlural', 'Description'];

        for (const prop of multiLangProps) {
            if (!ws.settings.SecondaryLanguageId) {
                delete entityDesc.properties[prop + '2'];
            }
            if (!ws.settings.TernaryLanguageId) {
                delete entityDesc.properties[prop + '3'];
            }
        }

        _cache = entityDesc;
    }

    return _cache;
}
