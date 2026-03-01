import { Component, Input } from '@angular/core';

@Component({standalone: false, 
  selector: 't-info-message',
  templateUrl: './info-message.component.html'
})
export class InfoMessageComponent {
  
  @Input()
  height: number;
}
