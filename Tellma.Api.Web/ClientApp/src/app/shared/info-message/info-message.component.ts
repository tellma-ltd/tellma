import { Component, Input } from '@angular/core';

@Component({
  selector: 't-info-message',
  templateUrl: './info-message.component.html'
})
export class InfoMessageComponent {
  
  @Input()
  height: number;
}
