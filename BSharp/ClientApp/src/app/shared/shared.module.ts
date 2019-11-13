import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { NgxChartsModule } from '@swimlane/ngx-charts';
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

import {
  faSignOutAlt,
  faCheck,
  faPlus,
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
  faCalendar,
  faPen,
  faAsterisk,
  faCameraRetro,
  faUser,
  faRulerCombined,
  faUsers,
  faCog,
  faTasks,
  faMale,
  faBuilding,
  faChartPie,

  faBars,
  faTable,
  faExternalLinkAlt,
  faMinusSquare,
  faChartBar,
  faChartArea,
  faChartLine,
  faInfoCircle,
  faTachometerAlt

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
import { ViewLinkComponent } from './view-link/view-link.component';
import { ImageComponent } from './image/image.component';
import { RestrictedComponent } from './restricted/restricted.component';
import { AutoCellComponent } from './auto-cell/auto-cell.component';
import { AutoLabelComponent } from './auto-label/auto-label.component';
import { LabelPipe } from './label/label.pipe';

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
    ViewLinkComponent,
    ImageComponent,
    RestrictedComponent,
    AutoCellComponent,
    AutoLabelComponent,
    LabelPipe
  ],
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    RouterModule.forChild([]),
    HttpClientModule,
    ScrollingModule,
    DragDropModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbPopoverModule,
    NgbDatepickerModule,
    NgbTabsetModule,
    NgbTooltipModule,
    NgxChartsModule
  ],
  exports: [
    // Modules
    CommonModule,
    HttpClientModule,
    ScrollingModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    NgbTabsetModule,
    NgbTooltipModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    NgxChartsModule,

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
    RestrictedComponent,
    AutoCellComponent,
    AutoLabelComponent,
    LabelPipe
  ],
  providers: [
    { provide: NgbDateAdapter, useClass: NgbDateStringAdapter },
    { provide: NgbDatepickerI18n, useClass: DatePickerLocalization }
  ]
})
export class SharedModule {
  constructor(library: FaIconLibrary) {
    // Icons to be used in the web app
    library.addIcons(
      faSignOutAlt, faCheck, faPlus, faAngleDoubleLeft,
      faAngleLeft, faAngleRight, faThLarge, faTable, faPen, faTrash,
      faSave, faTimes, faDownload, faArrowCircleRight, faThumbsUp, faThumbsDown,
      faUndo, faClipboardCheck, faUpload, faFileDownload, faFilter, faCalendar,
      faAsterisk, faCameraRetro, faUser, faRulerCombined, faUsers, faCog,
      faTasks, faMale, faBuilding, faChartPie, faInfoCircle,

      faBars,
      faChartLine, faExternalLinkAlt, faMinusSquare
    );
  }
}
