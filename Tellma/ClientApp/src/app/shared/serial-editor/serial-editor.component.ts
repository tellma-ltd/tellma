import { Component, ViewChild, ElementRef, Input, HostBinding, OnChanges, SimpleChanges } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { serialNumber } from '~/app/data/entities/document';

@Component({
  selector: 't-serial-editor',
  templateUrl: './serial-editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: SerialEditorComponent }]
})
export class SerialEditorComponent implements ControlValueAccessor, OnChanges {

  @Input()
  codeWidth = 4;

  @Input()
  prefix = '';

  @ViewChild('input', { static: true })
  input: ElementRef;

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  public isDisabled = false;
  public onChange: (val: any) => void = _ => { };
  public onTouched: () => void = () => { };
  public onValidatorChange: () => void = () => { };

  writeValue(v: any): void {

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

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.codeWidth && !changes.codeWidth.isFirstChange()) {
      this.input.nativeElement.value = this.format(this.input.nativeElement.value);
    }
  }


  ///////////////// UI Binding

  public get zeros(): string {
    return serialNumber(0, '', this.codeWidth);
  }

  private format(value: number | string): string {
    // This creates the part after the prefix
    if (value === null || value === undefined || value === '') {
      return '';
    }

    const serial = +value.toString();
    return serialNumber(serial, '', this.codeWidth);
  }

  private parse(value: string): number {

    if (value === null || value === undefined || value === '') {
      return undefined;
    }

    const serial = +value;
    return isNaN(serial) ? undefined : serial;
  }

  ////////////////// Behavior
  onBlur(value: any) {

    // signal Angular forms, that the element was touched
    this.onTouched();

    // always format the input on blur
    this.input.nativeElement.value = this.format(this.parse(value));
  }
}
