import { Component, Input } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { PickerBaseComponent } from '~/app/shared/picker-base/picker-base.component';

@Component({
  selector: 't-resources-picker',
  templateUrl: './resources-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ResourcesPickerComponent }]
})
export class ResourcesPickerComponent extends PickerBaseComponent {

  @Input()
  definitionIds: number[];

  // writeValue(obj: any): void {
  //   this.picker.writeValue(obj);
  //   console.log(obj);
  // }
  // registerOnChange(fn: any): void {
  //   this.picker.registerOnChange(fn);
  // }
  // registerOnTouched(fn: any): void {
  //   this.picker.registerOnTouched(fn);
  // }
  // setDisabledState?(isDisabled: boolean): void {
  //   this.picker.setDisabledState(isDisabled);
  // }

}
