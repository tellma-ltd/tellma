import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'b-warning-message',
  template: `<div class="border p-2 alert-warning">
              <fa-icon icon="exclamation-triangle"></fa-icon>&nbsp;&nbsp;<ng-content></ng-content>
            </div>`
})
export class WarningMessageComponent {

}
