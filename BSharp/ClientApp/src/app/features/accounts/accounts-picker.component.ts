import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-accounts-picker',
  templateUrl: './accounts-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: AccountsPickerComponent }]
})
export class AccountsPickerComponent extends PickerBaseComponent {

}
