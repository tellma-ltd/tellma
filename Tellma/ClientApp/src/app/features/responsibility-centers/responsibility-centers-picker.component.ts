import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-responsibility-centers-picker',
  templateUrl: './responsibility-centers-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ResponsibilityCentersPickerComponent }]

})
export class ResponsibilityCentersPickerComponent extends PickerBaseComponent {

}
