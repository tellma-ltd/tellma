import { inject, NgModule } from '@angular/core';
import { ActivatedRouteSnapshot, RouterModule, RouterStateSnapshot, Routes } from '@angular/router';
import { EllipsisModule } from 'ngx-ellipsis';
import { SharedModule } from '../shared/shared.module';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { UnitsMasterComponent } from './units/units-master.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { UnitsDetailsComponent } from './units/units-details.component';
import { saveInProgressGuard } from '~/app/data/save-in-progress.guard';
import { unsavedChangesGuard } from '~/app//data/unsaved-changes.guard';
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
import { MessagesDetailsComponent } from './messages/messages-details.component';
import { MessagesMasterComponent } from './messages/messages-master.component';
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
  faHandshake,
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
  faMailBulk,
  faMale,
  faMap,
  faMapMarkerAlt,
  faMicrochip,
  faMinus,
  faMoneyBillWave,
  faMoneyCheck,
  faMoneyCheckAlt,
  faMoon,
  faMountain,
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
  faPersonBooth,
  faPills,
  faPlane,
  faPlaneArrival,
  faPortrait,
  faPowerOff,
  faPrint,
  faProjectDiagram,
  faPuzzlePiece,
  faQrcode,
  faQuestionCircle,
  faReceipt,
  faRecycle,
  faScroll,
  faSearchDollar,
  faSeedling,
  faShapes,
  faShare,
  faShareAlt,
  faShareAltSquare,
  faShareSquare,
  faShieldAlt,
  faShip,
  faShoppingCart,
  faSign,
  faSignOutAlt,
  faSitemap,
  faSms,
  faSnowplow,
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
import { EmailTemplatesDetailsComponent } from './email-templates/email-templates-details.component';
import { EmailTemplatesMasterComponent } from './email-templates/email-templates-master.component';
import { EmailTemplatesPickerComponent } from './email-templates/email-templates-picker.component';
import { EmailCommandsDetailsComponent } from './email-commands/email-commands-details.component';
import { EmailCommandsMasterComponent } from './email-commands/email-commands-master.component';
import { EmailCommandsPickerComponent } from './email-commands/email-commands-picker.component';
import { MessageTemplatesDetailsComponent } from './message-templates/message-templates-details.component';
import { MessageTemplatesMasterComponent } from './message-templates/message-templates-master.component';
import { MessageTemplatesPickerComponent } from './message-templates/message-templates-picker.component';
import { MessageComponent } from './message/message.component';
import { MessageCommandsMasterComponent } from './message-commands/message-commands-master.component';
import { MessageCommandsDetailsComponent } from './message-commands/message-commands-details.component';
import { MessageCommandsPickerComponent } from './message-commands/message-commands-picker.component';
import { MessageButtonComponent } from './message-button/message-button.component';
import { MessagePreviewerComponent } from './message-previewer/message-previewer.component';
import { MessageStandaloneComponent } from './message-standalone/message-standalone.component';
import { EmailComponent } from './email/email.component';
import { EmailPreviewerComponent } from './email-previewer/email-previewer.component';
import { EmailButtonComponent } from './email-button/email-button.component';
import { EmailStandaloneComponent } from './email-standalone/email-standalone.component';
import { LinkyModule } from 'ngx-linky';

