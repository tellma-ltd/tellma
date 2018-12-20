import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ApplicationRoutingModule } from './application-routing.module';
import { MeasurementUnitsDetailsComponent } from './measurement-units/measurement-units-details.component';
import { MeasurementUnitsMasterComponent } from './measurement-units/measurement-units-master.component';
import { MainMenuComponent } from './main-menu/main-menu.component';
import { ApplicationShellComponent } from './application-shell/application-shell.component';
import { ApplicationPageNotFoundComponent } from './application-page-not-found/application-page-not-found.component';
import { WidgetsModule } from '../widgets/widgets.module';


@NgModule({
  declarations: [
    MeasurementUnitsDetailsComponent,
    MeasurementUnitsMasterComponent,
    MainMenuComponent,
    ApplicationShellComponent,
    ApplicationPageNotFoundComponent,
  ],
  imports: [
    CommonModule,
    ApplicationRoutingModule,
    WidgetsModule
  ]
})
export class ApplicationModule { }
