import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { MeasurementUnitsMasterComponent } from './measurement-units/measurement-units-master.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { MeasurementUnitsImportComponent } from './measurement-units/measurement-units-import.component';
import { MeasurementUnitsDetailsComponent } from './measurement-units/measurement-units-details.component';
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
import { IfrsNotesMasterComponent } from './ifrs-notes/ifrs-notes-master.component';
import { IfrsNotesDetailsComponent } from './ifrs-notes/ifrs-notes-details.component';
import { LegacyTypesMasterComponent } from './legacy-types/legacy-types-master.component';
import { LegacyTypesImportComponent } from './legacy-types/legacy-types-import.component';
import { LegacyTypesDetailsComponent } from './legacy-types/legacy-types-details.component';
import { LookupsMasterComponent } from './lookups/lookups-master.component';
import { LookupsDetailsComponent } from './lookups/lookups-details.component';
import { LookupsImportComponent } from './lookups/lookups-import.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import {
  faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd,
  faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder,
  faEuroSign, faTruck, faSitemap, faCoins, faUserFriends, faExchangeAlt, faLock, faFile,
  faFilePdf, faFileWord, faFileExcel, faFilePowerpoint, faFileAlt, faFileArchive, faFileImage,
  faFileAudio, faFileVideo, faThumbsUp, faThumbsDown, faEllipsisV
} from '@fortawesome/free-solid-svg-icons';
import { CurrenciesMasterComponent } from './currencies/currencies-master.component';
import { CurrenciesDetailsComponent } from './currencies/currencies-details.component';
import { CurrenciesImportComponent } from './currencies/currencies-import.component';
import { ResourcesMasterComponent } from './resources/resources-master.component';
import { ResourcesImportComponent } from './resources/resources-import.component';
import { ResourcesDetailsComponent } from './resources/resources-details.component';
import { MeasurementUnitsPickerComponent } from './measurement-units/measurement-units-picker.component';
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
import { ResponsibilityCentersMasterComponent } from './responsibility-centers/responsibility-centers-master.component';
import { ResponsibilityCentersDetailsComponent } from './responsibility-centers/responsibility-centers-details.component';
import { ResponsibilityCentersPickerComponent } from './responsibility-centers/responsibility-centers-picker.component';
import { LegacyTypesPickerComponent } from './legacy-types/legacy-types-picker.component';
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

const routes: Routes = [
  {
    path: ':tenantId',
    component: ApplicationShellComponent,
    canActivate: [TenantResolverGuard],
    canActivateChild: [AuthGuard],
    children: [
      // Measurement Units
      {
        path: 'measurement-units',
        component: MeasurementUnitsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'measurement-units/import',
        component: MeasurementUnitsImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'measurement-units/:id',
        component: MeasurementUnitsDetailsComponent,
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

      // Legacy Types
      {
        path: 'legacy-types',
        component: LegacyTypesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'legacy-types/import',
        component: LegacyTypesImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'legacy-types/:id',
        component: LegacyTypesDetailsComponent,
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

      // Report Definitions
      {
        path: 'responsibility-centers',
        component: ResponsibilityCentersMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'responsibility-centers/:id',
        component: ResponsibilityCentersDetailsComponent,
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
    MeasurementUnitsMasterComponent,
    MeasurementUnitsDetailsComponent,
    MeasurementUnitsImportComponent,
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
    IfrsNotesMasterComponent,
    IfrsNotesDetailsComponent,
    LegacyTypesMasterComponent,
    LegacyTypesImportComponent,
    LegacyTypesDetailsComponent,
    LegacyTypesPickerComponent,
    LookupsMasterComponent,
    LookupsDetailsComponent,
    LookupsImportComponent,
    CurrenciesMasterComponent,
    CurrenciesDetailsComponent,
    CurrenciesImportComponent,
    ResourcesMasterComponent,
    ResourcesDetailsComponent,
    ResourcesImportComponent,
    MeasurementUnitsPickerComponent,
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
    ResponsibilityCentersMasterComponent,
    ResponsibilityCentersDetailsComponent,
    ResponsibilityCentersPickerComponent,
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
      faThumbsUp, faThumbsDown,
      // File icons
      faFile, faFilePdf, faFileWord, faFileExcel, faFilePowerpoint, faFileAlt,
      faFileArchive, faFileImage, faFileVideo, faFileAudio, faEllipsisV,

      // Main menu icons
      faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd, faSitemap, faCoins,
      faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder, faEuroSign,
      faTruck, faUserFriends, faExchangeAlt, faLock
    );
  }
}