const routes: Routes = [
  {
    path: ':tenantId',
    component: ApplicationShellComponent,
    canActivate: [(next: ActivatedRouteSnapshot, state: RouterStateSnapshot) => inject(TenantResolverGuard).canActivate(next, state)],
    canActivateChild: [(_: ActivatedRouteSnapshot, s: RouterStateSnapshot) => inject(AuthGuard).canActivateChild(s)],
    children: [
      // Units
      {
        path: 'units',
        component: UnitsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'units/:id',
        component: UnitsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Roles
      {
        path: 'roles',
        component: RolesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'roles/:id',
        component: RolesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Users
      {
        path: 'users',
        component: UsersMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'users/:id',
        component: UsersDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Agents
      {
        path: 'agents',
        component: AgentsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'agents/:definitionId',
        component: AgentsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'agents/:definitionId/:id',
        component: AgentsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Lookups
      {
        path: 'lookups',
        component: LookupsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'lookups/:definitionId',
        component: LookupsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'lookups/:definitionId/:id',
        component: LookupsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Currencies
      {
        path: 'currencies',
        component: CurrenciesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'currencies/:id',
        component: CurrenciesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Resources
      {
        path: 'resources',
        component: ResourcesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'resources/:definitionId',
        component: ResourcesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'resources/:definitionId/:id',
        component: ResourcesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Account Classifications
      {
        path: 'account-classifications',
        component: AccountClassificationsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'account-classifications/:id',
        component: AccountClassificationsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // IFRS Concepts
      {
        path: 'ifrs-concepts',
        component: IfrsConceptsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'ifrs-concepts/:id',
        component: IfrsConceptsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Account Types
      {
        path: 'account-types',
        component: AccountTypesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'account-types/:id',
        component: AccountTypesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Accounts
      {
        path: 'accounts',
        component: AccountsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'accounts/:id',
        component: AccountsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Entry Types
      {
        path: 'entry-types',
        component: EntryTypesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'entry-types/:id',
        component: EntryTypesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Documents
      {
        path: 'documents',
        component: DocumentsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'documents/:definitionId',
        component: DocumentsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'documents/:definitionId/:id',
        component: DocumentsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
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
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'outbox',
        component: OutboxComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Report Definitions
      {
        path: 'report-definitions',
        component: ReportDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'report-definitions/:id',
        component: ReportDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Dashboard Definitions
      {
        path: 'dashboard-definitions',
        component: DashboardDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'dashboard-definitions/:id',
        component: DashboardDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Centers
      {
        path: 'centers',
        component: CentersMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'centers/:id',
        component: CentersDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Exchange Rates
      {
        path: 'exchange-rates',
        component: ExchangeRatesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'exchange-rates/:id',
        component: ExchangeRatesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Details Entries: TODO
      {
        path: 'details-entries',
        component: DetailsEntriesComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'details-entries/:id',
        component: DetailsEntriesComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Message Commands
      {
        path: 'message-commands',
        component: MessageCommandsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'message-commands/:id',
        component: MessageCommandsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Message Templates
      {
        path: 'message-templates',
        component: MessageTemplatesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'message-templates/:id',
        component: MessageTemplatesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Message Templates
      {
        path: 'message/:templateId',
        component: MessageStandaloneComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Email Templates
      {
        path: 'email/:templateId',
        component: EmailStandaloneComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Email Commands
      {
        path: 'email-commands',
        component: EmailCommandsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'email-commands/:id',
        component: EmailCommandsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Email Templates
      {
        path: 'email-templates',
        component: EmailTemplatesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'email-templates/:id',
        component: EmailTemplatesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Printing Templates
      {
        path: 'printing-templates',
        component: PrintingTemplatesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'printing-templates/:id',
        component: PrintingTemplatesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Agent Definitions
      {
        path: 'agent-definitions',
        component: AgentDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'agent-definitions/:id',
        component: AgentDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Resource Definitions
      {
        path: 'resource-definitions',
        component: ResourceDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'resource-definitions/:id',
        component: ResourceDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Lookup Definitions
      {
        path: 'lookup-definitions',
        component: LookupDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'lookup-definitions/:id',
        component: LookupDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Line Definitions
      {
        path: 'line-definitions',
        component: LineDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'line-definitions/:id',
        component: LineDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Document Definitions
      {
        path: 'document-definitions',
        component: DocumentDefinitionsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'document-definitions/:id',
        component: DocumentDefinitionsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Report
      {
        path: 'report/:definitionId',
        component: ReportComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Dashboard
      {
        path: 'dashboard/:definitionId',
        component: DashboardComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Drilldown
      {
        path: 'drilldown/:definitionId',
        component: DrilldownComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Account Statement
      {
        path: 'account-statement',
        component: AccountStatementComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'agent-statement/:definitionId',
        component: AgentStatementComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Bank Reconciliation
      {
        path: 'reconciliation',
        component: ReconciliationComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Emails
      {
        path: 'emails',
        component: EmailsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'emails/:id',
        component: EmailsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Messages
      {
        path: 'messages',
        component: MessagesMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'messages/:id',
        component: MessagesDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // General Settings
      {
        path: 'general-settings',
        component: GeneralSettingsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // General Settings
      {
        path: 'financial-settings',
        component: FinancialSettingsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Standalone Print
      {
        path: 'print/:templateId',
        component: PrintComponent,
        canDeactivate: [saveInProgressGuard]
      },

      // Misc
      {
        path: 'main-menu',
        component: MainMenuComponent,
        canDeactivate: [saveInProgressGuard] // for saving my user
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
    MessagesDetailsComponent,
    MessagesMasterComponent,
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
    EmailTemplatesDetailsComponent,
    EmailTemplatesMasterComponent,
    EmailTemplatesPickerComponent,
    EmailCommandsDetailsComponent,
    EmailCommandsMasterComponent,
    EmailCommandsPickerComponent,
    MessageTemplatesDetailsComponent,
    MessageTemplatesMasterComponent,
    MessageTemplatesPickerComponent,
    MessageComponent,
    MessageCommandsMasterComponent,
    MessageCommandsDetailsComponent,
    MessageCommandsPickerComponent,
    MessageButtonComponent,
    MessagePreviewerComponent,
    MessageStandaloneComponent,
    EmailComponent,
    EmailPreviewerComponent,
    EmailButtonComponent,
    EmailStandaloneComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes),
    EllipsisModule,
    LinkyModule
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
      faHandshake,
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
      faMailBulk,
      faMale,
      faMap,
      faMapMarkerAlt,
      faMicrochip,
      faMinus,
      faMoneyBillWave,
      faMoneyCheck,
      faMoneyCheckAlt,
      faMoon,
      faMountain,
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
      faPersonBooth,
      faPills,
      faPlane,
      faPlaneArrival,
      faPortrait,
      faPowerOff,
      faPrint,
      faProjectDiagram,
      faPuzzlePiece,
      faQrcode,
      faQuestionCircle,
      faReceipt,
      faRecycle,
      faScroll,
      faSearchDollar,
      faSeedling,
      faShapes,
      faShare,
      faShareAlt,
      faShareAltSquare,
      faShareSquare,
      faShieldAlt,
      faShip,
      faShoppingCart,
      faSign,
      faSignOutAlt,
      faSitemap,
      faSms,
      faSnowplow,
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
