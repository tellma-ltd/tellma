import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-relations-picker',
  templateUrl: './relations-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: RelationsPickerComponent }]
})
export class RelationsPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: number[];
}
