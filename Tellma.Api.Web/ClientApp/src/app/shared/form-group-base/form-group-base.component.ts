import { Component, Input, ContentChild } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NgControl } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';

/**
 * Determines whether the input needs to be highlighted in red color indicating that it's invalid
 *
 * @param control The NgControl bound to the input
 * @param serverErrors The server errors associated with this input
 */
export function highlightInvalid(control: NgControl, serverErrors: string[]): boolean {
  return (!!control && control.invalid && control.touched) || areServerErrors(serverErrors);
}

/**
 * Returns a list of functions that return error messages
 *
 * @param control The NgControl bound to the input
 * @param serverErrors The server errors associated with this input
 */
export function validationErrors(control: NgControl, serverErrors: string[], trx: TranslateService): (() => string)[] {

  // IF there are server errors, hide the client errors
  if (areServerErrors(serverErrors)) {
    return serverErrors.map(e => () => e);

  } else if (!!control) {

    const result: string[] = [];
    const errors = control.errors;
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

    return result.map(e => (() => trx.instant(e)));
  }
}

export function areServerErrors(serverErrors: string[]) {
  return !!serverErrors && serverErrors.length > 0;
}

@Component({
  selector: 't-form-group-base',
  template: ''
})
export class FormGroupBaseComponent {
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

  constructor(private workspace: WorkspaceService, private translate: TranslateService) { }

  get showLabel(): boolean {
    return !!this.label;
  }

  get showDescription(): boolean {
    return !!this.description;
  }

  get invalid(): boolean {
    return highlightInvalid(this.control, this.serverErrors);
  }

  get errors(): (() => string)[] {

    return validationErrors(this.control, this.serverErrors, this.translate);
  }

  get areServerErrors(): boolean {
    return areServerErrors(this.serverErrors);
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  get popoverPlacement(): string {
    // return this.isRtl ? 'bottom-right' : 'bottom-left';
    return 'bottom';
  }

}
