import { Component, Input, ElementRef, ViewChild, HostBinding } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { isNumber } from 'util';

export interface SelectorChoice {
  /**
   * A function that returns the display string of the choice
   */
  name: () => string;

  /**
   * The value of the choice
   */
  value: any;
}

@Component({
  selector: 't-selector',
  templateUrl: './selector.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: SelectorComponent }]
})
export class SelectorComponent implements ControlValueAccessor {

  // Selector from a list of choices, instead of using selector directly in all the screens, this allows
  // us to change the bahvior of all inputs in the application since they all use this control

  @Input()
  choices: SelectorChoice[] = [];

  @HostBinding('class.w-100')
  w100 = true;

  ///////////////// Implementation of ControlValueAccessor
  @ViewChild('selector', { static: true })
  selector: ElementRef;

  public isDisabled = false;
  public onChange: (val: any) => void = _ => { };
  public onTouched: () => void = () => { };
  public onValidatorChange: () => void = () => { };

  writeValue(v: any): void {
    if (this.selector.nativeElement.value !== v) {
      this.selector.nativeElement.value = v; // Format
    }
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  public trackByFn(_: number, item: SelectorChoice) {
    // Might cause problems if we supply a new list with an item having the same value but a different name
    return item.value;
  }

  public onValueChanged(value: any) {
    if (value === null || value === '') {
      value = undefined;
    }

    if (value !== undefined && !!this.choices && !!this.choices[0] && typeof this.choices[0].value === 'number') {
      this.onChange(+value);
    } else {
      this.onChange(value);
    }

    this.onTouched();
  }
}
