// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionState, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor, visibilityPropDescriptor, lookupDefinitionIdPropDescriptor, DefinitionCardinality, cardinalityPropDescriptor, statePropDescriptor, lookupDefinitionPropDescriptor } from './base/definition-common';
import { DefinitionVisibility as Visibility } from './base/definition-common';
import { ResourceDefinitionReportDefinition, ResourceDefinitionReportDefinitionForSave } from './resource-definition-report-definition';
import { TimeGranularity } from './base/metadata-types';

export interface ResourceDefinitionForSave<TReportDefinition = ResourceDefinitionReportDefinitionForSave> extends EntityForSave {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;

    ResourceDefinitionType?: string;

    // Common with Agent
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
    Decimal3Label?: string;
    Decimal3Label2?: string;
    Decimal3Label3?: string;
    Decimal3Visibility?: Visibility;
    Decimal4Label?: string;
    Decimal4Label2?: string;
    Decimal4Label3?: string;
    Decimal4Visibility?: Visibility;
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

    PreprocessScript?: string;
    ValidateScript?: string;

    // Resources Only

    IdentifierLabel?: string;
    IdentifierLabel2?: string;
    IdentifierLabel3?: string;
    IdentifierVisibility?: Visibility;

    VatRateVisibility?: Visibility;
    DefaultVatRate?: number;

    ReorderLevelVisibility?: Visibility;
    EconomicOrderQuantityVisibility?: Visibility;
    UnitCardinality?: DefinitionCardinality;
    DefaultUnitId?: number;
    UnitMassVisibility?: Visibility;
    DefaultUnitMassUnitId?: number;
    MonetaryValueVisibility?: Visibility;

    Agent1Label?: string;
    Agent1Label2?: string;
    Agent1Label3?: string;
    Agent1Visibility?: Visibility;
    Agent1DefinitionId?: number;

    Agent2Label?: string;
    Agent2Label2?: string;
    Agent2Label3?: string;
    Agent2Visibility?: Visibility;
    Agent2DefinitionId?: number;

    Resource1Label?: string;
    Resource1Label2?: string;
    Resource1Label3?: string;
    Resource1Visibility?: Visibility;
    Resource1DefinitionId?: number;

    Resource2Label?: string;
    Resource2Label2?: string;
    Resource2Label3?: string;
    Resource2Visibility?: Visibility;
    Resource2DefinitionId?: number;

    HasAttachments?: boolean;
    AttachmentsCategoryDefinitionId?: number;

    MainMenuIcon?: string;
    MainMenuSection?: string;
    MainMenuSortKey?: number;

    ReportDefinitions?: TReportDefinition[];
}

