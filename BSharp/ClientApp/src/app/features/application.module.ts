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
import { ProductCategoriesMasterComponent } from './product-categories/product-categories-master.component';
import { ProductCategoriesImportComponent } from './product-categories/product-categories-import.component';
import { ProductCategoriesDetailsComponent } from './product-categories/product-categories-details.component';

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
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
      },

      // Agents
      {
        path: 'agents/:agentType',
        component: AgentsMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:agentType/import',
        component: AgentsImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'agents/:agentType/:id',
        component: AgentsDetailsComponent,
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
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
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
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
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
      },

      // IFRS Notes
      {
        path: 'ifrs-notes',
        component: IfrsNotesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'ifrs-notes/:id',
        component: IfrsNotesDetailsComponent,
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
      },

      // Product Categories
      {
        path: 'product-categories',
        component: ProductCategoriesMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'product-categories/import',
        component: ProductCategoriesImportComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'product-categories/:id',
        component: ProductCategoriesDetailsComponent,
        canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
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
    ProductCategoriesMasterComponent,
    ProductCategoriesImportComponent,
    ProductCategoriesDetailsComponent],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class ApplicationModule { }
