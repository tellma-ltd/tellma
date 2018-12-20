import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { LandingRoutingModule } from './landing-routing.module';
import { NativeScriptCommonModule } from 'nativescript-angular/common';

@NgModule({
  declarations: [],
  imports: [
    LandingRoutingModule,
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class LandingModule { }
