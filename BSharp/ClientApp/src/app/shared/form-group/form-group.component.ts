import { Component, Input, ContentChild, AfterContentInit, OnDestroy } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NgControl } from '@angular/forms';
import { Subscription } from 'rxjs';

@Component({
  selector: 'b-form-group',
  templateUrl: './form-group.component.html',
})
export class FormGroupComponent implements OnDestroy {

  // this component wraps a component (typically a form field) and optionally adds a label to it
  // and also optionally displays an error icon shown the errors on that form field, it can also display
  // server errors and correctly hides the server errors as soon as the user makes a change on the field

  private _serverErrors: string[];
  private _control: NgControl;
  private controlValueChanges: Subscription;
  private touchedSinceServerErrors = false;

  @Input()
  label: string;

  @Input()
  set serverErrors(v: string[]) {
    if (this._serverErrors !== v) {
      // reset the value of 'touchedSinceServerErrors' whenever they change
      // this shows the server errors to the user until the field is modified
      this.touchedSinceServerErrors = false;
      this._serverErrors = v;
    }
  }

  get serverErrors(): string[] {
    return this._serverErrors;
  }

  @ContentChild(NgControl)
  set control(v: NgControl) {
    if (this._control !== v) {
      // unsubscribe from old NgControl
      if (!!this.controlValueChanges) {
        this.controlValueChanges.unsubscribe();
      }

      // set the new NgControl
      this._control = v;

      // subscribe to value changes on the new NgControl
      if (!!this._control) {
        this.controlValueChanges = this._control.valueChanges.subscribe(_ => {
          // hides away server errors when the user modifies the field
          this.touchedSinceServerErrors = true;
        });
      }
    }
  }

  get control(): NgControl {
    return this._control;
  }

  ngOnDestroy() {
    // clean up duty
    if (!!this.controlValueChanges) {
      this.controlValueChanges.unsubscribe();
    }
  }

  constructor(private workspace: WorkspaceService) { }

  get showLabel(): boolean {
    return !!this.label;
  }

  get invalid(): boolean {
    return (!!this.control && !!this.control.invalid && this.control.touched)
      || (!this.touchedSinceServerErrors && !!this.serverErrors && !!this.serverErrors.length);
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
