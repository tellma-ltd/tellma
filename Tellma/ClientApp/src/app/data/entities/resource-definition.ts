// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor, ChoicePropDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionState, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor, visibilityPropDescriptor, lookupDefinitionIdPropDescriptor, DefinitionCardinality, cardinalityPropDescriptor, statePropDescriptor } from './base/definition-common';
import { DefinitionVisibility as Visibility } from './base/definition-common';

export interface ResourceDefinitionForSave extends EntityForSave {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;

    // Common with Relation
    CurrencyVisibility?: Visibility;
    CenterVisibility?: Visibility;
    ImageVisibility?: Visibility;
    DescriptionVisibility?: Visibility;
    LocationVisibility?: Visibility;

    FromDateLabel?: string;
    FromDateLabel2?: string;
    FromDateLabel3?: string;
    FromDateVisibility?: Visibility;
    ToDateLabel?: string;
    ToDateLabel2?: string;
    ToDateLabel3?: string;
    ToDateVisibility?: Visibility;

    Decimal1Label?: string;
    Decimal1Label2?: string;
    Decimal1Label3?: string;
    Decimal1Visibility?: Visibility;
    Decimal2Label?: string;
    Decimal2Label2?: string;
    Decimal2Label3?: string;
    Decimal2Visibility?: Visibility;
    Int1Label?: string;
    Int1Label2?: string;
    Int1Label3?: string;
    Int1Visibility?: Visibility;
    Int2Label?: string;
    Int2Label2?: string;
    Int2Label3?: string;
    Int2Visibility?: Visibility;
    Lookup1Label?: string;
    Lookup1Label2?: string;
    Lookup1Label3?: string;
    Lookup1Visibility?: Visibility;
    Lookup1DefinitionId?: number;
    Lookup2Label?: string;
    Lookup2Label2?: string;
    Lookup2Label3?: string;
    Lookup2Visibility?: Visibility;
    Lookup2DefinitionId?: number;
    Lookup3Label?: string;
    Lookup3Label2?: string;
    Lookup3Label3?: string;
    Lookup3Visibility?: Visibility;
    Lookup3DefinitionId?: number;
    Lookup4Label?: string;
    Lookup4Label2?: string;
    Lookup4Label3?: string;
    Lookup4Visibility?: Visibility;
    Lookup4DefinitionId?: number;
    // Lookup5Label?: string;
    // Lookup5Label2?: string;
    // Lookup5Label3?: string;
    // Lookup5Visibility?: Visibility;
    // Lookup5DefinitionId?: number;
    Text1Label?: string;
    Text1Label2?: string;
    Text1Label3?: string;
    Text1Visibility?: Visibility;
    Text2Label?: string;
    Text2Label2?: string;
    Text2Label3?: string;
    Text2Visibility?: Visibility;

    Script?: string;

    // Resources Only

    IdentifierLabel?: string;
    IdentifierLabel2?: string;
    IdentifierLabel3?: string;
    IdentifierVisibility?: Visibility;

    ReorderLevelVisibility?: Visibility;
    EconomicOrderQuantityVisibility?: Visibility;
    UnitCardinality?: DefinitionCardinality;
    DefaultUnitId?: number;
    UnitMassVisibility?: Visibility;
    DefaultUnitMassUnitId?: number;
    MonetaryValueVisibility?: Visibility;

    // Main Menu

    MainMenuIcon?: string;
    MainMenuSection?: string;
    MainMenuSortKey?: number;
}

