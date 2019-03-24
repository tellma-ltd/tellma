import { Component, OnInit, Input, HostBinding } from '@angular/core';

@Component({
  selector: 'b-restricted',
  templateUrl: './restricted.component.html',
  styleUrls: ['./restricted.component.scss']
})
export class RestrictedComponent implements OnInit {

  @HostBinding('class.d-block')
  w100 = true;

  @Input()
  metadata: number;

  constructor() { }

  ngOnInit() {
  }

}
