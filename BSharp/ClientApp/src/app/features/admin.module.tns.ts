import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { AdminRoutingModule } from './admin-routing.module';
import { NativeScriptCommonModule } from 'nativescript-angular/common';

@NgModule({
  declarations: [],
  imports: [
    AdminRoutingModule,
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class AdminModule { }
