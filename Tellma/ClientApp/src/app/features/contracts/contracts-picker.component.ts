import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-contracts-picker',
  templateUrl: './contracts-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ContractsPickerComponent }]
})
export class ContractsPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: string[];
}
