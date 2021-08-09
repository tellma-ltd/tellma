// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';
import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { DefinitionState, mainMenuSectionPropDescriptor, mainMenuIconPropDescriptor, mainMenuSortKeyPropDescriptor, visibilityPropDescriptor, DefinitionCardinality, lookupDefinitionIdPropDescriptor, cardinalityPropDescriptor, statePropDescriptor, lookupDefinitionPropDescriptor } from './base/definition-common';
import { DefinitionVisibility as Visibility } from './base/definition-common';
import { RelationDefinitionReportDefinition, RelationDefinitionReportDefinitionForSave } from './relation-definition-report-definition';
import { TimeGranularity } from './base/metadata-types';

export interface RelationDefinitionForSave<TReportDefinition = RelationDefinitionReportDefinitionForSave> extends EntityForSave {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;

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
    DateOfBirthVisibility?: Visibility;
    ContactEmailVisibility?: Visibility;
    ContactMobileVisibility?: Visibility;
    ContactAddressVisibility?: Visibility;
    Date1Label?: string;
    Date1Label2?: string;
    Date1Label3?: string;
    Date1Visibility?: Visibility;
    Date2Label?: string;
    Date2Label2?: string;
    Date2Label3?: string;
    Date2Visibility?: Visibility;
    Date3Label?: string;
    Date3Label2?: string;
    Date3Label3?: string;
    Date3Visibility?: Visibility;
    Date4Label?: string;
    Date4Label2?: string;
    Date4Label3?: string;
    Date4Visibility?: Visibility;
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
    Lookup5Label?: string;
    Lookup5Label2?: string;
    Lookup5Label3?: string;
    Lookup5Visibility?: Visibility;
    Lookup5DefinitionId?: number;
    Lookup6Label?: string;
    Lookup6Label2?: string;
    Lookup6Label3?: string;
    Lookup6Visibility?: Visibility;
    Lookup6DefinitionId?: number;
    Lookup7Label?: string;
    Lookup7Label2?: string;
    Lookup7Label3?: string;
    Lookup7Visibility?: Visibility;
    Lookup7DefinitionId?: number;
    Lookup8Label?: string;
    Lookup8Label2?: string;
    Lookup8Label3?: string;
    Lookup8Visibility?: Visibility;
    Lookup8DefinitionId?: number;
    Text1Label?: string;
    Text1Label2?: string;
    Text1Label3?: string;
    Text1Visibility?: Visibility;
    Text2Label?: string;
    Text2Label2?: string;
    Text2Label3?: string;
    Text2Visibility?: Visibility;
    Text3Label?: string;
    Text3Label2?: string;
    Text3Label3?: string;
    Text3Visibility?: Visibility;
    Text4Label?: string;
    Text4Label2?: string;
    Text4Label3?: string;
    Text4Visibility?: Visibility;
    ExternalReferenceLabel?: string;
    ExternalReferenceLabel2?: string;
    ExternalReferenceLabel3?: string;
    ExternalReferenceVisibility?: Visibility;

    PreprocessScript?: string;
    ValidateScript?: string;

    // Relation Definition Only
    Relation1Label?: string;
    Relation1Label2?: string;
    Relation1Label3?: string;
    Relation1Visibility?: Visibility;
    Relation1DefinitionId?: number;

    AgentVisibility?: Visibility;
    TaxIdentificationNumberVisibility?: Visibility;
    BankAccountNumberVisibility?: Visibility;
    UserCardinality?: DefinitionCardinality;
    HasAttachments?: boolean;
    AttachmentsCategoryDefinitionId?: number;

    // Main Menu

    MainMenuIcon?: string;
    MainMenuSection?: string;
    MainMenuSortKey?: number;

    ReportDefinitions?: TReportDefinition[];
}

export interface RelationDefinition extends RelationDefinitionForSave<RelationDefinitionReportDefinition> {
    State?: DefinitionState;
    SavedById?: number | string;
}

