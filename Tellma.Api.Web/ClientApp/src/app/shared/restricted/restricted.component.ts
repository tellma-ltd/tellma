import { Component, OnInit, Input, HostBinding } from '@angular/core';

@Component({
  selector: 't-restricted',
  templateUrl: './restricted.component.html'
})
export class RestrictedComponent {

  @HostBinding('class.d-block')
  w100 = true;

  @Input()
  metadata: number;
}
