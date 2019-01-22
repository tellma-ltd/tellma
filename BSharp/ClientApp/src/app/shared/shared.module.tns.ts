import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { NativeScriptCommonModule } from 'nativescript-angular/common';
import { BrandComponent } from './brand/brand.component';
import { TranslateModule } from '@ngx-translate/core';
import { DecimalEditorComponent } from './decimal-editor/decimal-editor.component';
import { DetailsComponent } from './details/details.component';
import { DetailsBaseComponent } from './details-base/details-base.component';
import { ErrorMessageComponent } from './error-message/error-message.component';
import { FormGroupComponent } from './form-group/form-group.component';
import { ImportComponent } from './import/import.component';
import { MasterComponent } from './master/master.component';
import { SelectorComponent } from './selector/selector.component';
import { SpinnerComponent } from './spinner/spinner.component';
import { SuccessMessageComponent } from './success-message/success-message.component';
import { TextEditorComponent } from './text-editor/text-editor.component';
import { WarningMessageComponent } from './warning-message/warning-message.component';
import { NativeScriptUISideDrawerModule } from 'nativescript-ui-sidedrawer/angular';
import { NativeScriptUIListViewModule } from 'nativescript-ui-listview/angular';
import { NativeScriptFormsModule } from 'nativescript-angular/forms';
import { DatePickerComponent } from './date-picker/date-picker.component';
import { TableComponent } from './table/table.component';
import { DetailsPickerComponent } from './details-picker/details-picker.component';
// import { NativeScriptLocalizeModule } from 'nativescript-localize/angular';

@NgModule({
  declarations: [
    BrandComponent,
    DecimalEditorComponent,
    DetailsComponent,
    DetailsBaseComponent,
    ErrorMessageComponent,
    FormGroupComponent,
    ImportComponent,
    MasterComponent,
    SelectorComponent,
    SpinnerComponent,
    SuccessMessageComponent,
    TextEditorComponent,
    WarningMessageComponent,
    DatePickerComponent,
    TableComponent,
    DetailsPickerComponent],
  imports: [
    NativeScriptCommonModule
  ],
  exports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    TranslateModule,
    NativeScriptUISideDrawerModule,
    NativeScriptUIListViewModule,
    // NativeScriptLocalizeModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SharedModule { }
