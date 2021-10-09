import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-printing-templates-picker',
  templateUrl: './printing-templates-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: PrintingTemplatesPickerComponent }]
})
export class PrintingTemplatesPickerComponent extends PickerBaseComponent {

}
