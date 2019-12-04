import { Component, Input, HostBinding, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { DetailsPickerComponent } from '~/app/shared/details-picker/details-picker.component';
import { metadata_Agent } from '~/app/data/entities/agent';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { GENERIC } from '~/app/data/entities/base/constants';

@Component({
  selector: 'b-agents-picker',
  templateUrl: './agents-picker.component.html',
  styles: [],
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: AgentsPickerComponent }]
})
export class AgentsPickerComponent implements ControlValueAccessor {

  @Input()
  definitionIds: string[];

  @Input()
  filter: string;

  @HostBinding('class.w-100')
  w100 = true;

  @ViewChild(DetailsPickerComponent, { static: true })
  picker: DetailsPickerComponent;

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
  }

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

  // public getDefinitionIds(): string[] {
  //   return (!!this.definitionIds && this.definitionIds.length > 0) ? this.definitionIds :
  //     metadata_Agent(this.workspace.current, this.translate, GENERIC).definitionIds;
  // }
}
