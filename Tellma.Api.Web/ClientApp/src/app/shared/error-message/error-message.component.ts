import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({standalone: false, 
  selector: 't-error-message',
  templateUrl: './error-message.component.html',
})
export class ErrorMessageComponent {
  
  @Input()
  height: number;

  @Input()
  dismissable: boolean;

  @Output()
  dismiss = new EventEmitter<void>();

  public onDismiss() {
    this.dismiss.emit();
  }
}
