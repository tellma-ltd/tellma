import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-account-types-picker',
  templateUrl: './account-types-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: AccountTypesPickerComponent }]
})
export class AccountTypesPickerComponent extends PickerBaseComponent {

}
