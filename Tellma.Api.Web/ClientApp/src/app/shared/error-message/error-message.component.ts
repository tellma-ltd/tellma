import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 't-error-message',
  templateUrl: './error-message.component.html',
})
export class ErrorMessageComponent {

  @Input()
  dismissable: boolean;

  @Output()
  dismiss = new EventEmitter<void>();

  public onDismiss() {
    this.dismiss.emit();
  }
}
