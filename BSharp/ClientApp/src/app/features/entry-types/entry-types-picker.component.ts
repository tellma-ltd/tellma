import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-entry-types-picker',
  templateUrl: './entry-types-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: EntryTypesPickerComponent }]
})
export class EntryTypesPickerComponent extends PickerBaseComponent {
}
