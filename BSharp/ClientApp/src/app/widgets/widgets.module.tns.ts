import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { WidgetsRoutingModule } from './widgets-routing.module';
import { NativeScriptCommonModule } from 'nativescript-angular/common';

@NgModule({
  declarations: [],
  imports: [
    WidgetsRoutingModule,
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class WidgetsModule { }
