import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { UnitsMasterComponent } from './units/units-master.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { UnitsDetailsComponent } from './units/units-details.component';
import { SaveInProgressGuard } from '~/app/data/save-in-progress.guard';
import { UnsavedChangesGuard } from '~/app//data/unsaved-changes.guard';
import { RelationsMasterComponent } from './relations/relations-master.component';
import { RelationsDetailsComponent } from './relations/relations-details.component';
import { RolesMasterComponent } from './roles/roles-master.component';
import { RolesDetailsComponent } from './roles/roles-details.component';
import { UsersDetailsComponent } from './users/users-details.component';
import { UsersMasterComponent } from './users/users-master.component';
import { SettingsComponent } from './settings/settings.component';
import { TenantResolverGuard } from '../data/tenant-resolver.guard';
import { AuthGuard } from '../data/auth.guard';
import { LookupsMasterComponent } from './lookups/lookups-master.component';
import { LookupsDetailsComponent } from './lookups/lookups-details.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import {
  faAnchor,
  faAngleDown,
  faAngleUp,
  faArchive,
  faBalanceScale,
  faBarcode,
  faBolt,
  faBook,
  faBox,
  faBoxes,
  faCampground,
  faCar,
  faCarrot,
  faCarSide,
  faCashRegister,
  faCertificate,
  faChartArea,
  faChartBar,
  faChevronDown,
  faChevronUp,
  faCity,
  faClipboard,
  faCode,
  faCodeBranch,
  faCoins,
  faCopy,
  faDollyFlatbed,
  faDoorClosed,
  faDraftingCompass,
  faEdit,
  faEllipsisH,
  faEllipsisV,
  faEuroSign,
  faExchangeAlt,
  faExclamation,
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
  faFolder,
  faFolderMinus,
  faFolderPlus,
  faFont,
  faFunnelDollar,
  faGasPump,
  faGifts,
  faGrinHearts,
  faHammer,
  faHandHoldingUsd,
  faHandsHelping,
  faHistory,
  faHollyBerry,
  faIdBadge,
  faImage,
  faInbox,
  faIndent,
  faIndustry,
  faLandmark,
  faLaptop,
  faLaptopCode,
  faList,
  faListUl,
  faLock,
  faLockOpen,
  faMale,
  faMap,
  faMapMarkerAlt,
  faMicrochip,
  faMoneyBillWave,
  faMoneyCheck,
  faMoneyCheckAlt,
  faNetworkWired,
  faNewspaper,
  faObjectGroup,
  faPaintRoller,
  faPalette,
  faPallet,
  faPaperclip,
  faParachuteBox,
  faPills,
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
  faShip,
  faShoppingCart,
  faSign,
  faSitemap,
  faSpa,
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
  faTree,
  faTrophy,
  faTruck,
  faUmbrellaBeach,
  faUndoAlt,
  faUniversity,
  faUser,
  faUserCheck,
  faUserClock,
  faUserCog,
  faUserFriends,
  faUserMinus,
  faUserPlus,
  faUsersCog,
  faUserShield,
  faUserTag,
  faUserTie,
  faUtensils,
  faWarehouse,
  faStar,
  faCalendarAlt,
  faArrowsAlt,
  faCartArrowDown,
  faExpand,
  faCompress
} from '@fortawesome/free-solid-svg-icons';
import { faStar as faStarAlternative } from '@fortawesome/free-regular-svg-icons';
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
import { RelationsPickerComponent } from './relations/relations-picker.component';
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
import { MarkupTemplatesMasterComponent } from './markup-templates/markup-templates-master.component';
import { MarkupTemplatesDetailsComponent } from './markup-templates/markup-templates-details.component';
import { DocumentsPickerComponent } from './documents/documents-picker.component';
import { AgentsDetailsComponent } from './agents/agents-details.component';
import { AgentsMasterComponent } from './agents/agents-master.component';
import { AgentsPickerComponent } from './agents/agents-picker.component';
import { AgmCoreModule } from '@agm/core';
import { RelationDefinitionsDetailsComponent } from './relation-definitions/relation-definitions-details.component';
import { RelationDefinitionsMasterComponent } from './relation-definitions/relation-definitions-master.component';
import { RelationDefinitionsPickerComponent } from './relation-definitions/relation-definitions-picker.component';
import { ResourceDefinitionsDetailsComponent } from './resource-definitions/resource-definitions-details.component';
import { ResourceDefinitionsMasterComponent } from './resource-definitions/resource-definitions-master.component';
import { ResourceDefinitionsPickerComponent } from './resource-definitions/resource-definitions-picker.component';
import { LookupDefinitionsMasterComponent } from './lookup-definitions/lookup-definitions-master.component';
import { LookupDefinitionsDetailsComponent } from './lookup-definitions/lookup-definitions-details.component';
import { LookupDefinitionsPickerComponent } from './lookup-definitions/lookup-definitions-picker.component';
import { StatementComponent } from './statement/statement.component';
import { AccountStatementComponent } from './statement/account-statement.component';
import { RelationStatementComponent } from './statement/relation-statement.component';
import { CustodyDefinitionsMasterComponent } from './custody-definitions/custody-definitions-master.component';
import { CustodyDefinitionsDetailsComponent } from './custody-definitions/custody-definitions-details.component';
import { CustodyDefinitionsPickerComponent } from './custody-definitions/custody-definitions-picker.component';
import { CustodiesPickerComponent } from './custodies/custodies-picker.component';
import { CustodiesMasterComponent } from './custodies/custodies-master.component';
import { CustodiesDetailsComponent } from './custodies/custodies-details.component';
import { LineDefinitionsDetailsComponent } from './line-definitions/line-definitions-details.component';
import { LineDefinitionsMasterComponent } from './line-definitions/line-definitions-master.component';
import { LineDefinitionsPickerComponent } from './line-definitions/line-definitions-picker.component';
import { DocumentDefinitionsMasterComponent } from './document-definitions/document-definitions-master.component';
import { DocumentDefinitionsDetailsComponent } from './document-definitions/document-definitions-details.component';
import { DocumentDefinitionsPickerComponent } from './document-definitions/document-definitions-picker.component';
import { MarkupTemplatesPickerComponent } from './markup-templates/markup-templates-picker.component';

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

      // Relations
      {
        path: 'relations',
        component: RelationsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'relations/:definitionId',
        component: RelationsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'relations/:definitionId/:id',
        component: RelationsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Custodies
      {
        path: 'custodies',
        component: CustodiesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'custodies/:definitionId',
        component: CustodiesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'custodies/:definitionId/:id',
        component: CustodiesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Agents
      {
        path: 'agents',
        component: AgentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:id',
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

      // Markup Templates
      {
        path: 'markup-templates',
        component: MarkupTemplatesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'markup-templates/:id',
        component: MarkupTemplatesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Relation Definitions
      {
        path: 'relation-definitions',
        component: RelationDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'relation-definitions/:id',
        component: RelationDefinitionsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Custody Definitions
      {
        path: 'custody-definitions',
        component: CustodyDefinitionsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'custody-definitions/:id',
        component: CustodyDefinitionsDetailsComponent,
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

      // Account Statement
      {
        path: 'account-statement',
        component: AccountStatementComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'relation-statement/:definitionId',
        component: RelationStatementComponent,
        canDeactivate: [SaveInProgressGuard]
      },

      // Settings
      {
        path: 'settings',
        component: SettingsComponent,
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
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
    RelationsMasterComponent,
    RelationsDetailsComponent,
    RolesMasterComponent,
    RolesDetailsComponent,
    UsersDetailsComponent,
    UsersMasterComponent,
    SettingsComponent,
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
    RelationsPickerComponent,
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
    MarkupTemplatesMasterComponent,
    MarkupTemplatesDetailsComponent,
    DocumentsPickerComponent,
    AgentsDetailsComponent,
    AgentsMasterComponent,
    AgentsPickerComponent,
    RelationDefinitionsDetailsComponent,
    RelationDefinitionsMasterComponent,
    RelationDefinitionsPickerComponent,
    ResourceDefinitionsDetailsComponent,
    ResourceDefinitionsMasterComponent,
    ResourceDefinitionsPickerComponent,
    LookupDefinitionsMasterComponent,
    LookupDefinitionsDetailsComponent,
    LookupDefinitionsPickerComponent,
    StatementComponent,
    AccountStatementComponent,
    RelationStatementComponent,
    CustodyDefinitionsMasterComponent,
    CustodyDefinitionsDetailsComponent,
    CustodyDefinitionsPickerComponent,
    CustodiesPickerComponent,
    CustodiesMasterComponent,
    CustodiesDetailsComponent,
    LineDefinitionsDetailsComponent,
    LineDefinitionsMasterComponent,
    LineDefinitionsPickerComponent,
    DocumentDefinitionsMasterComponent,
    DocumentDefinitionsDetailsComponent,
    DocumentDefinitionsPickerComponent,
    MarkupTemplatesPickerComponent,
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes),
    AgmCoreModule.forRoot({
      apiKey: '<Google API Key goes here>'
    })
  ]
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
      faAnchor,
      faAngleDown,
      faAngleUp,
      faArchive,
      faArrowsAlt,
      faBalanceScale,
      faBarcode,
      faBolt,
      faBook,
      faBox,
      faBoxes,
      faCalendarAlt,
      faCampground,
      faCar,
      faCarrot,
      faCarSide,
      faCartArrowDown,
      faCashRegister,
      faCertificate,
      faChartArea,
      faChartBar,
      faChevronDown,
      faChevronUp,
      faCity,
      faClipboard,
      faCode,
      faCodeBranch,
      faCoins,
      faCompress,
      faCopy,
      faDollyFlatbed,
      faDoorClosed,
      faDraftingCompass,
      faEdit,
      faEllipsisH,
      faEllipsisV,
      faEuroSign,
      faExchangeAlt,
      faExclamation,
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
      faFolder,
      faFolderMinus,
      faFolderPlus,
      faFont,
      faFunnelDollar,
      faGasPump,
      faGifts,
      faGrinHearts,
      faHammer,
      faHandHoldingUsd,
      faHandsHelping,
      faHistory,
      faHollyBerry,
      faIdBadge,
      faImage,
      faInbox,
      faIndent,
      faIndustry,
      faLandmark,
      faLaptop,
      faLaptopCode,
      faList,
      faListUl,
      faLock,
      faLockOpen,
      faMale,
      faMap,
      faMapMarkerAlt,
      faMicrochip,
      faMoneyBillWave,
      faMoneyCheck,
      faMoneyCheckAlt,
      faNetworkWired,
      faNewspaper,
      faObjectGroup,
      faPaintRoller,
      faPalette,
      faPallet,
      faPaperclip,
      faParachuteBox,
      faPills,
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
      faShip,
      faShoppingCart,
      faSign,
      faSitemap,
      faSpa,
      faStar,
      faStarAlternative,
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
      faTree,
      faTrophy,
      faTruck,
      faUmbrellaBeach,
      faUndoAlt,
      faUniversity,
      faUser,
      faUserCheck,
      faUserClock,
      faUserCog,
      faUserFriends,
      faUserMinus,
      faUserPlus,
      faUsersCog,
      faUserShield,
      faUserTag,
      faUserTie,
      faUtensils,
      faWarehouse
    );
  }
}
