import { NgModule } from '@angular/core';
import { Routes, RouterModule, PreloadAllModules, NoPreloading } from '@angular/router';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { UnauthorizedForCompanyComponent } from './features/unauthorized-for-company/unauthorized-for-company.component';
import { ErrorLoadingCompanyComponent } from './features/error-loading-company/error-loading-company.component';

const routes: Routes = [
  { path: 'companies', component: CompaniesComponent },

  // Lazy loaded modules,
  {
    path: 'app',
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

  { path: '', redirectTo: 'companies', pathMatch: 'full' },
  { path: '**', component: PageNotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })], // TODO preload only select modules
  exports: [RouterModule]
})
export class RootRoutingModule { }
