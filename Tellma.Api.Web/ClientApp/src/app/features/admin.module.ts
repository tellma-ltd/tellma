import { inject, NgModule } from '@angular/core';
import { Routes, RouterModule, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { AdminMainMenuComponent } from './admin-main-menu/admin-main-menu.component';
import { AdminPageNotFoundComponent } from './admin-page-not-found/admin-page-not-found.component';
import { AdminShellComponent } from './admin-shell/admin-shell.component';
import { unsavedChangesGuard } from '../data/unsaved-changes.guard';
import { AdminResolverGuard } from '../data/admin-resolver.guard';
import { AuthGuard } from '../data/auth.guard';
import { saveInProgressGuard } from '../data/save-in-progress.guard';
import { AdminUsersMasterComponent } from './admin-users/admin-users-master.component';
import { AdminUsersDetailsComponent } from './admin-users/admin-users-details.component';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faShieldAlt, faEye, faEyeSlash } from '@fortawesome/free-solid-svg-icons';
import { IdentityServerUsersMasterComponent } from './identity-server-users/identity-server-users-master.component';
import { IdentityServerUsersDetailsComponent } from './identity-server-users/identity-server-users-details.component';
import { IdentityServerClientsDetailsComponent } from './identity-server-clients/identity-server-clients-details.component';
import { IdentityServerClientsMasterComponent } from './identity-server-clients/identity-server-clients-master.component';

const routes: Routes = [
  {
    path: 'console',
    component: AdminShellComponent,
    canActivate: [AdminResolverGuard],
    canActivateChild: [(_: ActivatedRouteSnapshot, s: RouterStateSnapshot) => inject(AuthGuard).canActivateChild(s)],
    children: [
      // Identity Server Users
      {
        path: 'identity-server-users',
        component: IdentityServerUsersMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'identity-server-users/:id',
        component: IdentityServerUsersDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Identity Server Clients
      {
        path: 'identity-server-clients',
        component: IdentityServerClientsMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'identity-server-clients/:id',
        component: IdentityServerClientsDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },

      // Admin Users
      {
        path: 'admin-users',
        component: AdminUsersMasterComponent,
        canDeactivate: [saveInProgressGuard]
      },
      {
        path: 'admin-users/:id',
        component: AdminUsersDetailsComponent,
        canDeactivate: [unsavedChangesGuard]
      },
      {
        path: 'main-menu',
        component: AdminMainMenuComponent,
        canDeactivate: [saveInProgressGuard] // for saving my user
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
    IdentityServerUsersDetailsComponent,
    IdentityServerClientsDetailsComponent,
    IdentityServerClientsMasterComponent
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
