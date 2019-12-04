import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 'b-resource-classifications-picker',
  templateUrl: './resource-classifications-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ResourceClassificationsPickerComponent }]
})
export class ResourceClassificationsPickerComponent extends PickerBaseComponent {

}
