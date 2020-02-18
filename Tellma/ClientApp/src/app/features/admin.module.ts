import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { AdminMainMenuComponent } from './admin-main-menu/admin-main-menu.component';
import { AdminPageNotFoundComponent } from './admin-page-not-found/admin-page-not-found.component';
import { AdminShellComponent } from './admin-shell/admin-shell.component';
import { UnsavedChangesGuard } from '../data/unsaved-changes.guard';
import { AdminResolverGuard } from '../data/admin-resolver.guard';
import { AuthGuard } from '../data/auth.guard';
import { SaveInProgressGuard } from '../data/save-in-progress.guard';
import { AdminUsersMasterComponent } from './admin-users/admin-users-master.component';
import { AdminUsersDetailsComponent } from './admin-users/admin-users-details.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faShieldAlt, faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons';
import { IdentityServerUsersMasterComponent } from './identity-server-users/identity-server-users-master.component';
import { IdentityServerUsersDetailsComponent } from './identity-server-users/identity-server-users-details.component';

const routes: Routes = [
  {
    path: 'console',
    component: AdminShellComponent,
    canActivate: [AdminResolverGuard],
    canActivateChild: [AuthGuard],
    children: [
      // Identity Server Users
      {
        path: 'identity-server-users',
        component: IdentityServerUsersMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'identity-server-users/:id',
        component: IdentityServerUsersDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // Admin Users
      {
        path: 'admin-users',
        component: AdminUsersMasterComponent,
        canDeactivate: [SaveInProgressGuard]
      },
      {
        path: 'admin-users/:id',
        component: AdminUsersDetailsComponent,
        canDeactivate: [UnsavedChangesGuard]
      },

      // // Settings
      // {
      //   path: 'settings',
      //   component: SettingsComponent,
      //   canDeactivate: [SaveInProgressGuard, UnsavedChangesGuard]
      // },
      // Misc
      {
        path: 'main-menu',
        component: AdminMainMenuComponent,
        canDeactivate: [SaveInProgressGuard] // for saving my user
      },
      { path: '', redirectTo: 'main-menu', pathMatch: 'full' },
      { path: '**', component: AdminPageNotFoundComponent },
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
    AdminShellComponent,
    AdminMainMenuComponent,
    AdminPageNotFoundComponent,
    AdminUsersMasterComponent,
    AdminUsersDetailsComponent,
    IdentityServerUsersMasterComponent,
    IdentityServerUsersDetailsComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class AdminModule {

  constructor(library: FaIconLibrary) {
    library.addIcons(faShieldAlt, faEye, faEyeSlash);
  }

}
