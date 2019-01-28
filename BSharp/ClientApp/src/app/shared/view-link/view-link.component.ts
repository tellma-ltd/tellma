import { Component, OnInit, HostBinding, Input } from '@angular/core';

@Component({
  selector: 'b-view-link',
  templateUrl: './view-link.component.html',
  styleUrls: ['./view-link.component.scss']
})
export class ViewLinkComponent implements OnInit {

  @Input()
  link: string;

  @Input()
  itemId: string | number;

  @HostBinding('class.w-100')
  w100 = true;

  constructor() { }

  ngOnInit() {
  }

}
