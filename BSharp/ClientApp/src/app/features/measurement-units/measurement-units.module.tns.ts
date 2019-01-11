import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { MeasurementUnitsMasterComponent } from './measurement-units-master.component';
import { MeasurementUnitsDetailsComponent } from './measurement-units-details.component';
import { Routes } from '@angular/router';
import { NativeScriptRouterModule } from 'nativescript-angular/router';

const routes: Routes = [
  {
    path: '',
    component: MeasurementUnitsMasterComponent,
  },
  {
    path: ':id',
    component: MeasurementUnitsDetailsComponent,
  },
];

@NgModule({
  declarations: [MeasurementUnitsMasterComponent, MeasurementUnitsDetailsComponent],
  imports: [
    SharedModule,
    NativeScriptRouterModule.forChild(routes)
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class MeasurementUnitsModule { }
