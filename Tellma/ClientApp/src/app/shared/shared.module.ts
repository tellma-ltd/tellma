import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { CDK_DRAG_CONFIG, DragDropModule } from '@angular/cdk/drag-drop';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import {
  NgbCollapseModule,
  NgbDropdownModule,
  NgbModalModule,
  NgbPopoverModule,
  NgbDatepickerModule,
  NgbDateAdapter,
  NgbDatepickerI18n,
  NgbTooltipModule,
  NgbNavModule
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
  faUsers,
  faCog,
  faTasks,
  faBuilding,
  faChartPie,

  faBars,
  faTable,
  faExternalLinkAlt,
  faMinusSquare,
  faChartLine,
  faInfoCircle,
  faTools,
  faCamera,
  faRuler

} from '@fortawesome/free-solid-svg-icons';
import { BrandComponent } from './brand/brand.component';
import { DecimalEditorComponent } from './decimal-editor/decimal-editor.component';
import { DetailsComponent } from './details/details.component';
import { DetailsBaseComponent } from './details-base/details-base.component';
import { ErrorMessageComponent } from './error-message/error-message.component';
import { FormGroupComponent } from './form-group/form-group.component';
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
import { PickerBaseComponent } from './picker-base/picker-base.component';
import { FormGroupBaseComponent } from '../shared/form-group-base/form-group-base.component';
import { FormGroupDynamicComponent } from '../shared/form-group-dynamic/form-group-dynamic.component';
import { FormGroupSettingsComponent } from '../shared/form-group-settings/form-group-settings.component';
import { FormGroupCellComponent } from '../shared/form-group-cell/form-group-cell.component';
import { SerialEditorComponent } from './serial-editor/serial-editor.component';
import { MapBoundsFitterComponent } from './map-bounds-fitter/map-bounds-fitter.component';
import { AccountingPipe } from './accounting/accounting.pipe';
import { ContextMenuDirective } from '../data/context-menu.directive';

@NgModule({
  declarations: [
    BrandComponent,
    DecimalEditorComponent,
    SerialEditorComponent,
    DetailsComponent,
    DetailsBaseComponent,
    MasterBaseComponent,
    ErrorMessageComponent,
    FormGroupComponent,
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
    LabelPipe,
    PickerBaseComponent,
    FormGroupBaseComponent,
    FormGroupDynamicComponent,
    FormGroupSettingsComponent,
    FormGroupCellComponent,
    MapBoundsFitterComponent,
    AccountingPipe,
    ContextMenuDirective
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
    NgbNavModule,
    NgbTooltipModule,
    NgxChartsModule,
  ],
  exports: [
    // Modules
    CommonModule,
    TranslateModule,
    FormsModule,
    FontAwesomeModule,
    HttpClientModule,
    ScrollingModule,
    DragDropModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbCollapseModule,
    NgbPopoverModule,
    NgbNavModule,
    NgbTooltipModule,
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
    FormGroupComponent,
    FormGroupSettingsComponent,
    FormGroupDynamicComponent,
    FormGroupCellComponent,
    SelectorComponent,
    DecimalEditorComponent,
    SerialEditorComponent,
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
    LabelPipe,
    MapBoundsFitterComponent,
    AccountingPipe,
    ContextMenuDirective
  ],
  providers: [
    { provide: NgbDateAdapter, useClass: NgbDateStringAdapter },
    { provide: NgbDatepickerI18n, useClass: DatePickerLocalization },
    { provide: CDK_DRAG_CONFIG, useValue: { zIndex: 10000 } }
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
      faAsterisk, faCameraRetro, faUser, faRuler, faUsers, faCog,
      faTasks, faBuilding, faChartPie, faInfoCircle, faTools, faCamera,

      faBars,
      faChartLine, faExternalLinkAlt, faMinusSquare
    );
  }
}
