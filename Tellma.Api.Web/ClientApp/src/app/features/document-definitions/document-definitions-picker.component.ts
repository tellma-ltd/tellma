import { Component } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-document-definitions-picker',
  templateUrl: './document-definitions-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DocumentDefinitionsPickerComponent }]
})
export class DocumentDefinitionsPickerComponent extends PickerBaseComponent {

}
