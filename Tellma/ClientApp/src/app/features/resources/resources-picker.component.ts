import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-resources-picker',
  templateUrl: './resources-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ResourcesPickerComponent }]
})
export class ResourcesPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: string[];

}
