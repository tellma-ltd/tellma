import { NgModule, Component } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules } from '@angular/router';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { UnauthorizedForCompanyComponent } from './features/unauthorized-for-company/unauthorized-for-company.component';
import { ErrorLoadingCompanyComponent } from './features/error-loading-company/error-loading-company.component';
import { AuthGuard } from './data/auth.guard';
import { SignInCallbackGuard } from './data/sign-in-callback.guard';
import { BaseAddressGuard } from './data/base-address.guard';


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

@Component({ template: '<p></p>'}) export class PlaceholderComponent { }


/*
  the root routes for the angular router
*/
const routes: Routes = [
  { path: 'companies', component: CompaniesComponent, canActivate: [AuthGuard] },

  // Lazy loaded modules,
  {
    path: 'app',
    canActivate: [AuthGuard], // otherwise the tenant resolver can't work
    loadChildren: './features/application.module#ApplicationModule',
    data: { preload: true }
  },
  {
    path: 'admin',
    loadChildren: './features/admin.module#AdminModule',
    data: { preload: false }
  },
  {
    path: 'identity',
    loadChildren: './features/identity.module#IdentityModule',
    data: { preload: true }
  },
  {
    path: 'welcome',
    loadChildren: './features/landing.module#LandingModule',
    data: { preload: false }
  },
  { path: 'unauthorized', component: UnauthorizedForCompanyComponent },
  { path: 'error-loading-company', component: ErrorLoadingCompanyComponent },
  { path: 'sign-in-callback', component: PlaceholderComponent, canActivate: [SignInCallbackGuard] },
  { path: '', component: PlaceholderComponent, canActivate: [BaseAddressGuard] },

  { path: '**', component: PageNotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes,
    { enableTracing: false, preloadingStrategy: PreloadAllModules })], // TODO preload only select modules
  exports: [RouterModule]
})
export class RootRoutingModule { }
