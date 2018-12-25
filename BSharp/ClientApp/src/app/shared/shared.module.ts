import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NgbCollapseModule, NgbDropdownModule, NgbModalModule, NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { CdkTableModule } from '@angular/cdk/table';

import { BrandComponent } from './brand/brand.component';
import { DetailsComponent } from './details/details.component';
import { MasterComponent } from './master/master.component';
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
  faFileDownload
} from '@fortawesome/free-solid-svg-icons';
import { RouterModule } from '@angular/router';
import { SpinnerComponent } from './spinner/spinner.component';
import { ErrorMessageComponent } from './error-message/error-message.component';
import { TextEditorComponent } from './text-editor/text-editor.component';
import { ImportComponent } from './import/import.component';
import { SuccessMessageComponent } from './success-message/success-message.component';

// Icons to be used in the app
library.add(
  faExclamationTriangle, faSpinner, faSignInAlt, faSignOutAlt,
  faCheck, faPlus, faSyncAlt, faAngleDoubleLeft,
  faAngleLeft, faAngleRight, faThLarge, faList, faEdit, faTrashAlt,
  faSave, faTimes, faDownload, faArrowCircleRight, faThumbsUp, faThumbsDown,
  faUndo, faClipboardCheck, faUpload, faFileDownload
);

@NgModule({
  declarations: [
    BrandComponent,
    MasterComponent,
    DetailsComponent,
    SpinnerComponent,
    ErrorMessageComponent,
    SuccessMessageComponent,
    TextEditorComponent,
    ImportComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    RouterModule.forChild([]),
    HttpClientModule,
    CdkTableModule
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
    ErrorMessageComponent,
    SuccessMessageComponent,
    TextEditorComponent,
    CdkTableModule,
    ImportComponent
  ]
})
export class SharedModule { }
