import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { UnitsMasterComponent } from './units/units-master.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { UnitsImportComponent } from './units/units-import.component';
import { UnitsDetailsComponent } from './units/units-details.component';
import { SaveInProgressGuard } from '~/app/data/save-in-progress.guard';
import { UnsavedChangesGuard } from '~/app//data/unsaved-changes.guard';
import { AgentsMasterComponent } from './agents/agents-master.component';
import { AgentsImportComponent } from './agents/agents-import.component';
import { AgentsDetailsComponent } from './agents/agents-details.component';
import { RolesMasterComponent } from './roles/roles-master.component';
import { RolesImportComponent } from './roles/roles-import.component';
import { RolesDetailsComponent } from './roles/roles-details.component';
import { UsersDetailsComponent } from './users/users-details.component';
import { UsersMasterComponent } from './users/users-master.component';
import { UsersImportComponent } from './users/users-import.component';
import { SettingsComponent } from './settings/settings.component';
import { TenantResolverGuard } from '../data/tenant-resolver.guard';
import { AuthGuard } from '../data/auth.guard';
import { LookupsMasterComponent } from './lookups/lookups-master.component';
import { LookupsDetailsComponent } from './lookups/lookups-details.component';
import { LookupsImportComponent } from './lookups/lookups-import.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import {
  faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd,
  faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder,
  faEuroSign, faTruck, faSitemap, faCoins, faUserFriends, faExchangeAlt, faLock, faFile,
  faFilePdf, faFileWord, faFileExcel, faFilePowerpoint, faFileAlt, faFileArchive, faFileImage,
  faFileAudio, faFileVideo, faThumbsUp, faThumbsDown, faEllipsisV, faEllipsisH, faArchive,
  faLaptop, faMicrochip, faLaptopCode, faUser, faUserTie, faUserTag, faUserShield,
  faUsersCog,
  faFemale,
  faMale,
  faPaperclip,
  faBook,
  faChartBar,
  faChartArea,
  faShoppingCart,
  faProjectDiagram,
  faExclamation,
  faLockOpen,
  faInbox,
  faShareSquare,
  faShare
} from '@fortawesome/free-solid-svg-icons';
import { CurrenciesMasterComponent } from './currencies/currencies-master.component';
import { CurrenciesDetailsComponent } from './currencies/currencies-details.component';
import { CurrenciesImportComponent } from './currencies/currencies-import.component';
import { ResourcesMasterComponent } from './resources/resources-master.component';
import { ResourcesImportComponent } from './resources/resources-import.component';
import { ResourcesDetailsComponent } from './resources/resources-details.component';
import { UnitsPickerComponent } from './units/units-picker.component';
import { LookupsPickerComponent } from './lookups/lookups-picker.component';
import { LegacyClassificationsMasterComponent } from './legacy-classifications/legacy-classifications-master.component';
import { LegacyClassificationsDetailsComponent } from './legacy-classifications/legacy-classifications-details.component';
import { LegacyClassificationsImportComponent } from './legacy-classifications/legacy-classifications-import.component';
import { LegacyClassificationsPickerComponent } from './legacy-classifications/legacy-classifications-picker.component';
import { AccountTypesMasterComponent } from './account-types/account-types-master.component';
import { AccountTypesDetailsComponent } from './account-types/account-types-details.component';
import { AccountTypesPickerComponent } from './account-types/account-types-picker.component';
import { AccountsMasterComponent } from './accounts/accounts-master.component';
import { AccountsDetailsComponent } from './accounts/accounts-details.component';
import { AccountsImportComponent } from './accounts/accounts-import.component';
import { AccountsPickerComponent } from './accounts/accounts-picker.component';
import { AgentsPickerComponent } from './agents/agents-picker.component';
import { ResourcesPickerComponent } from './resources/resources-picker.component';
import { ReportComponent } from './report/report.component';
import { ReportResultsComponent } from './report-results/report-results.component';
import { ReportDefinitionsMasterComponent } from './report-definitions/report-definitions-master.component';
import { ReportDefinitionsDetailsComponent } from './report-definitions/report-definitions-details.component';
import { ReportDefinitionsImportComponent } from './report-definitions/report-definitions-import.component';
import { CentersMasterComponent } from './centers/centers-master.component';
import { CentersDetailsComponent } from './centers/centers-details.component';
import { CentersPickerComponent } from './centers/centers-picker.component';
import { UsersPickerComponent } from './users/users-picker.component';
import { RolesPickerComponent } from './roles/roles-picker.component';
import { CurrenciesPickerComponent } from './currencies/currencies-picker.component';
import { EntryTypesMasterComponent } from './entry-types/entry-types-master.component';
import { EntryTypesImportComponent } from './entry-types/entry-types-import.component';
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
        path: 'units/import',
        component: UnitsImportComponent,
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
        path: 'roles/import',
        component: RolesImportComponent,
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
        path: 'users/import',
        component: UsersImportComponent,
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
        path: 'agents/:definitionId/import',
        component: AgentsImportComponent,
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
        path: 'lookups/:definitionId/import',
        component: LookupsImportComponent,
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
        path: 'currencies/import',
        component: CurrenciesImportComponent,
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
        path: 'resources/:definitionId/import',
        component: ResourcesImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resources/:definitionId/:id',
        component: ResourcesDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Account Classifications
      {
        path: 'legacy-classifications',
        component: LegacyClassificationsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'legacy-classifications/import',
        component: LegacyClassificationsImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'legacy-classifications/:id',
        component: LegacyClassificationsDetailsComponent,
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
        path: 'accounts/import',
        component: AccountsImportComponent,
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
        path: 'entry-types/import',
        component: EntryTypesImportComponent,
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
        path: 'report-definitions/import',
        component: ReportDefinitionsImportComponent,
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

      // Report
      {
        path: 'report/:definitionId',
        component: ReportComponent,
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
    UnitsImportComponent,
    ApplicationPageNotFoundComponent,
    MainMenuComponent,
    AgentsMasterComponent,
    AgentsImportComponent,
    AgentsDetailsComponent,
    RolesMasterComponent,
    RolesImportComponent,
    RolesDetailsComponent,
    UsersDetailsComponent,
    UsersMasterComponent,
    UsersImportComponent,
    SettingsComponent,
    LookupsMasterComponent,
    LookupsDetailsComponent,
    LookupsImportComponent,
    CurrenciesMasterComponent,
    CurrenciesDetailsComponent,
    CurrenciesImportComponent,
    ResourcesMasterComponent,
    ResourcesDetailsComponent,
    ResourcesImportComponent,
    UnitsPickerComponent,
    LookupsPickerComponent,
    LegacyClassificationsMasterComponent,
    LegacyClassificationsDetailsComponent,
    LegacyClassificationsImportComponent,
    LegacyClassificationsPickerComponent,
    AccountTypesMasterComponent,
    AccountTypesDetailsComponent,
    AccountTypesPickerComponent,
    AccountsMasterComponent,
    AccountsDetailsComponent,
    AccountsImportComponent,
    AccountsPickerComponent,
    AgentsPickerComponent,
    ResourcesPickerComponent,
    ReportComponent,
    ReportResultsComponent,
    ReportDefinitionsMasterComponent,
    ReportDefinitionsDetailsComponent,
    ReportDefinitionsImportComponent,
    CentersMasterComponent,
    CentersDetailsComponent,
    CentersPickerComponent,
    UsersPickerComponent,
    RolesPickerComponent,
    CurrenciesPickerComponent,
    EntryTypesMasterComponent,
    EntryTypesImportComponent,
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
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class ApplicationModule {
  constructor(library: FaIconLibrary) {
    // Icons to be used in the web app
    library.addIcons(
      // Document icons
      faThumbsUp, faThumbsDown, faPaperclip, faExclamation, faLockOpen,

      // File icons
      faFile, faFilePdf, faFileWord, faFileExcel, faFilePowerpoint, faFileAlt,
      faFileArchive, faFileImage, faFileVideo, faFileAudio, faEllipsisV, faEllipsisH, faArchive,

      // Main menu icons
      faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd, faSitemap, faCoins,
      faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder, faEuroSign,
      faTruck, faUserFriends, faExchangeAlt, faLock, faLaptop, faMicrochip, faLaptopCode,
      faUser, faUsersCog, faUserTie, faUserTag, faUserShield, faFemale, faMale, faBook, faChartBar, faChartArea,
      faShoppingCart, faProjectDiagram, faShareSquare, faInbox, faShare
    );
  }
}
