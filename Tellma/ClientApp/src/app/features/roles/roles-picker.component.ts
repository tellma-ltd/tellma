import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-roles-picker',
  templateUrl: './roles-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: RolesPickerComponent }]
})
export class RolesPickerComponent extends PickerBaseComponent {

  @Input()
  showMembers = true;

}
