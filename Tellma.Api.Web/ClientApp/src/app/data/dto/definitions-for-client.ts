import { ReportOrderDirection, ReportType, ChartType } from '../entities/report-definition';
import { PositiveLineState } from '../entities/line';
import { PrintingUsage } from '../entities/printing-template';
import { DefinitionVisibility as Visibility, DefinitionCardinality, DefinitionState } from '../entities/base/definition-common';
import { InheritsFrom } from '../entities/line-definition-column';
import { ExistingItemHandling, LineType } from '../entities/line-definition';
import { Collection, Control, PropVisualDescriptor } from '../entities/base/metadata';
import { Cardinality, NotificationUsage } from '../entities/email-template';

// tslint:disable:variable-name
export interface DefinitionsForClient {
    Documents: { [definitionId: number]: DocumentDefinitionForClient };
    Lines: { [definitionId: number]: LineDefinitionForClient };
    Agents: { [definitionId: number]: AgentDefinitionForClient };
    Resources: { [definitionId: number]: ResourceDefinitionForClient };
    Lookups: { [definitionId: number]: LookupDefinitionForClient };
    Reports: { [definitionId: number]: ReportDefinitionForClient };
    Dashboards: { [definitionId: number]: DashboardDefinitionForClient };
    PrintingTemplates: { [definitionId: number]: PrintingTemplateForClient };
    EmailTemplates: { [definitionId: number]: EmailTemplateForClient };
    MessageTemplates: { [definitionId: number]: MessageTemplateForClient };

    ManualJournalVouchersDefinitionId: number;
    ManualLinesDefinitionId: number;
    ReferenceSourceDefinitionIds: number[];
}

export interface DefinitionForClient {
    MainMenuSection?: string;
    MainMenuIcon?: string;
    MainMenuSortKey?: number;
}

export interface MasterDetailsDefinitionForClient extends DefinitionForClient {
    Code?: string;
    TitleSingular?: string;
    TitleSingular2?: string;
    TitleSingular3?: string;
    TitlePlural?: string;
    TitlePlural2?: string;
    TitlePlural3?: string;
    State?: DefinitionState;
}

export interface ReportDefinitionForClient extends DefinitionForClient {
    Id: number;
    Title: string;
    Title2?: string;
    Title3?: string;
    Description?: string;
    Description2?: string;
    Description3?: string;
    Type: ReportType; // summary or details
    Chart?: ChartType;
    DefaultsToChart: boolean; // ?
    ChartOptions?: string;
    Collection: Collection;
    DefinitionId?: number;
    Select: ReportDefinitionSelectForClient[];
    Parameters?: ReportDefinitionParameterForClient[];
    Filter?: string;
    Having?: string;
    OrderBy?: string;
    Rows: ReportDefinitionDimensionForClient[];
    Columns: ReportDefinitionDimensionForClient[];
    Measures: ReportDefinitionMeasureForClient[];
    Top?: number;
    ShowColumnsTotal: boolean;
    ColumnsTotalLabel?: string;
    ColumnsTotalLabel2?: string;
    ColumnsTotalLabel3?: string;
    ShowRowsTotal: boolean;
    RowsTotalLabel?: string;
    RowsTotalLabel2?: string;
    RowsTotalLabel3?: string;
    IsCustomDrilldown: boolean;
    ShowInMainMenu: boolean;
}

export interface ReportDefinitionParameterForClient {
    Key: string; // e.g. 'FromDate'
    Label?: string;
    Label2?: string;
    Label3?: string;
    Visibility?: Visibility;
    DefaultExpression?: string;
    Control?: Control;
    ControlOptions?: string;
}

export interface ReportDefinitionSelectForClient {
    Expression: string;
    Localize: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    Control?: Control;
    ControlOptions?: string;
}

export interface ReportDefinitionMeasureForClient {
    Expression: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    Control?: Control;
    ControlOptions?: string;
    DangerWhen?: string;
    WarningWhen?: string;
    SuccessWhen?: string;
}

