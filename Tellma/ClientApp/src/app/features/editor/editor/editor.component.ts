import { Component, HostBinding, Input, ViewChild } from '@angular/core';
import { ControlValueAccessor, NgControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { getChoices, NavigationPropDescriptor, PropVisualDescriptor } from '~/app/data/entities/base/metadata';
import { IdService } from '~/app/data/id.service';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-editor',
  templateUrl: './editor.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: EditorComponent }]
})
export class EditorComponent implements ControlValueAccessor {

  constructor(private id: IdService) { }

  @HostBinding('class.w-100')
  w100 = true;

  @Input()
  public desc: PropVisualDescriptor;

  @ViewChild(NgControl, { static: false })
  public get control(): NgControl {
    return this._control;
  }

  public set control(v: NgControl) {
    if (this._control !== v) {
      this._control = v;
      this.initializeNewControl(v);
    }
  }

  private definitionIdForArray: number;
  private definitionIdArray: number[] = [];

  private _control: NgControl;
  private _checkboxId: string;

  ///////////// ControlValueAccessor

  public value: any;
  private isDisabled = false;
  private onChangeFn: (val: any) => void = _ => { };
  private onTouchedFn: () => void = () => { };

  private initializeNewControl(ctrl: NgControl) {
    if (!!ctrl && ctrl.valueAccessor) {
      console.log('Initializing New Component', this.desc);
      const va = ctrl.valueAccessor;
      va.writeValue(this.value);
      va.registerOnChange(this.onChangeFn);
      va.registerOnTouched(this.onTouchedFn);
      if (va.setDisabledState) {
        va.setDisabledState(this.isDisabled);
      }
    }
  }

  writeValue(obj: any): void {
    // For future controls
    this.value = obj;

    // For current control
    const ctrl = this.control;
    if (ctrl && ctrl.valueAccessor) {
      console.log('Writing Value', this.value);
      ctrl.valueAccessor.writeValue(this.value);
    }
  }

  registerOnChange(fn: any): void {
    this.onChangeFn = fn;

    // For current control
    const ctrl = this.control;
    if (ctrl && ctrl.valueAccessor) {
      console.log('Registering onChange', this.desc);
      ctrl.valueAccessor.registerOnChange(this.onChangeFn);
    }
  }

  registerOnTouched(fn: any): void {
    // For future controls
    this.onTouchedFn = fn;

    // For current control
    const ctrl = this.control;
    if (ctrl && ctrl.valueAccessor) {
      console.log('Registering onTouch', this.desc);
      ctrl.valueAccessor.registerOnTouched(this.onTouchedFn);
    }
  }

  setDisabledState?(isDisabled: boolean): void {
    // For future controls
    this.isDisabled = isDisabled;

    // For current control
    const ctrl = this.control;
    if (ctrl && ctrl.valueAccessor && ctrl.valueAccessor.setDisabledState) {
      console.log('Setting disabled state', this.desc);
      ctrl.valueAccessor.setDisabledState(this.isDisabled);
    }
  }

  ///////////// UI Binding

  public get checkboxId(): string {

    if (!this._checkboxId) {
      this._checkboxId = this.id.getId() + '';
    }

    return this._checkboxId;
  }

  public get minDecimalPlaces(): number {
    if (this.desc.control === 'number' || this.desc.control === 'percent') {
      return this.desc.minDecimalPlaces;
    }

    console.error(`Editor error: requesting minDecimalPlaces from a ${this.desc.control}`);
    return 0;
  }

  public get maxDecimalPlaces(): number {
    if (this.desc.control === 'number' || this.desc.control === 'percent') {
      return this.desc.maxDecimalPlaces;
    }

    console.error(`Editor error: requesting maxDecimalPlaces from a ${this.desc.control}`);
    return 0;
  }

  public get prefix(): string {
    if (this.desc.control === 'serial') {
      return this.desc.prefix;
    }

    console.error(`Editor error: requesting prefix from a ${this.desc.control}`);
    return '';
  }

  public get codeWidth(): number {
    if (this.desc.control === 'serial') {
      return this.desc.codeWidth;
    }

    console.error(`Editor error: requesting codeWidth from a ${this.desc.control}`);
    return 4;
  }

  public get choices(): SelectorChoice[] {
    if (this.desc.control === 'choice') {
      return getChoices(this.desc);
    }

    console.error(`Editor error: requesting choices from a ${this.desc.control}`);
    return [];
  }

  public get filter(): string {
    const desc = this.desc as NavigationPropDescriptor;
    return desc.filter;
  }

  public get definitionId(): number {
    const desc = this.desc as NavigationPropDescriptor;
    return desc.definitionId;
  }

  public get definitionIds(): number[] {
    const desc = this.desc as NavigationPropDescriptor;
    const defId = desc.definitionId;
    if (this.definitionIdForArray !== defId) {
      this.definitionIdForArray = defId;
      if (!defId) {
        this.definitionIdArray = [];
      } else {
        this.definitionIdArray = [defId];
      }
    }

    return this.definitionIdArray;
  }
}
