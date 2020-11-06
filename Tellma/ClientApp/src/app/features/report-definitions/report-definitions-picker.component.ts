import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-report-definitions-picker',
  templateUrl: './report-definitions-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ReportDefinitionsPickerComponent }]
})
export class ReportDefinitionsPickerComponent extends PickerBaseComponent {

}
