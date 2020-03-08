import { Component, OnInit, ViewChild, ElementRef, Input, HostBinding, AfterViewInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 't-text-editor',
  templateUrl: './text-editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: TextEditorComponent }]
})
export class TextEditorComponent implements ControlValueAccessor, AfterViewInit {

  // A simple text editor, instead of using input directly in all the screens, this allows
  // us to change the bahvior of all inputs in the application since they all use this control

  @Input()
  placeholder = '';

  @Input()
  focusIf: boolean;

  @Input()
  type = 'text';

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  @ViewChild('input', { static: true })
  input: ElementRef;

  private triggerTouch = true;
  public isDisabled = false;
  public onChangeFn: (val: any) => void = _ => { };
  public onTouchedFn: () => void = () => { };
  public onValidatorChange: () => void = () => { };

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
    this.isDisabled = isDisabled;
  }

  onTouched() {
    if (this.triggerTouch) {
      this.onTouchedFn();
    } else {
      this.triggerTouch = true;
    }
  }

  onChange(val: any) {
    this.triggerTouch = true;
    if (!val) {
      this.onChangeFn(undefined);
    } else {
      this.onChangeFn(val);
    }
  }

  ///////////////// Implementation of AfterViewInit
  ngAfterViewInit() {
    if (this.focusIf && this.input) {
      this.input.nativeElement.focus();

      // when the field is auto-focused, don't trigger touch as
      // soon as the user moves the focus away the first time
      this.triggerTouch = false;
    }
  }

}
