import { Component, Input, ContentChild, AfterContentInit, OnDestroy } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NgControl } from '@angular/forms';

@Component({
  selector: 'b-form-group',
  templateUrl: './form-group.component.html',
})
export class FormGroupComponent {

  // this component wraps a component (typically a form field) and optionally adds a label to it
  // and also optionally displays an error icon showing the errors on that form field, it can also display
  // server errors and correctly hides the server errors as soon as the user makes a change on the field

  @Input()
  label: string;

  @Input()
  description: string;

  @Input()
  serverErrors: string[];

  @ContentChild(NgControl, { static : false })
  control: NgControl;

  constructor(private workspace: WorkspaceService) { }

  get showLabel(): boolean {
    return !!this.label;
  }

  get showDescription(): boolean {
    return !!this.description;
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

      const result: string[] = [];
      const errors = this.control.errors;
      if (!!errors) {
        if (errors.required) {
          result.push('RequiredField');
        }

        if (errors.ngbDate) {
          result.push('InvalidDate');
        }

        if (errors.email) {
          result.push('InvalidEmail');
        }
      }

      return result;
    }
  }

  get areServerErrors(): boolean {
    return !!this.serverErrors && !!this.serverErrors.length;
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  get popoverPlacement(): string {
    return this.isRtl ? 'bottom-left' : 'bottom-right';
  }

}
