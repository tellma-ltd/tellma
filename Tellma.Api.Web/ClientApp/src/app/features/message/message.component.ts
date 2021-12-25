import { Component, Input, OnInit } from '@angular/core';
import { MessagePreview } from '~/app/data/dto/message-command-preview';

@Component({
  selector: 't-message',
  templateUrl: './message.component.html',
  styles: [
  ]
})
export class MessageComponent implements OnInit {

  @Input()
  message: MessagePreview;

  constructor() { }

  ngOnInit(): void {
  }

}
