import { Component, ViewChild, ElementRef, Input, HostBinding, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 't-multiline-editor',
  templateUrl: './multiline-editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: MultilineEditorComponent }]
})
export class MultilineEditorComponent implements ControlValueAccessor {

  // A simple text editor, instead of using input directly in all the screens, this allows
  // us to change the bahvior of all inputs in the application since they all use this control

  @Input()
  placeholder = '';

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  @ViewChild('input', { static: true })
  input: ElementRef;

  public onChangeFn: (val: any) => void = _ => { };
  public onTouchedFn: () => void = () => { };

  public focus(): void {
    if (this.input.nativeElement) {
      this.input.nativeElement.focus();
    }
  }

  public select(): void {
    if (this.input.nativeElement) {
      this.input.nativeElement.select();
    }
  }

  writeValue(v: any): void {

    v = v || '';
    this.input.nativeElement.value = v; // Format
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouchedFn = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.input.nativeElement.disabled = isDisabled;
  }


  onChange(val: any) {
    if (!val) {
      this.onChangeFn(undefined);
    } else {
      this.onChangeFn(val);
    }
  }
}
