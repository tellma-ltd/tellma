import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MeasurementUnitsDetailsComponent } from './measurement-units/measurement-units-details.component';
import { MeasurementUnitsMasterComponent } from './measurement-units/measurement-units-master.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { MeasurementUnitsImportComponent } from './measurement-units/measurement-units-import.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';

const routes: Routes = [
  {
    path: ':tenantId',
    component: ApplicationShellComponent,
    children: [
      { path: 'measurement-units', component: MeasurementUnitsMasterComponent },
      { path: 'measurement-units/import', component: MeasurementUnitsImportComponent },
      { path: 'measurement-units/:id', component: MeasurementUnitsDetailsComponent },

      { path: 'main-menu', component: MainMenuComponent },
      { path: '', redirectTo: 'main-menu', pathMatch: 'full' },
      { path: '**', component: ApplicationPageNotFoundComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ApplicationRoutingModule { }