export interface ReportDefinitionDimensionForClient {
    KeyExpression: string;
    DisplayExpression?: string;
    Localize: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
    AutoExpandLevel: number;
    ShowAsTree?: boolean;
    Control?: Control;
    ControlOptions?: string; // JSON
    Attributes?: ReportDefinitionDimensionAttributeForClient[];
}

export interface ReportDefinitionDimensionAttributeForClient {
    Expression?: string;
    Localize: boolean;
    Label?: string;
    Label2?: string;
    Label3?: string;
    OrderDirection?: ReportOrderDirection;
}

export interface DashboardDefinitionForClient extends DefinitionForClient {
    Id: number;
    Title: string;
    Title2?: string;
    Title3?: string;
    AutoRefreshPeriodInMinutes: number;
    Widgets: DashboardDefinitionWidgetForClient[];
    ShowInMainMenu: boolean;
}

export interface DashboardDefinitionWidgetForClient {
    ReportDefinitionId: number;
    OffsetX: number;
    OffsetY: number;
    Width: number;
    Height: number;
    Title?: string;
    Title2?: string;
    Title3?: string;
    AutoRefreshPeriodInMinutes: number;

    // Client only
    changeY?: number;
    changeW?: number;
    changeH?: number;
}

export interface DocumentDefinitionForClient extends MasterDetailsDefinitionForClient {
    IsOriginalDocument?: boolean;
    AttachmentVisibility?: Visibility;
    HasBookkeeping?: boolean;
    Prefix?: string;
    CodeWidth?: number;

    // Posting Date
    PostingDateVisibility?: Visibility;
    PostingDateIsCommonVisibility?: boolean;
    PostingDateLabel?: string;
    PostingDateLabel2?: string;
    PostingDateLabel3?: string;
    PostingDateRequiredState?: PositiveLineState | 5;
    PostingDateReadOnlyState?: PositiveLineState | 5;

    // Center
    CenterVisibility?: Visibility;
    CenterIsCommonVisibility?: boolean;
    CenterLabel?: string;
    CenterLabel2?: string;
    CenterLabel3?: string;
    CenterFilter?: string;
    CenterRequiredState?: PositiveLineState | 5;
    CenterReadOnlyState?: PositiveLineState | 5;

    // Memo
    MemoVisibility?: Visibility;
    MemoIsCommonVisibility?: boolean;
    MemoLabel?: string;
    MemoLabel2?: string;
    MemoLabel3?: string;
    MemoRequiredState?: PositiveLineState | 5;
    MemoReadOnlyState?: PositiveLineState | 5;

    // Currency
    CurrencyVisibility?: boolean;
    CurrencyRequiredState?: PositiveLineState | 5;
    CurrencyReadOnlyState?: PositiveLineState | 5;
    CurrencyLabel?: string;
    CurrencyLabel2?: string;
    CurrencyLabel3?: string;
    CurrencyFilter?: string;

    // Agent
    AgentVisibility?: boolean;
    AgentRequiredState?: PositiveLineState | 5;
    AgentReadOnlyState?: PositiveLineState | 5;
    AgentDefinitionIds?: number[];
    AgentLabel?: string;
    AgentLabel2?: string;
    AgentLabel3?: string;
    AgentFilter?: string;

    // Resource
    ResourceVisibility?: boolean;
    ResourceRequiredState?: PositiveLineState | 5;
    ResourceReadOnlyState?: PositiveLineState | 5;
    ResourceDefinitionIds?: number[];
    ResourceLabel?: string;
    ResourceLabel2?: string;
    ResourceLabel3?: string;
    ResourceFilter?: string;

    // NotedAgent
    NotedAgentVisibility?: boolean;
    NotedAgentRequiredState?: PositiveLineState | 5;
    NotedAgentReadOnlyState?: PositiveLineState | 5;
    NotedAgentDefinitionIds?: number[];
    NotedAgentLabel?: string;
    NotedAgentLabel2?: string;
    NotedAgentLabel3?: string;
    NotedAgentFilter?: string;

