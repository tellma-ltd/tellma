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

const routes: Routes = [
  {
    path: ':tenantId',
    component: ApplicationShellComponent,
    children: [
      // Measurement Units
      {
        path: 'measurement-units',
        component: MeasurementUnitsMasterComponent, canDeactivate: [SaveInProgressGuard]
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

      { path: 'main-menu', component: MainMenuComponent },
      { path: '', redirectTo: 'measurement-units', pathMatch: 'full' },
      { path: '**', component: ApplicationPageNotFoundComponent },
    ]
  }
];


@NgModule({
  declarations: [
    ApplicationShellComponent,
    MeasurementUnitsMasterComponent,
    MeasurementUnitsDetailsComponent,
    MeasurementUnitsImportComponent,
    ApplicationPageNotFoundComponent,
    MainMenuComponent],
  imports: [
    SharedModule,
    RouterModule.forChild(routes)
  ]
})
export class ApplicationModule { }
