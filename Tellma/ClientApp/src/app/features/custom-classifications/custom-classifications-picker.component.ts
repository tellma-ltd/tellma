import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-custom-classifications-picker',
  templateUrl: './custom-classifications-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: CustomClassificationsPickerComponent }]
})
export class CustomClassificationsPickerComponent extends PickerBaseComponent {

}
