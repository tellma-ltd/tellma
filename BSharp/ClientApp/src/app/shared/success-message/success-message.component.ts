import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'b-success-message',
  template: `<div class="border p-2 alert-success">
              <fa-icon icon="check"></fa-icon>&nbsp;&nbsp;<ng-content></ng-content>
            </div>`
})
export class SuccessMessageComponent {

}