const _select = ['', '2', '3'].map(pf => 'TitleSingular' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor = null;

export function metadata_RelationDefinition(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
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
            collection: 'RelationDefinition',
            titleSingular: () => trx.instant('RelationDefinition'),
            titlePlural: () => trx.instant('RelationDefinitions'),
            select: _select,
            apiEndpoint: 'relation-definitions',
            masterScreenUrl: 'relation-definitions',
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

                // Common with Resource

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

                DateOfBirthVisibility: visibilityPropDescriptor('Relation_DateOfBirth', trx),
                ContactEmailVisibility: visibilityPropDescriptor('Entity_ContactEmail', trx),
                ContactMobileVisibility: visibilityPropDescriptor('Entity_ContactMobile', trx),
                ContactAddressVisibility: visibilityPropDescriptor('Entity_ContactAddress', trx),
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
                Lookup5Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup5') }) + ws.primaryPostfix },
                Lookup5Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup5') }) + ws.secondaryPostfix },
                Lookup5Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup5') }) + ws.ternaryPostfix },
                Lookup5Visibility: visibilityPropDescriptor('Entity_Lookup5', trx),
                Lookup5DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup5', trx),
                Lookup5Definition: lookupDefinitionPropDescriptor('Entity_Lookup5', 'Lookup5DefinitionId', trx),
                Lookup6Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup6') }) + ws.primaryPostfix },
                Lookup6Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup6') }) + ws.secondaryPostfix },
                Lookup6Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup6') }) + ws.ternaryPostfix },
                Lookup6Visibility: visibilityPropDescriptor('Entity_Lookup6', trx),
                Lookup6DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup6', trx),
                Lookup6Definition: lookupDefinitionPropDescriptor('Entity_Lookup6', 'Lookup6DefinitionId', trx),
                Lookup7Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup7') }) + ws.primaryPostfix },
                Lookup7Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup7') }) + ws.secondaryPostfix },
                Lookup7Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup7') }) + ws.ternaryPostfix },
                Lookup7Visibility: visibilityPropDescriptor('Entity_Lookup7', trx),
                Lookup7DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup7', trx),
                Lookup7Definition: lookupDefinitionPropDescriptor('Entity_Lookup7', 'Lookup7DefinitionId', trx),
                Lookup8Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup8') }) + ws.primaryPostfix },
                Lookup8Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup8') }) + ws.secondaryPostfix },
                Lookup8Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Lookup8') }) + ws.ternaryPostfix },
                Lookup8Visibility: visibilityPropDescriptor('Entity_Lookup8', trx),
                Lookup8DefinitionId: lookupDefinitionIdPropDescriptor('Entity_Lookup8', trx),
                Lookup8Definition: lookupDefinitionPropDescriptor('Entity_Lookup8', 'Lookup8DefinitionId', trx),
                Text1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.primaryPostfix },
                Text1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.secondaryPostfix },
                Text1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text1') }) + ws.ternaryPostfix },
                Text1Visibility: visibilityPropDescriptor('Entity_Text1', trx),
                Text2Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.primaryPostfix },
                Text2Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.secondaryPostfix },
                Text2Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text2') }) + ws.ternaryPostfix },
                Text2Visibility: visibilityPropDescriptor('Entity_Text2', trx),
                Text3Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text3') }) + ws.primaryPostfix },
                Text3Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text3') }) + ws.secondaryPostfix },
                Text3Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text3') }) + ws.ternaryPostfix },
                Text3Visibility: visibilityPropDescriptor('Entity_Text3', trx),
                Text4Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text4') }) + ws.primaryPostfix },
                Text4Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text4') }) + ws.secondaryPostfix },
                Text4Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Text4') }) + ws.ternaryPostfix },
                Text4Visibility: visibilityPropDescriptor('Entity_Text4', trx),
                ExternalReferenceLabel: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ExternalReference') }) + ws.primaryPostfix },
                ExternalReferenceLabel2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ExternalReference') }) + ws.secondaryPostfix },
                ExternalReferenceLabel3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_ExternalReference') }) + ws.ternaryPostfix },
                ExternalReferenceVisibility: visibilityPropDescriptor('Entity_ExternalReference', trx),

                PreprocessScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_PreprocessScript') },
                ValidateScript: { datatype: 'string', control: 'text', label: () => trx.instant('Definition_ValidateScript') },

                Relation1Label: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Relation1') }) + ws.primaryPostfix },
                Relation1Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Relation1') }) + ws.secondaryPostfix },
                Relation1Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Field0Label', { 0: trx.instant('Entity_Relation1') }) + ws.ternaryPostfix },
                Relation1Visibility: visibilityPropDescriptor('Entity_Relation1', trx),
                Relation1DefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('Field0Definition', { 0: trx.instant('Entity_Relation1') })} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Relation1Definition: { datatype: 'entity', control: 'RelationDefinition', label: () => trx.instant('Field0Definition', { 0: trx.instant('Entity_Relation1') }), foreignKeyName: 'Relation1DefinitionId' },

                AgentVisibility: visibilityPropDescriptor('Relation_Agent', trx),
                TaxIdentificationNumberVisibility: visibilityPropDescriptor('Relation_TaxIdentificationNumber', trx),
                BankAccountNumberVisibility: visibilityPropDescriptor('Relation_BankAccountNumber', trx),
                UserCardinality: cardinalityPropDescriptor('RelationDefinition_UserCardinality', trx),
                HasAttachments: { datatype: 'bit', control: 'check', label: () => trx.instant('Definition_HasAttachments') },
                AttachmentsCategoryDefinitionId: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('RelationDefinition_AttachmentsCategoryDefinition')})} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                AttachmentsCategoryDefinition: { datatype: 'entity', label: () => trx.instant('RelationDefinition_AttachmentsCategoryDefinition'), control: 'LookupDefinition', foreignKeyName: 'AttachmentsCategoryDefinitionId' },

                State: statePropDescriptor(trx),
                MainMenuSection: mainMenuSectionPropDescriptor(trx),
                MainMenuIcon: mainMenuIconPropDescriptor(trx),
                MainMenuSortKey: mainMenuSortKeyPropDescriptor(trx),

                // IsActive & Audit info
                SavedById: { noSeparator: true, datatype: 'numeric', control: 'number', label: () => `${trx.instant('ModifiedBy')} (${trx.instant('Id')})`, minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                SavedBy: { datatype: 'entity', control: 'User', label: () => trx.instant('ModifiedBy'), foreignKeyName: 'SavedById' },
                ValidFrom: { datatype: 'datetimeoffset', control: 'datetime', label: () => trx.instant('ModifiedAt'), granularity: TimeGranularity.minutes },
            }
        };

        // Remove multi-lingual properties if the tenant doesn't define the language
        const multiLangProps = ['TitleSingular', 'TitlePlural',
            'FromDateLabel', 'ToDateLabel',
            'Date1Label', 'Date2Label', 'Date3Label', 'Date4Label',
            'Decimal1Label', 'Decimal2Label',
            'Int1Label', 'Int2Label',
            'Lookup1Label', 'Lookup2Label', 'Lookup3Label', 'Lookup4Label', 'Lookup5Label', 'Lookup6Label', 'Lookup7Label', 'Lookup8Label',
            'Text1Label', 'Text2Label', 'Text3Label', 'Text4Label'];

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
