import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-documents-picker',
  templateUrl: './documents-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DocumentsPickerComponent }]
})
export class DocumentsPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: string[];
}
