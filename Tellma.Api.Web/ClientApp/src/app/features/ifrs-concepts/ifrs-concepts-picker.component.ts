import { Component } from '@angular/core';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 't-ifrs-concepts-picker',
  templateUrl: './ifrs-concepts-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: IfrsConceptsPickerComponent }]
})
export class IfrsConceptsPickerComponent extends PickerBaseComponent {

}
