import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { ApplicationRoutingModule } from './application-routing.module';
import { NativeScriptCommonModule } from 'nativescript-angular/common';

@NgModule({
  declarations: [],
  imports: [
    ApplicationRoutingModule,
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class ApplicationModule { }
