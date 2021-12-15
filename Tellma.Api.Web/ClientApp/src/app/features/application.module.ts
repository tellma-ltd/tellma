import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EllipsisModule } from 'ngx-ellipsis';
import { SharedModule } from '../shared/shared.module';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { UnitsMasterComponent } from './units/units-master.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { UnitsDetailsComponent } from './units/units-details.component';
import { SaveInProgressGuard } from '~/app/data/save-in-progress.guard';
import { UnsavedChangesGuard } from '~/app//data/unsaved-changes.guard';
import { AgentsMasterComponent } from './agents/agents-master.component';
import { AgentsDetailsComponent } from './agents/agents-details.component';
import { RolesMasterComponent } from './roles/roles-master.component';
import { RolesDetailsComponent } from './roles/roles-details.component';
import { UsersDetailsComponent } from './users/users-details.component';
import { UsersMasterComponent } from './users/users-master.component';
import { GeneralSettingsComponent } from './general-settings/general-settings.component';
import { TenantResolverGuard } from '../data/tenant-resolver.guard';
import { AuthGuard } from '../data/auth.guard';
import { LookupsMasterComponent } from './lookups/lookups-master.component';
import { LookupsDetailsComponent } from './lookups/lookups-details.component';
import { CurrenciesMasterComponent } from './currencies/currencies-master.component';
import { CurrenciesDetailsComponent } from './currencies/currencies-details.component';
import { ResourcesMasterComponent } from './resources/resources-master.component';
import { ResourcesDetailsComponent } from './resources/resources-details.component';
import { UnitsPickerComponent } from './units/units-picker.component';
import { LookupsPickerComponent } from './lookups/lookups-picker.component';
import { AccountClassificationsMasterComponent } from './account-classifications/account-classifications-master.component';
import { AccountClassificationsDetailsComponent } from './account-classifications/account-classifications-details.component';
import { AccountClassificationsPickerComponent } from './account-classifications/account-classifications-picker.component';
import { AccountTypesMasterComponent } from './account-types/account-types-master.component';
import { AccountTypesDetailsComponent } from './account-types/account-types-details.component';
import { AccountTypesPickerComponent } from './account-types/account-types-picker.component';
import { AccountsMasterComponent } from './accounts/accounts-master.component';
import { AccountsDetailsComponent } from './accounts/accounts-details.component';
import { AccountsPickerComponent } from './accounts/accounts-picker.component';
import { AgentsPickerComponent } from './agents/agents-picker.component';
import { ResourcesPickerComponent } from './resources/resources-picker.component';
import { ReportComponent } from './report/report.component';
import { ReportResultsComponent } from './report-results/report-results.component';
import { ReportDefinitionsMasterComponent } from './report-definitions/report-definitions-master.component';
import { ReportDefinitionsDetailsComponent } from './report-definitions/report-definitions-details.component';
import { CentersMasterComponent } from './centers/centers-master.component';
import { CentersDetailsComponent } from './centers/centers-details.component';
import { CentersPickerComponent } from './centers/centers-picker.component';
import { UsersPickerComponent } from './users/users-picker.component';
import { RolesPickerComponent } from './roles/roles-picker.component';
import { CurrenciesPickerComponent } from './currencies/currencies-picker.component';
import { EntryTypesMasterComponent } from './entry-types/entry-types-master.component';
import { EntryTypesDetailsComponent } from './entry-types/entry-types-details.component';
import { EntryTypesPickerComponent } from './entry-types/entry-types-picker.component';
import { DocumentsMasterComponent } from './documents/documents-master.component';
import { DocumentsDetailsComponent } from './documents/documents-details.component';
import { DetailsEntriesComponent } from './details-entries/details-entries.component';
import { ExchangeRatesMasterComponent } from './exchange-rates/exchange-rates-master.component';
import { ExchangeRatesDetailsComponent } from './exchange-rates/exchange-rates-details.component';
import { InboxComponent } from './inbox/inbox.component';
import { OutboxComponent } from './outbox/outbox.component';
import { IfrsConceptsMasterComponent } from './ifrs-concepts/ifrs-concepts-master.component';
import { IfrsConceptsDetailsComponent } from './ifrs-concepts/ifrs-concepts-details.component';
import { IfrsConceptsPickerComponent } from './ifrs-concepts/ifrs-concepts-picker.component';
import { PrintingTemplatesMasterComponent } from './printing-templates/printing-templates-master.component';
import { PrintingTemplatesDetailsComponent } from './printing-templates/printing-templates-details.component';
import { DocumentsPickerComponent } from './documents/documents-picker.component';
import { AgentDefinitionsDetailsComponent } from './agent-definitions/agent-definitions-details.component';
import { AgentDefinitionsMasterComponent } from './agent-definitions/agent-definitions-master.component';
import { AgentDefinitionsPickerComponent } from './agent-definitions/agent-definitions-picker.component';
import { ResourceDefinitionsDetailsComponent } from './resource-definitions/resource-definitions-details.component';
import { ResourceDefinitionsMasterComponent } from './resource-definitions/resource-definitions-master.component';
import { ResourceDefinitionsPickerComponent } from './resource-definitions/resource-definitions-picker.component';
import { LookupDefinitionsMasterComponent } from './lookup-definitions/lookup-definitions-master.component';
import { LookupDefinitionsDetailsComponent } from './lookup-definitions/lookup-definitions-details.component';
import { LookupDefinitionsPickerComponent } from './lookup-definitions/lookup-definitions-picker.component';
import { StatementComponent } from './statement/statement.component';
import { AccountStatementComponent } from './statement/account-statement.component';
import { AgentStatementComponent } from './statement/agent-statement.component';
import { LineDefinitionsDetailsComponent } from './line-definitions/line-definitions-details.component';
import { LineDefinitionsMasterComponent } from './line-definitions/line-definitions-master.component';
import { LineDefinitionsPickerComponent } from './line-definitions/line-definitions-picker.component';
import { DocumentDefinitionsMasterComponent } from './document-definitions/document-definitions-master.component';
import { DocumentDefinitionsDetailsComponent } from './document-definitions/document-definitions-details.component';
import { DocumentDefinitionsPickerComponent } from './document-definitions/document-definitions-picker.component';
import { PrintingTemplatesPickerComponent } from './printing-templates/printing-templates-picker.component';
import { ReconciliationComponent } from './reconciliation/reconciliation.component';
import { EmailsMasterComponent } from './emails/emails-master.component';
import { EmailsDetailsComponent } from './emails/emails-details.component';
import { SmsMessagesDetailsComponent } from './sms-messages/sms-messages-details.component';
import { SmsMessagesMasterComponent } from './sms-messages/sms-messages-master.component';
import { ReportDefinitionsPickerComponent } from './report-definitions/report-definitions-picker.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import {
  faAirFreshener,
  faAnchor,
  faAngleDown,
  faAngleUp,
  faAngry,
  faArchive,
  faArrowsAlt,
  faBalanceScale,
  faBarcode,
  faBell,
  faBlind,
  faBolt,
  faBook,
  faBox,
  faBoxes,
  faBroadcastTower,
  faBus,
  faCalendarAlt,
  faCampground,
  faCar,
  faCarrot,
  faCarSide,
  faCartArrowDown,
  faCashRegister,
  faCertificate,
  faChalkboardTeacher,
  faChartArea,
  faChartBar,
  faChevronDown,
  faChevronUp,
  faCity,
  faClipboard,
  faClipboardList,
  faClone,
  faCode,
  faCodeBranch,
  faCoins,
  faCompress,
  faCopy,
  faCopyright,
  faDollyFlatbed,
  faDoorClosed,
  faDraftingCompass,
  faDragon,
  faDrawPolygon,
  faEdit,
  faEllipsisH,
  faEllipsisV,
  faEnvelope,
  faEquals,
  faEuroSign,
  faExchangeAlt,
  faExclamation,
  faExclamationCircle,
  faExpand,
  faFax,
  faFemale,
  faFile,
  faFileAlt,
  faFileArchive,
  faFileAudio,
  faFileCode,
  faFileContract,
  faFileExcel,
  faFileExport,
  faFileImage,
  faFileImport,
  faFileInvoiceDollar,
  faFilePdf,
  faFilePowerpoint,
  faFileVideo,
  faFileWord,
  faFistRaised,
  faFlag,
  faFolder,
  faFolderMinus,
  faFolderPlus,
  faFont,
  faFunnelDollar,
  faGasPump,
  faGavel,
  faGifts,
  faGlobe,
  faGraduationCap,
  faGrinHearts,
  faHammer,
  faHandHoldingUsd,
  faHandPointRight,
  faHandRock,
  faHandsHelping,
  faHistory,
  faHollyBerry,
  faHourglassHalf,
  faIdBadge,
  faImage,
  faInbox,
  faIndent,
  faIndustry,
  faKissWinkHeart,
  faKiwiBird,
  faLandmark,
  faLanguage,
  faLaptop,
  faLaptopCode,
  faLightbulb,
  faList,
  faListUl,
  faLock,
  faLockOpen,
  faMagic,
  faMale,
  faMap,
  faMapMarkerAlt,
  faMicrochip,
  faMinus,
  faMoneyBillWave,
  faMoneyCheck,
  faMoneyCheckAlt,
  faMoon,
  faNetworkWired,
  faNewspaper,
  faObjectGroup,
  faPaintRoller,
  faPalette,
  faPallet,
  faPaperclip,
  faParachuteBox,
  faPencilRuler,
  faPercent,
  faPercentage,
  faPills,
  faPlane,
  faPortrait,
  faPowerOff,
  faPrint,
  faProjectDiagram,
  faPuzzlePiece,
  faRecycle,
  faScroll,
  faSearchDollar,
  faSeedling,
  faShapes,
  faShare,
  faShareSquare,
  faShieldAlt,
  faShip,
  faShoppingCart,
  faSign,
  faSignOutAlt,
  faSitemap,
  faSms,
  faSpa,
  faStar,
  faStoreSlash,
  faStream,
  faSuitcaseRolling,
  faTags,
  faTasks,
  faThumbsDown,
  faThumbsUp,
  faTint,
  faTintSlash,
  faTractor,
  faTrademark,
  faTree,
  faTrophy,
  faTruck,
  faUmbrellaBeach,
  faUndoAlt,
  faUniversity,
  faUser,
  faUserCheck,
  faUserCircle,
  faUserClock,
  faUserCog,
  faUserFriends,
  faUserGraduate,
  faUserMd,
  faUserMinus,
  faUserPlus,
  faUsersCog,
  faUserShield,
  faUserTag,
  faUserTie,
  faUtensils,
  faVenusMars,
  faVial,
  faWarehouse
} from '@fortawesome/free-solid-svg-icons';
import { FinancialSettingsComponent } from './financial-settings/financial-settings.component';
import { ControlOptionsComponent } from './control-options/control-options.component';
import { EditorComponent } from './editor/editor.component';
import { DrilldownComponent } from './drilldown/drilldown.component';
import { DashboardDefinitionsMasterComponent } from './dashboard-definitions/dashboard-definitions-master.component';
import { DashboardDefinitionsDetailsComponent } from './dashboard-definitions/dashboard-definitions-details.component';
import { DashboardDefinitionsPickerComponent } from './dashboard-definitions/dashboard-definitions-picker.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { PrintComponent } from './print/print.component';
import { NotificationTemplatesDetailsComponent } from './notification-templates/notification-templates-details.component';
import { NotificationTemplatesMasterComponent } from './notification-templates/notification-templates-master.component';
import { NotificationTemplatesPickerComponent } from './notification-templates/notification-templates-picker.component';
import { NotificationCommandsDetailsComponent } from './notification-commands/notification-commands-details.component';
import { NotificationCommandsMasterComponent } from './notification-commands/notification-commands-master.component';
import { NotificationCommandsPickerComponent } from './notification-commands/notification-commands-picker.component';
import { EmailButtonComponent } from './email-button/email-button.component';

