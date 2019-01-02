import { Component, OnInit, Input, ContentChild } from '@angular/core';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { NgControl } from '@angular/forms';

@Component({
  selector: 'b-form-group',
  templateUrl: './form-group.component.html',
  styleUrls: ['./form-group.component.css']
})
export class FormGroupComponent implements OnInit {

  @Input()
  label: string;

  @Input()
  serverErrors: string[];

  @ContentChild(NgControl)
  control: NgControl

  constructor(private workspace: WorkspaceService) { }

  ngOnInit() {
  }

  get showLabel(): boolean {
    return !!this.label;
  }

  get invalid(): boolean {
    return (!!this.control && !!this.control.invalid && this.control.touched)
      || (!!this.serverErrors && !!this.serverErrors.length);
  }

  get errors(): string[] {

    // IF there are server errors, hide the client errors
    if (this.areServerErrors) {
      return this.serverErrors;

    } else if (!!this.control) {

      let result: string[] = [];
      const errors = this.control.errors;
      if (!!errors) {
        if (errors.required) {
          result.push('RequiredField');
        }
      }

      return result;
    }
  }

  get areServerErrors(): boolean {
    return !!this.serverErrors && !!this.serverErrors.length;
  }

  get popoverPlacement(): string {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }
}
