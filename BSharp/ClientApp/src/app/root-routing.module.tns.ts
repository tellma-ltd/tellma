import { NgModule } from '@angular/core';
import { NativeScriptRouterModule } from 'nativescript-angular/router';
import { Routes, PreloadAllModules } from '@angular/router';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { CompaniesComponent } from './features/companies/companies.component';

const routes: Routes = [
  {
      path: '',
      redirectTo: '/companies',
      pathMatch: 'full',
  },
  {
      path: 'companies',
      component: CompaniesComponent,
  },
  {
    path: 'app/:tenantId/measurement-units',
    loadChildren: '~/app/features/measurement-units/measurement-units.module#MeasurementUnitsModule',
  },
  { path: '**', component: PageNotFoundComponent },
];

@NgModule({
  imports: [NativeScriptRouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
  exports: [NativeScriptRouterModule]
})
export class RootRoutingModule { }