    // NotedResource
    NotedResourceVisibility?: boolean;
    NotedResourceRequiredState?: PositiveLineState | 5;
    NotedResourceReadOnlyState?: PositiveLineState | 5;
    NotedResourceDefinitionIds?: number[];
    NotedResourceLabel?: string;
    NotedResourceLabel2?: string;
    NotedResourceLabel3?: string;
    NotedResourceFilter?: string;

    // Quantity
    QuantityVisibility?: boolean;
    QuantityRequiredState?: PositiveLineState | 5;
    QuantityReadOnlyState?: PositiveLineState | 5;
    QuantityLabel?: string;
    QuantityLabel2?: string;
    QuantityLabel3?: string;

    // Unit
    UnitVisibility?: boolean;
    UnitRequiredState?: PositiveLineState | 5;
    UnitReadOnlyState?: PositiveLineState | 5;
    UnitLabel?: string;
    UnitLabel2?: string;
    UnitLabel3?: string;
    UnitFilter?: string;

    // Time1
    Time1Visibility?: boolean;
    Time1RequiredState?: PositiveLineState | 5;
    Time1ReadOnlyState?: PositiveLineState | 5;
    Time1Label?: string;
    Time1Label2?: string;
    Time1Label3?: string;

    // Duration
    DurationVisibility?: boolean;
    DurationRequiredState?: PositiveLineState | 5;
    DurationReadOnlyState?: PositiveLineState | 5;
    DurationLabel?: string;
    DurationLabel2?: string;
    DurationLabel3?: string;

    // DurationUnit
    DurationUnitVisibility?: boolean;
    DurationUnitRequiredState?: PositiveLineState | 5;
    DurationUnitReadOnlyState?: PositiveLineState | 5;
    DurationUnitLabel?: string;
    DurationUnitLabel2?: string;
    DurationUnitLabel3?: string;
    DurationUnitFilter?: string;

    // Time2
    Time2Visibility?: boolean;
    Time2RequiredState?: PositiveLineState | 5;
    Time2ReadOnlyState?: PositiveLineState | 5;
    Time2Label?: string;
    Time2Label2?: string;
    Time2Label3?: string;

    // ExternalReference
    ExternalReferenceVisibility?: boolean;
    ExternalReferenceRequiredState?: PositiveLineState | 5;
    ExternalReferenceReadOnlyState?: PositiveLineState | 5;
    ExternalReferenceLabel?: string;
    ExternalReferenceLabel2?: string;
    ExternalReferenceLabel3?: string;

    // ReferenceSource
    ReferenceSourceVisibility?: boolean;
    ReferenceSourceRequiredState?: PositiveLineState | 5;
    ReferenceSourceReadOnlyState?: PositiveLineState | 5;
    ReferenceSourceLabel?: string;
    ReferenceSourceLabel2?: string;
    ReferenceSourceLabel3?: string;
    ReferenceSourceFilter?: string;

    // InternalReference
    InternalReferenceVisibility?: boolean;
    InternalReferenceRequiredState?: PositiveLineState | 5;
    InternalReferenceReadOnlyState?: PositiveLineState | 5;
    InternalReferenceLabel?: string;
    InternalReferenceLabel2?: string;
    InternalReferenceLabel3?: string;

    // Clearance
    ClearanceVisibility?: Visibility;

    CanReachState1?: boolean;
    CanReachState2?: boolean;
    CanReachState3?: boolean;
    HasWorkflow?: boolean;
    LineDefinitions?: DocumentDefinitionLineDefinitionForClient[];
}

export interface PrintingTemplateForClient extends DefinitionForClient {
    PrintingTemplateId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    SupportsPrimaryLanguage?: boolean;
    SupportsSecondaryLanguage?: boolean;
    SupportsTernaryLanguage?: boolean;
    Usage?: PrintingUsage;
    Collection?: Collection;
    DefinitionId?: number;
    Parameters?: TemplateParameterForClient[];
}

export interface EmailTemplateForClient extends DefinitionForClient {
    EmailTemplateId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    Cardinality?: Cardinality;
    Usage?: NotificationUsage;
    Collection?: Collection;
    DefinitionId?: number;
    Parameters?: TemplateParameterForClient[];
}

