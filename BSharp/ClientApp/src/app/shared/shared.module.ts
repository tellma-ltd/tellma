import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { CdkTableModule } from '@angular/cdk/table';
import { CdkTreeModule } from '@angular/cdk/tree';
import { ScrollingModule } from '@angular/cdk/scrolling';
import {
  NgbCollapseModule,
  NgbDropdownModule,
  NgbModalModule,
  NgbPopoverModule,
  NgbDatepickerModule,
  NgbDateAdapter,
  NgbDatepickerI18n,
  NgbTabsetModule,
  NgbTooltipModule
} from '@ng-bootstrap/ng-bootstrap';

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
  faAngleRight,
  faThLarge,
  faTrash,
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
  faFilter,
  faCalendarAlt,
  faPen,
  faSearch,
  faAsterisk,
  faCameraRetro,
  faUser,
  faRulerCombined,
  faUsers,
  faCog,
  faTasks,
  faMale,
  faBuilding,

  faListUl,
  faMoneyCheck,
  faMoneyCheckAlt,
  faHandHoldingUsd,
  faLandmark,
  faFileContract,
  faFileInvoiceDollar,
  faMoneyBillWave,
  faArrowLeft,
  faArrowRight,
  faClipboard,
  faBars,
  faTable,
  faList,
  faCodeBranch

} from '@fortawesome/free-solid-svg-icons';
import { BrandComponent } from './brand/brand.component';
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
import { MasterBaseComponent } from './master-base/master-base.component';
import { DatePickerComponent } from './date-picker/date-picker.component';
import { NgbDateStringAdapter } from './date-picker/date-string-adapter';
import { DatePickerLocalization } from './date-picker/date-picker-localization';
import { TableComponent } from './table/table.component';
import { DetailsPickerComponent } from './details-picker/details-picker.component';
import { DropdownAppendToBodyDirective } from './details-picker/dropdown-append-to-body.directive';
import { ViewLinkComponent } from './view-link/view-link.component';
import { ImageComponent } from './image/image.component';
import { RestrictedComponent } from './restricted/restricted.component';

// Icons to be used in the web app
library.add(
  faExclamationTriangle, faSpinner, faSignInAlt, faSignOutAlt,
  faCheck, faPlus, faSyncAlt, faAngleDoubleLeft,
  faAngleLeft, faAngleRight, faThLarge, faTable, faPen, faTrash,
  faSave, faTimes, faDownload, faArrowCircleRight, faThumbsUp, faThumbsDown,
  faUndo, faClipboardCheck, faUpload, faFileDownload, faFilter, faCalendarAlt,
  faSearch, faAsterisk, faCameraRetro, faUser, faRulerCombined, faUsers, faCog,
  faTasks, faMale, faBuilding, faArrowLeft, faArrowRight, faBars,

  faCodeBranch,
  faList,
  faListUl,
  faMoneyCheck,
  faMoneyCheckAlt,
  faHandHoldingUsd,
  faLandmark,
  faFileContract,
  faFileInvoiceDollar,
  faMoneyBillWave,
  faClipboard
);

@NgModule({
  declarations: [
    BrandComponent,
    DecimalEditorComponent,
    DetailsComponent,
    DetailsBaseComponent,
    MasterBaseComponent,
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
    DetailsPickerComponent,
    DropdownAppendToBodyDirective,
    ViewLinkComponent,
    ImageComponent,
    RestrictedComponent
  ],
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    RouterModule.forChild([]),
    HttpClientModule,
    CdkTableModule,
    CdkTreeModule,
    ScrollingModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbPopoverModule,
    NgbDatepickerModule,
    NgbTabsetModule,
    NgbTooltipModule
  ],
  exports: [
    // Modules
    CommonModule,
    HttpClientModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    NgbTabsetModule,
    NgbTooltipModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    CdkTableModule,
    CdkTreeModule,

    // Components & others
    SpinnerComponent,
    BrandComponent,
    MasterComponent,
    DetailsComponent,
    ErrorMessageComponent,
    SuccessMessageComponent,
    WarningMessageComponent,
    TextEditorComponent,
    ImportComponent,
    FormGroupComponent,
    SelectorComponent,
    DecimalEditorComponent,
    DetailsBaseComponent,
    MasterBaseComponent,
    DatePickerComponent,
    TableComponent,
    DetailsPickerComponent,
    ViewLinkComponent,
    ImageComponent,
    DropdownAppendToBodyDirective,
    RestrictedComponent
  ],
  providers: [
    { provide: NgbDateAdapter, useClass: NgbDateStringAdapter },
    { provide: NgbDatepickerI18n, useClass: DatePickerLocalization }
  ]
})
export class SharedModule { }