export interface ResourceDefinition extends ResourceDefinitionForSave {
    State?: DefinitionState;
    SavedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'TitleSingular' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_ResourceDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
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
            collection: 'ResourceDefinition',
            titleSingular: () => trx.instant('ResourceDefinition'),
            titlePlural: () => trx.instant('ResourceDefinitions'),
            select: _select,
            apiEndpoint: 'resource-definitions',
            masterScreenUrl: 'resource-definitions',
            orderby: () => ws.isSecondaryLanguage ? [_select[1], _select[0]] :
                ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
            inactiveFilter: null, // TODO
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
            properties: {
                Id: { control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { control: 'text', label: () => trx.instant('Code') },
                TitleSingular: { control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },

                // Common with Relation

                CurrencyVisibility: visibilityPropDescriptor('Entity_Currency', trx),
                CenterVisibility: visibilityPropDescriptor('Entity_Center', trx),
                ImageVisibility: visibilityPropDescriptor('Image', trx),
                DescriptionVisibility: visibilityPropDescriptor('Description', trx),
                LocationVisibility: visibilityPropDescriptor('Entity_Location', trx),

                FromDateLabel: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.primaryPostfix },
                FromDateLabel2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.secondaryPostfix },
                FromDateLabel3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.ternaryPostfix },
                FromDateVisibility: visibilityPropDescriptor('Entity_FromDate', trx),
                ToDateLabel: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.primaryPostfix },
                ToDateLabel2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.secondaryPostfix },
                ToDateLabel3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.ternaryPostfix },
                ToDateVisibility: visibilityPropDescriptor('Entity_ToDate', trx),
                Decimal1Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.primaryPostfix },
                Decimal1Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.secondaryPostfix },
                Decimal1Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.ternaryPostfix },
                Decimal1Visibility: visibilityPropDescriptor('Entity_Decimal1', trx),
                Decimal2Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.primaryPostfix },
                Decimal2Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.secondaryPostfix },
                Decimal2Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.ternaryPostfix },
                Decimal2Visibility: visibilityPropDescriptor('Entity_Decimal2', trx),
                Int1Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.primaryPostfix },
                Int1Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.secondaryPostfix },
                Int1Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.ternaryPostfix },
                Int1Visibility: visibilityPropDescriptor('Entity_Int1', trx),
                Int2Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.primaryPostfix },
                Int2Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.secondaryPostfix },
                Int2Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.ternaryPostfix },
                Int2Visibility: visibilityPropDescriptor('Entity_Int2', trx),
                Lookup1Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.primaryPostfix },
                Lookup1Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.secondaryPostfix },
                Lookup1Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.ternaryPostfix },
                Lookup1Visibility: visibilityPropDescriptor('Entity_Lookup1', trx),
                Lookup1DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup1', ws, trx),
                Lookup2Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.primaryPostfix },
                Lookup2Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.secondaryPostfix },
                Lookup2Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.ternaryPostfix },
                Lookup2Visibility: visibilityPropDescriptor('Entity_Lookup2', trx),
                Lookup2DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup2', ws, trx),
                Lookup3Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.primaryPostfix },
                Lookup3Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.secondaryPostfix },
                Lookup3Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.ternaryPostfix },
                Lookup3Visibility: visibilityPropDescriptor('Entity_Lookup3', trx),
                Lookup3DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup3', ws, trx),
                Lookup4Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.primaryPostfix },
                Lookup4Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.secondaryPostfix },
                Lookup4Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.ternaryPostfix },
                Lookup4Visibility: visibilityPropDescriptor('Entity_Lookup4', trx),
                Lookup4DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup4', ws, trx),
                Text1Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.primaryPostfix },
                Text1Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.secondaryPostfix },
                Text1Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.ternaryPostfix },
                Text1Visibility: visibilityPropDescriptor('Entity_Text1', trx),
                Text2Label: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.primaryPostfix },
                Text2Label2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.secondaryPostfix },
                Text2Label3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.ternaryPostfix },
                Text2Visibility: visibilityPropDescriptor('Entity_Text2', trx),
                Script: { control: 'text', label: () => trx.instant('Definition_Script') },

                // Resource Only

                IdentifierLabel: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Resource_Identifier') }) + ws.primaryPostfix },
                IdentifierLabel2: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Resource_Identifier') }) + ws.secondaryPostfix },
                IdentifierLabel3: { control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Resource_Identifier') }) + ws.ternaryPostfix },
                IdentifierVisibility: visibilityPropDescriptor('Resource_Identifier', trx),

                ReorderLevelVisibility: visibilityPropDescriptor('Resource_ReorderLevel', trx),
                EconomicOrderQuantityVisibility: visibilityPropDescriptor('Resource_EconomicOrderQuantity', trx),
                UnitCardinality: cardinalityPropDescriptor('ResourceDefinition_UnitCardinality', trx),
                DefaultUnitId: { control: 'number', label: () => `${trx.instant('Field0Default', { 0: trx.instant('Resource_Unit') })} (${trx.instant('Id')})`, maxDecimalPlaces: 0, minDecimalPlaces: 0 },
                DefaultUnit: { control: 'navigation', label: () => trx.instant('Field0Default', { 0: trx.instant('Resource_Unit') }), type: 'Unit', foreignKeyName: 'DefaultUnitId' },
                UnitMassVisibility: visibilityPropDescriptor('Resource_UnitMass', trx),
                DefaultUnitMassUnitId: { control: 'number', label: () => `${trx.instant('Field0Default', { 0: trx.instant('Resource_UnitMassUnit') })} (${trx.instant('Id')})`, maxDecimalPlaces: 0, minDecimalPlaces: 0 },
                DefaultUnitMassUnit: { control: 'navigation', label: () => trx.instant('Field0Default', { 0: trx.instant('Resource_UnitMassUnit') }), type: 'Unit', foreignKeyName: 'DefaultUnitMassUnitId' },
                MonetaryValueVisibility: visibilityPropDescriptor('Resource_MonetaryValue', trx),

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
        const multiLangProps = ['TitleSingular', 'TitlePlural', 'IdentifierLabel',
            'FromDateLabel', 'ToDateLabel', 'Decimal1Label', 'Decimal2Label',
            'Int1Label', 'Int2Label', 'Lookup1Label', 'Lookup2Label', 'Lookup3Label', 'Lookup4Label',
            'Text1Label', 'Text2Label'];

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