export interface MessageTemplateForClient extends DefinitionForClient {
    MessageTemplateId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
    Cardinality?: Cardinality;
    Usage?: NotificationUsage;
    Collection?: Collection;
    DefinitionId?: number;
    Parameters?: TemplateParameterForClient[];
}

export interface TemplateParameterForClient {
    Key?: string;
    Label?: string;
    Label2?: string;
    Label3?: string;
    IsRequired?: boolean;
    Control?: Control;
    ControlOptions?: string;

    desc?: PropVisualDescriptor; // For caching purposes
}

export interface DocumentDefinitionLineDefinitionForClient {
    LineDefinitionId?: number;
    IsVisibleByDefault?: boolean;
}

export interface LineDefinitionForClient {
    Code: string;
    LineType: LineType;
    TitleSingular: string;
    TitleSingular2: string;
    TitleSingular3: string;
    TitlePlural: string;
    TitlePlural2: string;
    TitlePlural3: string;
    AllowSelectiveSigning: boolean;
    ViewDefaultsToForm: boolean;

    BarcodeColumnIndex: number;
    BarcodeProperty: string;
    BarcodeExistingItemHandling: ExistingItemHandling;
    BarcodeBeepsEnabled: boolean;

    GenerateScript: boolean;
    GenerateLabel: string;
    GenerateLabel2: string;
    GenerateLabel3: string;
    Entries: LineDefinitionEntryForClient[];
    Columns: LineDefinitionColumnForClient[];
    StateReasons: LineDefinitionStateReasonForClient[];
    GenerateParameters: LineDefinitionGenerateParameterForClient[];
}

export interface LineDefinitionEntryForClient {
    Direction: 1 | -1;
    ParentAccountTypeId?: number;
    EntryTypeId?: number;
    EntryTypeParentId?: number; // Comes from the Account Types
    AgentDefinitionIds: number[];
    ResourceDefinitionIds: number[];
    NotedAgentDefinitionIds: number[];
    NotedResourceDefinitionIds: number[];
}

export interface LineDefinitionColumnForClient {
    ColumnName: EntryColumnName;
    EntryIndex: number;
    Label: string;
    Label2: string;
    Label3: string;
    Filter?: string;
    RequiredState?: PositiveLineState | 5;
    ReadOnlyState?: PositiveLineState | 5;
    InheritsFromHeader?: InheritsFrom;
}

export interface LineDefinitionStateReasonForClient {
    Id: number;
    State: number;
    Name: string;
    Name2: string;
    Name3: string;
    IsActive: boolean;
}

export interface LineDefinitionGenerateParameterForClient {
    Key: string;
    Label: string;
    Label2: string;
    Label3: string;
    Control: Control;
    ControlOptions?: string;
    Visibility: Visibility;

    desc?: PropVisualDescriptor; // For caching purposes
}

export const entryColumnNames: EntryColumnName[] = ['Memo', 'PostingDate', 'Boolean1', 'Decimal1', 'Text1', 'AccountId', 'CurrencyId',
    'AgentId', 'ResourceId', 'NotedAgentId', 'NotedResourceId', 'CenterId', 'EntryTypeId',
    'MonetaryValue', 'Quantity', 'UnitId', 'Time1', 'Duration', 'DurationUnitId', 'Time2', 'Value',
    'ExternalReference', 'ReferenceSourceId', 'InternalReference', 'NotedAgentName', 'NotedAmount', 'NotedDate'];

export type EntryColumnName = 'Memo' | 'PostingDate' | 'Boolean1' | 'Decimal1' | 'Text1' | 'AccountId' | 'CurrencyId' |
    'AgentId' | 'ResourceId' | 'NotedAgentId' | 'NotedResourceId' | 'CenterId' | 'EntryTypeId' |
    'MonetaryValue' | 'Quantity' | 'UnitId' | 'Time1' | 'Duration' | 'DurationUnitId' | 'Time2' | 'Value' |
    'ExternalReference' | 'ReferenceSourceId' | 'InternalReference' | 'NotedAgentName' | 'NotedAmount' | 'NotedDate';

