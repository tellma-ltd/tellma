import { Component, Input } from '@angular/core';
import { FormGroupBaseComponent } from '../form-group-base/form-group-base.component';

@Component({
  selector: 't-form-group-settings',
  templateUrl: './form-group-settings.component.html',
  styles: []
})
export class FormGroupSettingsComponent extends FormGroupBaseComponent {

  @Input()
  description: string;
}
