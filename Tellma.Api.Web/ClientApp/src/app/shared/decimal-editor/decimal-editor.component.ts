import { Component, ViewChild, ElementRef, Input, HostBinding, OnChanges, SimpleChanges } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { isSpecified } from '~/app/data/util';

@Component({
  selector: 't-decimal-editor',
  templateUrl: './decimal-editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DecimalEditorComponent }]
})
export class DecimalEditorComponent implements ControlValueAccessor, OnChanges {

  @Input()
  placeholder = '';

  @Input()
  textAlignment: 'left' | 'right' = null;

  @Input()
  maxDecimalPlaces: number = null;

  @Input()
  minDecimalPlaces: number = null;

  @Input()
  noSeparator = false;

  @Input()
  theme: 'light' | 'dark' = 'light';

  @Input()
  isPercentage = false;

  @ViewChild('input', { static: true })
  input: ElementRef;

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  public isDisabled = false;
  public onChange: (val: any) => void = _ => { };
  public onTouched: () => void = () => { };
  public onValidatorChange: () => void = () => { };

  writeValue(num: number): void {
    num = this.isPercentage ? (num * 100) : num;
    const s = this.format(num);

    this.input.nativeElement.value = s;
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChange = (s) => {
      let num = this.parse(s);
      num = this.isPercentage ? (num / 100) : num;
      fn(num);
    };
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  ngOnChanges(changes: SimpleChanges): void {
    // to update the formatting if any of the inputs change
    if ((!!changes.maxDecimalPlaces && !changes.maxDecimalPlaces.isFirstChange())
      || (!!changes.minDecimalPlaces && !changes.minDecimalPlaces.isFirstChange())) {
      const parsed = this.parse(this.input.nativeElement.value);
      this.input.nativeElement.value = this.format(parsed);
    }
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

  private isBracketed(val: string): boolean {
    return val.startsWith('(') && val.endsWith(')');
  }

  private removeBrackets(val: string): string {
    return val.substring(1, val.length - 1);
  }

  /**
   * Takes a number and formats it with a decimal point and a thousands separator and a percent sign
   */
  private format(value: number): string {
    if (!value && value !== 0) {
      return '';
    }

    // if (this.isPercentage) {
    //   value *= 100;
    // }

    const isNegative = value < 0;
    const valueString = (Math.abs(value) || '0').toString();

    let [integer, fraction = ''] = valueString.split(this.decimalSeparator);

    // Trim the fraction part as per min and max decimal places
    let min = this.minDecimalPlaces;
    if (min === null) {
      min = fraction.length;
    }

    let max = Math.max(this.maxDecimalPlaces, min);
    if (max === null) {
      max = fraction.length;
    }

    let fractionSize = fraction.length;
    fractionSize = Math.max(min, fractionSize);
    fractionSize = Math.min(max, fractionSize);

    fraction = fractionSize > 0
      ? this.decimalSeparator + (fraction + this.padding).substring(0, fractionSize)
      : '';

    // Insert the thousands separator in the integer part
    if (!this.noSeparator) {
      integer = integer.replace(/\B(?=(\d{3})+(?!\d))/g, this.thousandsSeparator);
    }

    // Combine for final result
    let result = integer + fraction;

    // Add percent sign if specified
    if (this.isPercentage) {
      result = result + '%';
    }

    // Add brackets if negative number
    if (isNegative) {
      result = `(${result})`;
    }
    return result;
  }

  /**
   * Reverses the effect of 'format' method above, and returns the original number
   */
  private parse(value: string): number {

    if (value === null || value === undefined || value === '') {
      return undefined;
    }

    value = (value || '').trim();
    const isNegativeBrackets = this.isBracketed(value);
    if (isNegativeBrackets) {
      value = this.removeBrackets(value).trim(); // remove the brackets
    }

    if (this.isPercentage) {
      while (value.endsWith('%')) {
        value = value.slice(0, -1);
      }
    }

    let [integer, fraction = ''] = value.split(this.decimalSeparator);

    integer = integer.replace(new RegExp(this.thousandsSeparator, 'g'), '');

    let fractionSize = this.maxDecimalPlaces;
    if (fractionSize === null) {
      fractionSize = fraction.length;
    }

    fraction = parseInt(fraction, 10) > 0 && fractionSize > 0
      ? this.decimalSeparator + (fraction + this.padding).substring(0, fractionSize)
      : '';

    const stringResult = integer + fraction;
    const result = !!stringResult ? +stringResult : 0;

    if (!result && result !== 0) {
      return undefined; // Standardize output when input is not valid
    }

    return isNegativeBrackets ? -result : result;
  }

  ////////////////// Behavior
  onBlur(value: any) {

    // signal Angular forms, that the element was touched
    this.onTouched();

    // always format the input on blur
    this.input.nativeElement.value = this.format(this.parse(value));
  }

  public get isDark(): boolean {
    return this.theme === 'dark';
  }
}
