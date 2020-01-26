import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-legacy-types-picker',
  templateUrl: './legacy-types-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: LegacyTypesPickerComponent }]
})
export class LegacyTypesPickerComponent extends PickerBaseComponent {

}
