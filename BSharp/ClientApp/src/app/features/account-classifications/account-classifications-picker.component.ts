import { Component, OnInit, Input, HostBinding, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DetailsPickerComponent } from '~/app/shared/details-picker/details-picker.component';

@Component({
  selector: 'b-account-classifications-picker',
  templateUrl: './account-classifications-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: AccountClassificationsPickerComponent }]
})
export class AccountClassificationsPickerComponent implements ControlValueAccessor {

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
