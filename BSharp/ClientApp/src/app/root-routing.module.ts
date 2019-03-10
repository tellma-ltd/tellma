import { NgModule, Component } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules } from '@angular/router';
import { CompaniesComponent } from './features/companies/companies.component';
import { AuthGuard } from './data/auth.guard';
import { SignInCallbackGuard } from './data/sign-in-callback.guard';
import { BaseAddressGuard } from './data/base-address.guard';
import { GlobalResolverGuard } from './data/global-resolver.guard';
import { RootShellComponent } from './features/root-shell/root-shell.component';
import { LandingComponent } from './features/landing/landing.component';
import { ErrorComponent } from './features/error/error.component';


/*
  this component is never displayed, we use it to do redirecting, we set it in a route and protect
  it with a guard that always returns false, and the guard is responsible for redirecting the user

  -- example usage --

  (1) sign-in-callback the guard is responsible for parsing and validating the authentication tokens
  that come from the identity server, and then redirecting the user to the originally requested URL,
  or to the home page if there was no requested URL

  (2) base address '/', the guard checks if the user is authenticated, if s/he is the guard redirects
  the user to the application module, otherwise to the landing module/welcome screen
*/

@Component({ template: '<div></div>' }) export class PlaceholderComponent { }

/*
  the root routes for the angular router
*/
const routes: Routes = [
  {
    path: 'root',
    component: RootShellComponent,
    canActivate: [GlobalResolverGuard],
    children: [
      {
        path: 'welcome',
        component: LandingComponent,
      },
      {
        path: 'companies',
        component: CompaniesComponent,
        canActivate: [AuthGuard]
      },
      {
        path: 'error/:error',
        component: ErrorComponent
      },
      {
        path: '',
        redirectTo: 'welcome',
        pathMatch: 'full'
      },
      {
        path: '**',
        redirectTo: 'error/page-not-found',
        // component: PageNotFoundComponent
      }
    ]
  },

  // Lazy loaded modules,
  {
    path: 'app',
    canActivate: [AuthGuard, GlobalResolverGuard], // otherwise the tenant resolver can't work
    loadChildren: './features/application.module#ApplicationModule',
    data: { preload: true }
  },
  {
    path: 'admin',
    loadChildren: './features/admin.module#AdminModule',
    data: { preload: false }
  },
  // {
  //   path: 'identity',
  //   loadChildren: './features/identity.module#IdentityModule',
  //   data: { preload: true }
  // },

  // global error screen
  { path: 'error/:error', component: ErrorComponent },

  // those paths always end in a redirect
  { path: 'sign-in-callback', component: PlaceholderComponent, canActivate: [SignInCallbackGuard] },
  { path: '', component: PlaceholderComponent, canActivate: [BaseAddressGuard] },

  // page not found
  {
    path: '**',
    redirectTo: 'error/page-not-found',
    // component: PageNotFoundComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [RouterModule]
})
export class RootRoutingModule { }
