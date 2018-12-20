import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { IdentityRoutingModule } from './identity-routing.module';
import { NativeScriptCommonModule } from 'nativescript-angular/common';

@NgModule({
  declarations: [],
  imports: [
    IdentityRoutingModule,
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class IdentityModule { }