export interface ResourceDefinitionForClient extends MasterDetailsDefinitionForClient {

    ResourceDefinitionType: string;

    CurrencyVisibility: Visibility;
    CenterVisibility: Visibility;
    ImageVisibility: Visibility;
    DescriptionVisibility: Visibility;
    LocationVisibility: Visibility;

    FromDateLabel: string;
    FromDateLabel2: string;
    FromDateLabel3: string;
    FromDateVisibility: Visibility;

    ToDateLabel: string;
    ToDateLabel2: string;
    ToDateLabel3: string;
    ToDateVisibility: Visibility;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;

    // Decimal 3
    Decimal3Label: string;
    Decimal3Label2: string;
    Decimal3Label3: string;
    Decimal3Visibility: Visibility;

    // Decimal 4
    Decimal4Label: string;
    Decimal4Label2: string;
    Decimal4Label3: string;
    Decimal4Visibility: Visibility;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefinitionId: number;

    //// Lookup 5
    // Lookup5Label: string;
    // Lookup5Label2: string;
    // Lookup5Label3: string;
    // Lookup5Visibility: Visibility;
    // Lookup5DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;

    // Resource Only

    IdentifierLabel: string;
    IdentifierLabel2: string;
    IdentifierLabel3: string;
    IdentifierVisibility: Visibility;

    VatRateVisibility: Visibility;
    DefaultVatRate?: number;

    ReorderLevelVisibility: Visibility;
    EconomicOrderQuantityVisibility: Visibility;
    UnitCardinality: DefinitionCardinality;
    DefaultUnitId?: number;
    UnitMassVisibility?: Visibility;
    DefaultUnitMassUnitId?: number;
    MonetaryValueVisibility: Visibility;

    // Agent1
    Agent1Label: string;
    Agent1Label2: string;
    Agent1Label3: string;
    Agent1Visibility?: Visibility;
    Agent1DefinitionId?: number;

    // Agent2
    Agent2Label: string;
    Agent2Label2: string;
    Agent2Label3: string;
    Agent2Visibility?: Visibility;
    Agent2DefinitionId?: number;

    // Resource1
    Resource1Label: string;
    Resource1Label2: string;
    Resource1Label3: string;
    Resource1Visibility: Visibility;
    Resource1DefinitionId: number;

    // Resource2
    Resource2Label: string;
    Resource2Label2: string;
    Resource2Label3: string;
    Resource2Visibility: Visibility;
    Resource2DefinitionId: number;

    ReportDefinitions?: DefinitionReportDefinitionForClient[];
}

export interface LookupDefinitionForClient extends MasterDetailsDefinitionForClient {
    ReportDefinitions?: DefinitionReportDefinitionForClient[];
}

export interface AgentDefinitionForClient extends MasterDetailsDefinitionForClient {

    CurrencyVisibility: Visibility;
    CenterVisibility: Visibility;
    ImageVisibility: Visibility;
    DescriptionVisibility: Visibility;
    LocationVisibility: Visibility;

    FromDateLabel: string;
    FromDateLabel2: string;
    FromDateLabel3: string;
    FromDateVisibility: Visibility;

    ToDateLabel: string;
    ToDateLabel2: string;
    ToDateLabel3: string;
    ToDateVisibility: Visibility;

    DateOfBirthVisibility?: Visibility;
    ContactEmailVisibility?: Visibility;
    ContactMobileVisibility?: Visibility;
    ContactAddressVisibility?: Visibility;

    // Date 1
    Date1Label?: string;
    Date1Label2?: string;
    Date1Label3?: string;
    Date1Visibility?: Visibility;

    // Date 2
    Date2Label?: string;
    Date2Label2?: string;
    Date2Label3?: string;
    Date2Visibility?: Visibility;

    // Date 3
    Date3Label?: string;
    Date3Label2?: string;
    Date3Label3?: string;
    Date3Visibility?: Visibility;

