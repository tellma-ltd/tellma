import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-measurement-units-picker',
  templateUrl: './measurement-units-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: MeasurementUnitsPickerComponent }]
})
export class MeasurementUnitsPickerComponent extends PickerBaseComponent {

}
