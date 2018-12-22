import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { NativeScriptCommonModule } from 'nativescript-angular/common';
import { MasterComponent } from './master/master.component';
import { DetailsComponent } from './details/details.component';

@NgModule({
  declarations: [MasterComponent, DetailsComponent],
  imports: [
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SharedModule { }