const routes: Routes = [
  {
    path: ':tenantId',
    component: ApplicationShellComponent,
    canActivate: [TenantResolverGuard],
    canActivateChild: [AuthGuard],
    children: [
      // Units
      {
        path: 'units',
        component: UnitsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'units/:id',
        component: UnitsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Roles
      {
        path: 'roles',
        component: RolesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'roles/:id',
        component: RolesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Users
      {
        path: 'users',
        component: UsersMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'users/:id',
        component: UsersDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Agents
      {
        path: 'agents',
        component: AgentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:definitionId',
        component: AgentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:definitionId/:id',
        component: AgentsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Lookups
      {
        path: 'lookups',
        component: LookupsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'lookups/:definitionId',
        component: LookupsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'lookups/:definitionId/:id',
        component: LookupsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Currencies
      {
        path: 'currencies',
        component: CurrenciesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'currencies/:id',
        component: CurrenciesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Resources
      {
        path: 'resources',
        component: ResourcesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resources/:definitionId',
        component: ResourcesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resources/:definitionId/:id',
        component: ResourcesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Account Classifications
      {
        path: 'account-classifications',
        component: AccountClassificationsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'account-classifications/:id',
        component: AccountClassificationsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // IFRS Concepts
      {
        path: 'ifrs-concepts',
        component: IfrsConceptsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'ifrs-concepts/:id',
        component: IfrsConceptsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Account Types
      {
        path: 'account-types',
        component: AccountTypesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'account-types/:id',
        component: AccountTypesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Accounts
      {
        path: 'accounts',
        component: AccountsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'accounts/:id',
        component: AccountsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Entry Types
      {
        path: 'entry-types',
        component: EntryTypesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'entry-types/:id',
        component: EntryTypesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Documents
      {
        path: 'documents',
        component: DocumentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'documents/:definitionId',
        component: DocumentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'documents/:definitionId/:id',
        component: DocumentsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },
      // Those redirects help shorten notification links (especially SMS)
      {
        path: 'd/:definitionId',
        redirectTo: 'documents/:definitionId'
      },
      {
        path: 'd/:definitionId/:id',
        redirectTo: 'documents/:definitionId/:id'
      },
      {
        path: 'inbox',
        component: InboxComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'outbox',
        component: OutboxComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Report Definitions
      {
        path: 'report-definitions',
        component: ReportDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'report-definitions/:id',
        component: ReportDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Dashboard Definitions
      {
        path: 'dashboard-definitions',
        component: DashboardDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'dashboard-definitions/:id',
        component: DashboardDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Centers
      {
        path: 'centers',
        component: CentersMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'centers/:id',
        component: CentersDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Exchange Rates
      {
        path: 'exchange-rates',
        component: ExchangeRatesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'exchange-rates/:id',
        component: ExchangeRatesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Details Entries: TODO
      {
        path: 'details-entries',
        component: DetailsEntriesComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'details-entries/:id',
        component: DetailsEntriesComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Notification Commands
      {
        path: 'notification-commands',
        component: NotificationCommandsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'notification-commands/:id',
        component: NotificationCommandsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Notification Templates
      {
        path: 'notification-templates',
        component: NotificationTemplatesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'notification-templates/:id',
        component: NotificationTemplatesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Printing Templates
      {
        path: 'printing-templates',
        component: PrintingTemplatesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'printing-templates/:id',
        component: PrintingTemplatesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Agent Definitions
      {
        path: 'agent-definitions',
        component: AgentDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agent-definitions/:id',
        component: AgentDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Resource Definitions
      {
        path: 'resource-definitions',
        component: ResourceDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resource-definitions/:id',
        component: ResourceDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Lookup Definitions
      {
        path: 'lookup-definitions',
        component: LookupDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'lookup-definitions/:id',
        component: LookupDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Line Definitions
      {
        path: 'line-definitions',
        component: LineDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'line-definitions/:id',
        component: LineDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Document Definitions
      {
        path: 'document-definitions',
        component: DocumentDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'document-definitions/:id',
        component: DocumentDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Report
      {
        path: 'report/:definitionId',
        component: ReportComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Dashboard
      {
        path: 'dashboard/:definitionId',
        component: DashboardComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Drilldown
      {
        path: 'drilldown/:definitionId',
        component: DrilldownComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Account Statement
      {
        path: 'account-statement',
        component: AccountStatementComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agent-statement/:definitionId',
        component: AgentStatementComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Bank Reconciliation
      {
        path: 'reconciliation',
        component: ReconciliationComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Emails
      {
        path: 'emails',
        component: EmailsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'emails/:id',
        component: EmailsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // SMS Messages
      {
        path: 'sms-messages',
        component: SmsMessagesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'sms-messages/:id',
        component: SmsMessagesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // General Settings
      {
        path: 'general-settings',
        component: GeneralSettingsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // General Settings
      {
        path: 'financial-settings',
        component: FinancialSettingsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Standalone Print
      {
        path: 'print/:templateId',
        component: PrintComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Misc
      {
        path: 'main-menu',
        component: MainMenuComponent,
        canDeactivate: [SaveInProgressGuard] // for saving my user
      },
      { path: '', redirectTo: 'main-menu', pathMatch: 'full' },
      { path: '**', component: ApplicationPageNotFoundComponent },
    ]
  },
  {
    // Otherwise it gets stuck in a blank page
    path: '',
    redirectTo: '/root/welcome',
    pathMatch: 'full'
  }
];

@NgModule({
  declarations: [
    ApplicationShellComponent,
    UnitsMasterComponent,
    UnitsDetailsComponent,
    ApplicationPageNotFoundComponent,
    MainMenuComponent,
    AgentsMasterComponent,
    AgentsDetailsComponent,
    RolesMasterComponent,
    RolesDetailsComponent,
    UsersDetailsComponent,
    UsersMasterComponent,
    GeneralSettingsComponent,
    LookupsMasterComponent,
    LookupsDetailsComponent,
    CurrenciesMasterComponent,
    CurrenciesDetailsComponent,
    ResourcesMasterComponent,
    ResourcesDetailsComponent,
    UnitsPickerComponent,
    LookupsPickerComponent,
    AccountClassificationsMasterComponent,
    AccountClassificationsDetailsComponent,
    AccountClassificationsPickerComponent,
    AccountTypesMasterComponent,
    AccountTypesDetailsComponent,
    AccountTypesPickerComponent,
    AccountsMasterComponent,
    AccountsDetailsComponent,
    AccountsPickerComponent,
    AgentsPickerComponent,
    ResourcesPickerComponent,
    ReportComponent,
    ReportResultsComponent,
    ReportDefinitionsMasterComponent,
    ReportDefinitionsDetailsComponent,
    CentersMasterComponent,
    CentersDetailsComponent,
    CentersPickerComponent,
    UsersPickerComponent,
    RolesPickerComponent,
    CurrenciesPickerComponent,
    EntryTypesMasterComponent,
    EntryTypesDetailsComponent,
    EntryTypesPickerComponent,
    DocumentsMasterComponent,
    DocumentsDetailsComponent,
    DetailsEntriesComponent,
    ExchangeRatesMasterComponent,
    ExchangeRatesDetailsComponent,
    InboxComponent,
    OutboxComponent,
    IfrsConceptsMasterComponent,
    IfrsConceptsDetailsComponent,
    IfrsConceptsPickerComponent,
    PrintingTemplatesMasterComponent,
    PrintingTemplatesDetailsComponent,
    DocumentsPickerComponent,
    AgentDefinitionsDetailsComponent,
    AgentDefinitionsMasterComponent,
    AgentDefinitionsPickerComponent,
    ResourceDefinitionsDetailsComponent,
    ResourceDefinitionsMasterComponent,
    ResourceDefinitionsPickerComponent,
    LookupDefinitionsMasterComponent,
    LookupDefinitionsDetailsComponent,
    LookupDefinitionsPickerComponent,
    StatementComponent,
    AccountStatementComponent,
    AgentStatementComponent,
    LineDefinitionsDetailsComponent,
    LineDefinitionsMasterComponent,
    LineDefinitionsPickerComponent,
    DocumentDefinitionsMasterComponent,
    DocumentDefinitionsDetailsComponent,
    DocumentDefinitionsPickerComponent,
    PrintingTemplatesPickerComponent,
    ReconciliationComponent,
    EmailsMasterComponent,
    EmailsDetailsComponent,
    SmsMessagesDetailsComponent,
    SmsMessagesMasterComponent,
    ReportDefinitionsPickerComponent,
    FinancialSettingsComponent,
    ControlOptionsComponent,
    EditorComponent,
    DrilldownComponent,
    DashboardDefinitionsMasterComponent,
    DashboardDefinitionsDetailsComponent,
    DashboardDefinitionsPickerComponent,
    DashboardComponent,
    PrintComponent,
    NotificationTemplatesDetailsComponent,
    NotificationTemplatesMasterComponent,
    NotificationTemplatesPickerComponent,
    NotificationCommandsDetailsComponent,
    NotificationCommandsMasterComponent,
    NotificationCommandsPickerComponent,
    EmailButtonComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes),
    EllipsisModule,
  ],
})
export class ApplicationModule {
  constructor(library: FaIconLibrary) {
    // Icons to be used in the web app
    library.addIcons(
      // Document icons
      //  faThumbsUp, faThumbsDown, faPaperclip, faExclamation, faLockOpen,

      // File icons
      // faFile, faFilePdf, faFileWord, faFileExcel, faFilePowerpoint, faFileAlt, faFileCode,
      // faFileArchive, faFileImage, faFileVideo, faFileAudio, faEllipsisV, faEllipsisH, faArchive,

      // Main menu icons, IMPORTANT: Keep in sync with definition-common.ts
      faAirFreshener,
      faAnchor,
      faAngleDown,
      faAngleUp,
      faAngry,
      faArchive,
      faArrowsAlt,
      faBalanceScale,
      faBarcode,
      faBell,
      faBlind,
      faBolt,
      faBook,
      faBox,
      faBoxes,
      faBroadcastTower,
      faBus,
      faCalendarAlt,
      faCampground,
      faCar,
      faCarrot,
      faCarSide,
      faCartArrowDown,
      faCashRegister,
      faCertificate,
      faChalkboardTeacher,
      faChartArea,
      faChartBar,
      faChevronDown,
      faChevronUp,
      faCity,
      faClipboard,
      faClipboardList,
      faClone,
      faCode,
      faCodeBranch,
      faCoins,
      faCompress,
      faCopy,
      faCopyright,
      faDollyFlatbed,
      faDoorClosed,
      faDraftingCompass,
      faDragon,
      faDrawPolygon,
      faEdit,
      faEllipsisH,
      faEllipsisV,
      faEnvelope,
      faEquals,
      faEuroSign,
      faExchangeAlt,
      faExclamation,
      faExclamationCircle,
      faExpand,
      faFax,
      faFemale,
      faFile,
      faFileAlt,
      faFileArchive,
      faFileAudio,
      faFileCode,
      faFileContract,
      faFileExcel,
      faFileExport,
      faFileImage,
      faFileImport,
      faFileInvoiceDollar,
      faFilePdf,
      faFilePowerpoint,
      faFileVideo,
      faFileWord,
      faFistRaised,
      faFlag,
      faFolder,
      faFolderMinus,
      faFolderPlus,
      faFont,
      faFunnelDollar,
      faGasPump,
      faGavel,
      faGifts,
      faGlobe,
      faGraduationCap,
      faGrinHearts,
      faHammer,
      faHandHoldingUsd,
      faHandPointRight,
      faHandRock,
      faHandsHelping,
      faHistory,
      faHollyBerry,
      faHourglassHalf,
      faIdBadge,
      faImage,
      faInbox,
      faIndent,
      faIndustry,
      faKissWinkHeart,
      faKiwiBird,
      faLandmark,
      faLanguage,
      faLaptop,
      faLaptopCode,
      faLightbulb,
      faList,
      faListUl,
      faLock,
      faLockOpen,
      faMagic,
      faMale,
      faMap,
      faMapMarkerAlt,
      faMicrochip,
      faMinus,
      faMoneyBillWave,
      faMoneyCheck,
      faMoneyCheckAlt,
      faMoon,
      faNetworkWired,
      faNewspaper,
      faObjectGroup,
      faPaintRoller,
      faPalette,
      faPallet,
      faPaperclip,
      faParachuteBox,
      faPencilRuler,
      faPercent,
      faPercentage,
      faPills,
      faPlane,
      faPortrait,
      faPowerOff,
      faPrint,
      faProjectDiagram,
      faPuzzlePiece,
      faRecycle,
      faScroll,
      faSearchDollar,
      faSeedling,
      faShapes,
      faShare,
      faShareSquare,
      faShieldAlt,
      faShip,
      faShoppingCart,
      faSign,
      faSignOutAlt,
      faSitemap,
      faSms,
      faSpa,
      faStar,
      faStoreSlash,
      faStream,
      faSuitcaseRolling,
      faTags,
      faTasks,
      faThumbsDown,
      faThumbsUp,
      faTint,
      faTintSlash,
      faTractor,
      faTrademark,
      faTree,
      faTrophy,
      faTruck,
      faUmbrellaBeach,
      faUndoAlt,
      faUniversity,
      faUser,
      faUserCheck,
      faUserCircle,
      faUserClock,
      faUserCog,
      faUserFriends,
      faUserGraduate,
      faUserMd,
      faUserMinus,
      faUserPlus,
      faUsersCog,
      faUserShield,
      faUserTag,
      faUserTie,
      faUtensils,
      faVenusMars,
      faVial,
      faWarehouse
    );
  }
}
