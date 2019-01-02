import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgbCollapseModule, NgbDropdownModule, NgbModalModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { CdkTableModule } from '@angular/cdk/table';

import { BrandComponent } from './brand/brand.component';
import { MasterComponent } from './master/master.component';
import { DetailsComponent } from './details/details.component';
import { RouterModule } from '@angular/router';
import { SpinnerComponent } from './spinner/spinner.component';
import { ErrorMessageComponent } from './error-message/error-message.component';
import { TextEditorComponent } from './text-editor/text-editor.component';
import { ImportComponent } from './import/import.component';
import { SuccessMessageComponent } from './success-message/success-message.component';
import { WarningMessageComponent } from './warning-message/warning-message.component';
import { FormGroupComponent } from './form-group/form-group.component';

import { library } from '@fortawesome/fontawesome-svg-core';
import {
  faExclamationTriangle,
  faSpinner,
  faSignInAlt,
  faSignOutAlt,
  faCheck,
  faPlus,
  faSyncAlt,
  faAngleDoubleLeft,
  faAngleLeft,
  faAngleRight,  faThLarge,
  faList,
  faEdit,
  faTrashAlt,
  faSave,
  faTimes,
  faDownload,
  faArrowCircleRight,
  faThumbsUp,
  faThumbsDown,
  faUndo,
  faClipboardCheck,
  faUpload,
  faFileDownload,
  faFilter
} from '@fortawesome/free-solid-svg-icons';
import { SelectorComponent } from './selector/selector.component';

// Icons to be used in the app
library.add(
  faExclamationTriangle, faSpinner, faSignInAlt, faSignOutAlt,
  faCheck, faPlus, faSyncAlt, faAngleDoubleLeft,
  faAngleLeft, faAngleRight, faThLarge, faList, faEdit, faTrashAlt,
  faSave, faTimes, faDownload, faArrowCircleRight, faThumbsUp, faThumbsDown,
  faUndo, faClipboardCheck, faUpload, faFileDownload, faFilter
);

@NgModule({
  declarations: [
    BrandComponent,
    MasterComponent,
    DetailsComponent,
    SpinnerComponent,
    ErrorMessageComponent,
    SuccessMessageComponent,
    WarningMessageComponent,
    TextEditorComponent,
    ImportComponent,
    FormGroupComponent,
    SelectorComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    RouterModule.forChild([]),
    HttpClientModule,
    CdkTableModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbPopoverModule
  ],
  exports: [
    CommonModule,
    HttpClientModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    SpinnerComponent,
    BrandComponent,
    MasterComponent,
    DetailsComponent,
    ErrorMessageComponent,
    SuccessMessageComponent,
    WarningMessageComponent,
    TextEditorComponent,
    CdkTableModule,
    ImportComponent,
    FormGroupComponent,
    SelectorComponent
  ]
})
export class SharedModule { }
