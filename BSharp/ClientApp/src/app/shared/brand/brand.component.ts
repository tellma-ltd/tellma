import { Component, OnInit, Input, HostBinding } from '@angular/core';

@Component({
  selector: 'b-brand',
  templateUrl: './brand.component.html'
})
export class BrandComponent {

  @Input()
  public scale = 1;
}
