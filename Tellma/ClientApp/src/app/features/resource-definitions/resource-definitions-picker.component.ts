import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-resource-definitions-picker',
  templateUrl: './resource-definitions-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ResourceDefinitionsPickerComponent }]
})
export class ResourceDefinitionsPickerComponent extends PickerBaseComponent {

}
