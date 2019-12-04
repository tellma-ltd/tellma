import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-lookups-picker',
  templateUrl: './lookups-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: LookupsPickerComponent }]
})
export class LookupsPickerComponent extends PickerBaseComponent {

  @Input()
  definitionId: string;

}
