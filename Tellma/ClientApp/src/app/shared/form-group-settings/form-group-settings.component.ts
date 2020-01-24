import { Component, OnInit, Input } from '@angular/core';
import { FormGroupBaseComponent } from '../form-group-base/form-group-base.component';

@Component({
  selector: 'b-form-group-settings',
  templateUrl: './form-group-settings.component.html',
  styles: []
})
export class FormGroupSettingsComponent extends FormGroupBaseComponent {

  @Input()
  description: string;
}
