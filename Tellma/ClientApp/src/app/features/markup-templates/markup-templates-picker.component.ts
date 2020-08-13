import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-markup-templates-picker',
  templateUrl: './markup-templates-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: MarkupTemplatesPickerComponent }]
})
export class MarkupTemplatesPickerComponent extends PickerBaseComponent {

}
