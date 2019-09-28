import { Component, ViewChild, Input, HostBinding } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DetailsPickerComponent } from '~/app/shared/details-picker/details-picker.component';

@Component({
  selector: 'b-lookups-picker',
  templateUrl: './lookups-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: LookupsPickerComponent }]
})
export class LookupsPickerComponent implements ControlValueAccessor {

  @Input()
  definitionId: string;

  @Input()
  filter: string;

  @HostBinding('class.w-100')
  w100 = true;

  @ViewChild(DetailsPickerComponent, { static: true })
  picker: DetailsPickerComponent;

  writeValue(obj: any): void {
    this.picker.writeValue(obj);
  }
  registerOnChange(fn: any): void {
    this.picker.registerOnChange(fn);
  }
  registerOnTouched(fn: any): void {
    this.picker.registerOnTouched(fn);
  }
  setDisabledState?(isDisabled: boolean): void {
    this.picker.setDisabledState(isDisabled);
  }
}