export interface ResourceDefinition extends ResourceDefinitionForSave<ResourceDefinitionReportDefinition> {
    State?: DefinitionState;
    SavedById?: number | string;
    SavedAt?: string;
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
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                TitleSingular: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.primaryPostfix },
                TitleSingular2: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.secondaryPostfix },
                TitleSingular3: { datatype: 'string', control: 'text', label: () => trx.instant('TitleSingular') + ws.ternaryPostfix },
                TitlePlural: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.primaryPostfix },
                TitlePlural2: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.secondaryPostfix },
                TitlePlural3: { datatype: 'string', control: 'text', label: () => trx.instant('TitlePlural') + ws.ternaryPostfix },

                ResourceDefinitionType: {
                    datatype: 'string',
                    control: 'choice',
                    label: () => trx.instant('ResourceDefinition_ResourceDefinitionType'),
                    choices: [
                        'PropertyPlantAndEquipment',
                        'InvestmentProperty',
                        'IntangibleAssetsOtherThanGoodwill',
                        'OtherFinancialAssets',
                        'BiologicalAssets',
                        'InventoriesTotal',
                        'TradeAndOtherReceivables',
                        'CashAndCashEquivalents',
                        'TradeAndOtherPayables',
                        'Provisions',
                        'OtherFinancialLiabilities',
                        'Miscellaneous',
                    ],
                    format: (c: string) => !!c ? trx.instant('RD_Type_' + c) : ''
                },

                // Common with Agent

                CurrencyVisibility: visibilityPropDescriptor('Entity_Currency', trx),
                CenterVisibility: visibilityPropDescriptor('Entity_Center', trx),
                ImageVisibility: visibilityPropDescriptor('Image', trx),
                DescriptionVisibility: visibilityPropDescriptor('Description', trx),
                LocationVisibility: visibilityPropDescriptor('Entity_Location', trx),

                FromDateLabel: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.primaryPostfix },
                FromDateLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.secondaryPostfix },
                FromDateLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_FromDate') }) + ws.ternaryPostfix },
                FromDateVisibility: visibilityPropDescriptor('Entity_FromDate', trx),
                ToDateLabel: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.primaryPostfix },
                ToDateLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.secondaryPostfix },
                ToDateLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ToDate') }) + ws.ternaryPostfix },
                ToDateVisibility: visibilityPropDescriptor('Entity_ToDate', trx),
                Date1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date1') }) + ws.primaryPostfix },
                Date1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date1') }) + ws.secondaryPostfix },
                Date1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date1') }) + ws.ternaryPostfix },
                Date1Visibility: visibilityPropDescriptor('Entity_Date1', trx),
                Date2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date2') }) + ws.primaryPostfix },
                Date2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date2') }) + ws.secondaryPostfix },
                Date2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date2') }) + ws.ternaryPostfix },
                Date2Visibility: visibilityPropDescriptor('Entity_Date2', trx),
                Date3Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date3') }) + ws.primaryPostfix },
                Date3Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date3') }) + ws.secondaryPostfix },
                Date3Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date3') }) + ws.ternaryPostfix },
                Date3Visibility: visibilityPropDescriptor('Entity_Date3', trx),
                Date4Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date4') }) + ws.primaryPostfix },
                Date4Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date4') }) + ws.secondaryPostfix },
                Date4Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Date4') }) + ws.ternaryPostfix },
                Date4Visibility: visibilityPropDescriptor('Entity_Date4', trx),
                Decimal1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.primaryPostfix },
                Decimal1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.secondaryPostfix },
                Decimal1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal1') }) + ws.ternaryPostfix },
                Decimal1Visibility: visibilityPropDescriptor('Entity_Decimal1', trx),
                Decimal2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.primaryPostfix },
                Decimal2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.secondaryPostfix },
                Decimal2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal2') }) + ws.ternaryPostfix },
                Decimal2Visibility: visibilityPropDescriptor('Entity_Decimal2', trx),
                Decimal3Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal3') }) + ws.primaryPostfix },
                Decimal3Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal3') }) + ws.secondaryPostfix },
                Decimal3Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal3') }) + ws.ternaryPostfix },
                Decimal3Visibility: visibilityPropDescriptor('Entity_Decimal3', trx),
                Decimal4Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal4') }) + ws.primaryPostfix },
                Decimal4Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal4') }) + ws.secondaryPostfix },
                Decimal4Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Decimal4') }) + ws.ternaryPostfix },
                Decimal4Visibility: visibilityPropDescriptor('Entity_Decimal4', trx),
                Int1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.primaryPostfix },
                Int1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.secondaryPostfix },
                Int1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int1') }) + ws.ternaryPostfix },
                Int1Visibility: visibilityPropDescriptor('Entity_Int1', trx),
                Int2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.primaryPostfix },
                Int2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.secondaryPostfix },
                Int2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Int2') }) + ws.ternaryPostfix },
                Int2Visibility: visibilityPropDescriptor('Entity_Int2', trx),
                Lookup1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.primaryPostfix },
                Lookup1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.secondaryPostfix },
                Lookup1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup1') }) + ws.ternaryPostfix },
                Lookup1Visibility: visibilityPropDescriptor('Entity_Lookup1', trx),
                Lookup1DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup1', trx),
                Lookup1Definition: lookupDefinitionPropDescriptor('Entity_Lookup1', 'Lookup1DefinitionId', trx),
                Lookup2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.primaryPostfix },
                Lookup2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.secondaryPostfix },
                Lookup2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup2') }) + ws.ternaryPostfix },
                Lookup2Visibility: visibilityPropDescriptor('Entity_Lookup2', trx),
                Lookup2DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup2', trx),
                Lookup2Definition: lookupDefinitionPropDescriptor('Entity_Lookup2', 'Lookup2DefinitionId', trx),
                Lookup3Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.primaryPostfix },
                Lookup3Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.secondaryPostfix },
                Lookup3Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup3') }) + ws.ternaryPostfix },
                Lookup3Visibility: visibilityPropDescriptor('Entity_Lookup3', trx),
                Lookup3DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup3', trx),
                Lookup3Definition: lookupDefinitionPropDescriptor('Entity_Lookup3', 'Lookup3DefinitionId', trx),
                Lookup4Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.primaryPostfix },
                Lookup4Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.secondaryPostfix },
                Lookup4Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup4') }) + ws.ternaryPostfix },
                Lookup4Visibility: visibilityPropDescriptor('Entity_Lookup4', trx),
                Lookup4DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup4', trx),
                Lookup4Definition: lookupDefinitionPropDescriptor('Entity_Lookup4', 'Lookup4DefinitionId', trx),
                Text1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.primaryPostfix },
                Text1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.secondaryPostfix },
                Text1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.ternaryPostfix },
                Text1Visibility: visibilityPropDescriptor('Entity_Text1', trx),
                Text2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.primaryPostfix },
                Text2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.secondaryPostfix },
                Text2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.ternaryPostfix },
                Text2Visibility: visibilityPropDescriptor('Entity_Text2', trx),

                PreprocessScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_PreprocessScript') },
                ValidateScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_ValidateScript') },

                IdentifierLabel: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Identifier') }) + ws.primaryPostfix },
                IdentifierLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Identifier') }) + ws.secondaryPostfix },
                IdentifierLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Identifier') }) + ws.ternaryPostfix },
                IdentifierVisibility: visibilityPropDescriptor('Entity_Identifier', trx),

                // Resource Only

                VatRateVisibility: visibilityPropDescriptor('Resource_VatRate', trx),
                DefaultVatRate: { datatype: 'numeric', control: 'percent', label: () => `${trx.instant('Field0Default', { 0: trx.instant('Resource_VatRate') })}`, minDecimalPlaces: 2, maxDecimalPlaces: 4, noSeparator: false },

                ReorderLevelVisibility: visibilityPropDescriptor('Resource_ReorderLevel', trx),
                EconomicOrderQuantityVisibility: visibilityPropDescriptor('Resource_EconomicOrderQuantity', trx),
                UnitCardinality: cardinalityPropDescriptor('ResourceDefinition_UnitCardinality', trx),
                DefaultUnitId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Field0Default', { 0: trx.instant('Resource_Unit') })} (${trx.instant('Id')})`, maxDecimalPlaces: 0, minDecimalPlaces: 0, noSeparator: true },
                DefaultUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Field0Default', { 0: trx.instant('Resource_Unit') }), foreignKeyName: 'DefaultUnitId' },
                UnitMassVisibility: visibilityPropDescriptor('Resource_UnitMass', trx),
                DefaultUnitMassUnitId: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('Field0Default', { 0: trx.instant('Resource_UnitMassUnit') })} (${trx.instant('Id')})`, maxDecimalPlaces: 0, minDecimalPlaces: 0, noSeparator: true },
                DefaultUnitMassUnit: { datatype: 'entity', control: 'Unit', label: () => trx.instant('Field0Default', { 0: trx.instant('Resource_UnitMassUnit') }), foreignKeyName: 'DefaultUnitMassUnitId' },
                MonetaryValueVisibility: visibilityPropDescriptor('Resource_MonetaryValue', trx),

                Agent1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent1') }) + ws.primaryPostfix },
                Agent1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent1') }) + ws.secondaryPostfix },
                Agent1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent1') }) + ws.ternaryPostfix },
                Agent1Visibility: visibilityPropDescriptor('Resource_Agent1', trx),
                Agent1DefinitionId: {
                    datatype: 'numeric',
                    control: 'choice',
                    label: () => trx.instant('Field0Definition', { 0: trx.instant('Resource_Agent1') }),
                    choices: Object.keys(ws.definitions.Agents).map(stringDefId => +stringDefId),
                    format: (defId: number) => ws.getMultilingualValueImmediate(ws.definitions.Agents[defId], 'TitlePlural')
                },
                Agent2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent2') }) + ws.primaryPostfix },
                Agent2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent2') }) + ws.secondaryPostfix },
                Agent2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Agent2') }) + ws.ternaryPostfix },
                Agent2Visibility: visibilityPropDescriptor('Resource_Agent2', trx),
                Agent2DefinitionId: {
                    datatype: 'numeric',
                    control: 'choice',
                    label: () => trx.instant('Field0Definition', { 0: trx.instant('Resource_Agent2') }),
                    choices: Object.keys(ws.definitions.Agents).map(stringDefId => +stringDefId),
                    format: (defId: number) => ws.getMultilingualValueImmediate(ws.definitions.Agents[defId], 'TitlePlural')
                },

                Resource1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource1') }) + ws.primaryPostfix },
                Resource1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource1') }) + ws.secondaryPostfix },
                Resource1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource1') }) + ws.ternaryPostfix },
                Resource1Visibility: visibilityPropDescriptor('Entity_Resource1', trx),
                Resource1DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Field0Definition', { 0: trx.instant('Entity_Resource1') })} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource1Definition: { datatype: 'entity', control: 'ResourceDefinition', label: () => trx.instant('Field0Definition', { 0: trx.instant('Entity_Resource1') }), foreignKeyName: 'Resource1DefinitionId' },

                Resource2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource2') }) + ws.primaryPostfix },
                Resource2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource2') }) + ws.secondaryPostfix },
                Resource2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Resource2') }) + ws.ternaryPostfix },
                Resource2Visibility: visibilityPropDescriptor('Entity_Resource2', trx),
                Resource2DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Field0Definition', { 0: trx.instant('Entity_Resource2') })} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Resource2Definition: { datatype: 'entity', control: 'ResourceDefinition', label: () => trx.instant('Field0Definition', { 0: trx.instant('Entity_Resource2') }), foreignKeyName: 'Resource2DefinitionId' },

                HasAttachments: { datatype: 'bit', control: 'check', label: () => trx.instant('Definition_HasAttachments') },
                AttachmentsCategoryDefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Definition_AttachmentsCategoryDefinition')})} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AttachmentsCategoryDefinition: { datatype: 'entity', label: () => trx.instant('Definition_AttachmentsCategoryDefinition'), control: 'LookupDefinition', foreignKeyName: 'AttachmentsCategoryDefinitionId' },


                State: statePropDescriptor(trx),
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),

                // IsActive & Audit info
                SavedById: { datatype: 'numeric', control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0, noSeparator: true },
                SavedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'SavedById' },
                SavedAt: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
            }
        };

        // Remove multi-lingual properties if the tenant doesn't define the language
        const multiLangProps = ['TitleSingular', 'TitlePlural', 'IdentifierLabel',
            'FromDateLabel', 'ToDateLabel', 'Decimal1Label', 'Decimal2Label', 'Decimal3Label', 'Decimal4Label',
            'Int1Label', 'Int2Label', 'Lookup1Label', 'Lookup2Label', 'Lookup3Label', 'Lookup4Label',
            'Text1Label', 'Text2Label', 'Agent1Label', 'Agent2Label', 'Resource1Label', 'Resource2Label'];

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
