import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';

import { NativeScriptCommonModule } from 'nativescript-angular/common';
import { MasterComponent } from './master/master.component';
import { DetailsComponent } from './details/details.component';
import { SpinnerComponent } from './spinner/spinner.component';
import { ErrorMessageComponent } from './error-message/error-message.component';
import { TextEditorComponent } from './text-editor/text-editor.component';

@NgModule({
  declarations: [MasterComponent, DetailsComponent, SpinnerComponent, ErrorMessageComponent, TextEditorComponent],
  imports: [
    NativeScriptCommonModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SharedModule { }
