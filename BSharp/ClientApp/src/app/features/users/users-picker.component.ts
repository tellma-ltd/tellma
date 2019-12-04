import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-users-picker',
  templateUrl: './users-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: UsersPickerComponent }]
})
export class UsersPickerComponent extends PickerBaseComponent {

  @Input()
  showRoles = true;
}
