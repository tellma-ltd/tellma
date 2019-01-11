import { Component, ViewChild, ElementRef, Input, HostBinding } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'b-decimal-editor',
  templateUrl: './decimal-editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DecimalEditorComponent }]
})
export class DecimalEditorComponent implements ControlValueAccessor {

  @Input()
  textAlignment: 'left' | 'right' = null;

  @Input()
  decimalPlaces: number = null;

  @ViewChild('input')
  input: ElementRef;

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  public isDisabled = false;
  public onChange: (val: any) => void = _ => { };
  public onTouched: () => void = () => { };
  public onValidatorChange: () => void = () => { };

  writeValue(v: any): void {

    v = v || '';
    this.input.nativeElement.value = this.format(v); // Format
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChange = (val) => {
      fn(this.parse(val));
    };
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }


  ///////////////// Helper methods: Copied from the Internet and modified
  ///////////////// Credit goes to https://bit.ly/2BV9oy3

  private get padding() {
    return '00000000';
  }

  private get decimalSeparator() {
    // TODO Determine based on current culture
    return '.';
  }

  private get thousandsSeparator() {
    // TODO Determine based on current culture
    return ',';
  }

  private format(value: number | string): string {

    // Takes a number and formats it with a decimal point and a thousands separator
    let [integer, fraction = ''] = (value || '0').toString()
      .split(this.decimalSeparator);

    let fractionSize = this.decimalPlaces;
    if (fractionSize === null) {
      fractionSize = fraction.length;
    }

    fraction = fractionSize > 0
      ? this.decimalSeparator + (fraction + this.padding).substring(0, fractionSize)
      : '';

    integer = integer.replace(/\B(?=(\d{3})+(?!\d))/g, this.thousandsSeparator);

    return integer + fraction;
  }

  private parse(value: string): number {

    // Reverses the effect of 'format' method above, and returns the original number
    let [integer, fraction = ''] = (value || '').split(this.decimalSeparator);

    integer = integer.replace(new RegExp(this.thousandsSeparator, 'g'), '');

    let fractionSize = this.decimalPlaces;
    if (fractionSize === null) {
      fractionSize = fraction.length;
    }

    fraction = parseInt(fraction, 10) > 0 && fractionSize > 0
      ? this.decimalSeparator + (fraction + this.padding).substring(0, fractionSize)
      : '';

    const stringResult = integer + fraction;
    return !!stringResult ? +stringResult : 0;
  }

  ////////////////// Behavior
  onBlur(value) {

    // signal Angular forms, that the element was touched
    this.onTouched();

    // always format the input on blur
    this.input.nativeElement.value = this.format(this.parse(value));
  }
}