    // Date 4
    Date4Label?: string;
    Date4Label2?: string;
    Date4Label3?: string;
    Date4Visibility?: Visibility;

    // Decimal 1
    Decimal1Label: string;
    Decimal1Label2: string;
    Decimal1Label3: string;
    Decimal1Visibility: Visibility;

    // Decimal 2
    Decimal2Label: string;
    Decimal2Label2: string;
    Decimal2Label3: string;
    Decimal2Visibility: Visibility;

    // Int 1
    Int1Label: string;
    Int1Label2: string;
    Int1Label3: string;
    Int1Visibility: Visibility;

    // Int 2
    Int2Label: string;
    Int2Label2: string;
    Int2Label3: string;
    Int2Visibility: Visibility;

    // Lookup 1
    Lookup1Label: string;
    Lookup1Label2: string;
    Lookup1Label3: string;
    Lookup1Visibility: Visibility;
    Lookup1DefinitionId: number;

    // Lookup 2
    Lookup2Label: string;
    Lookup2Label2: string;
    Lookup2Label3: string;
    Lookup2Visibility: Visibility;
    Lookup2DefinitionId: number;

    // Lookup 3
    Lookup3Label: string;
    Lookup3Label2: string;
    Lookup3Label3: string;
    Lookup3Visibility: Visibility;
    Lookup3DefinitionId: number;

    // Lookup 4
    Lookup4Label: string;
    Lookup4Label2: string;
    Lookup4Label3: string;
    Lookup4Visibility: Visibility;
    Lookup4DefinitionId: number;

    // Lookup 5
    Lookup5Label: string;
    Lookup5Label2: string;
    Lookup5Label3: string;
    Lookup5Visibility: Visibility;
    Lookup5DefinitionId: number;

    // Lookup 6
    Lookup6Label: string;
    Lookup6Label2: string;
    Lookup6Label3: string;
    Lookup6Visibility: Visibility;
    Lookup6DefinitionId: number;

    // Lookup 7
    Lookup7Label: string;
    Lookup7Label2: string;
    Lookup7Label3: string;
    Lookup7Visibility: Visibility;
    Lookup7DefinitionId: number;

    // Lookup 8
    Lookup8Label: string;
    Lookup8Label2: string;
    Lookup8Label3: string;
    Lookup8Visibility: Visibility;
    Lookup8DefinitionId: number;

    // Text 1
    Text1Label: string;
    Text1Label2: string;
    Text1Label3: string;
    Text1Visibility: Visibility;

    // Text 2
    Text2Label: string;
    Text2Label2: string;
    Text2Label3: string;
    Text2Visibility: Visibility;

    // Text 3
    Text3Label: string;
    Text3Label2: string;
    Text3Label3: string;
    Text3Visibility: Visibility;

    // Text 4
    Text4Label: string;
    Text4Label2: string;
    Text4Label3: string;
    Text4Visibility: Visibility;

    // Agent Only

    // ExternalReference
    ExternalReferenceLabel: string;
    ExternalReferenceLabel2: string;
    ExternalReferenceLabel3: string;
    ExternalReferenceVisibility: Visibility;

    // Agent 1
    Agent1Label: string;
    Agent1Label2: string;
    Agent1Label3: string;
    Agent1Visibility: Visibility;
    Agent1DefinitionId: number;

    // Agent 2
    Agent2Label: string;
    Agent2Label2: string;
    Agent2Label3: string;
    Agent2Visibility: Visibility;
    Agent2DefinitionId: number;

    TaxIdentificationNumberVisibility?: Visibility;
    BankAccountNumberVisibility?: Visibility;
    UserCardinality?: DefinitionCardinality;
    HasAttachments?: boolean;
    AttachmentsCategoryDefinitionId?: number;

    ReportDefinitions?: DefinitionReportDefinitionForClient[];
}

export interface DefinitionReportDefinitionForClient {
    ReportDefinitionId?: number;
    Name?: string;
    Name2?: string;
    Name3?: string;
}
