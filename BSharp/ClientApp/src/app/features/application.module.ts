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
import { ResourceClassificationsMasterComponent } from './resource-classifications/resource-classifications-master.component';
import { ResourceClassificationsImportComponent } from './resource-classifications/resource-classifications-import.component';
import { ResourceClassificationsDetailsComponent } from './resource-classifications/resource-classifications-details.component';
import { LookupsMasterComponent } from './lookups/lookups-master.component';
import { LookupsDetailsComponent } from './lookups/lookups-details.component';
import { LookupsImportComponent } from './lookups/lookups-import.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import {
  faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd,
  faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder, faEuroSign, faTruck
} from '@fortawesome/free-solid-svg-icons';
import { CurrenciesMasterComponent } from './currencies/currencies-master.component';
import { CurrenciesDetailsComponent } from './currencies/currencies-details.component';
import { CurrenciesImportComponent } from './currencies/currencies-import.component';
import { ResourcesMasterComponent } from './resources/resources-master.component';
import { ResourcesImportComponent } from './resources/resources-import.component';
import { ResourcesDetailsComponent } from './resources/resources-details.component';

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
        path: 'agents/import',
        component: AgentsImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:id',
        component: AgentsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // // IFRS Notes
      // {
      //   path: 'ifrs-notes',
      //   component: IfrsNotesMasterComponent,
      //   canDeactivate: [SaveInProgressGuard]
      // },
      // {
      //   path: 'ifrs-notes/:id',
      //   component: IfrsNotesDetailsComponent,
      //   canDeactivate: [UnsavedChangesGuard]
      // },

      // Resource Classifications
      {
        path: 'resource-classifications/:definitionId',
        component: ResourceClassificationsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resource-classifications/:definitionId/import',
        component: ResourceClassificationsImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'resource-classifications/:definitionId/:id',
        component: ResourceClassificationsDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Lookups
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

      // Settings
      {
        path: 'settings',
        component: SettingsComponent,
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
      },
      // Misc
      { path: 'main-menu', component: MainMenuComponent },
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
    ResourceClassificationsMasterComponent,
    ResourceClassificationsImportComponent,
    ResourceClassificationsDetailsComponent,
    LookupsMasterComponent,
    LookupsDetailsComponent,
    LookupsImportComponent,
    CurrenciesMasterComponent,
    CurrenciesDetailsComponent,
    CurrenciesImportComponent,
    ResourcesMasterComponent,
    ResourcesDetailsComponent,
    ResourcesImportComponent
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
      // Main menu icons
      faCodeBranch, faList, faListUl, faMoneyCheck, faMoneyCheckAlt, faHandHoldingUsd,
      faLandmark, faFileContract, faFileInvoiceDollar, faMoneyBillWave, faClipboard, faFolder, faEuroSign, faTruck
    );
  }
}
