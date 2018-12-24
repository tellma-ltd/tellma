import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'b-error-message',
  template: `<div class="border p-2 alert-danger">
              <fa-icon icon="exclamation-triangle"></fa-icon>&nbsp;&nbsp;<ng-content></ng-content>
            </div>`
})
export class ErrorMessageComponent {

}
