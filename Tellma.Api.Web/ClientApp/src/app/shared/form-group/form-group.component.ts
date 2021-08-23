import { Component, Input, TemplateRef } from '@angular/core';
import { FormGroupBaseComponent } from '../form-group-base/form-group-base.component';


@Component({
  selector: 't-form-group',
  templateUrl: './form-group.component.html',
})
export class FormGroupComponent extends FormGroupBaseComponent {

  @Input()
  labelContextMenu: TemplateRef<any>;

  @Input()
  labelContext: any;

  @Input()
  disableLabelMenu: boolean;
}
