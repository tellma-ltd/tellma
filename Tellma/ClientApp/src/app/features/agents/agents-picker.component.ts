import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-agents-picker',
  templateUrl: './agents-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: AgentsPickerComponent }]
})
export class AgentsPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: string[];
}
