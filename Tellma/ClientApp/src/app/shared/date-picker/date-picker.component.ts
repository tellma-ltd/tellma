
import { Component, HostBinding, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor, Validator, ValidationErrors, AbstractControl, NG_VALIDATORS } from '@angular/forms';
import { NgbInputDatepicker } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 't-date-picker',
  templateUrl: './date-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DatePickerComponent },
  { provide: NG_VALIDATORS, useExisting: DatePickerComponent, multi: true }]
})
export class DatePickerComponent implements ControlValueAccessor, Validator {

  @ViewChild('d', { static: true })
  picker: NgbInputDatepicker;

  constructor() { }

  @HostBinding('class.w-100')
  w100 = true;

  public isDisabled = false;

  ///////////////// Implementation of ControlValueAccessor
  writeValue(v: any): void {
    this.picker.writeValue(v); // Format
  }

  registerOnChange(fn: (val: any) => void): void {
    this.picker.registerOnChange((v) => {
      if (!v) {
        fn(undefined);
      } else {
        fn(v);
      }
    });
  }

  registerOnTouched(fn: any): void {
    this.picker.registerOnTouched(fn);
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
    this.picker.setDisabledState(isDisabled);
  }

  registerOnValidatorChange?(fn: () => void): void {
    this.picker.registerOnValidatorChange(fn);
  }

  public validate(control: AbstractControl): ValidationErrors | null {
    let errors = this.picker.validate(control);

    // this is only since the default validation does not flag some invalid strings as invalid
    // for example: "1980" is marked valid by the default validation
    if (!!errors) {
      return errors;
    } else {
      if (!!control.value) {
        // the correct format is 'yyyy-mm-dd'
        const sections = control.value.split('-');
        if (sections.length < 2 || sections.some((e: any) => isNaN(parseInt(e, 10)))) {
          errors = {};
          errors.ngbDate = control.value;
        }
      }
    }
    return errors;
  }
}

