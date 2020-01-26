import { Component, OnInit, Input, HostBinding } from '@angular/core';

@Component({
  selector: 't-brand',
  templateUrl: './brand.component.html'
})
export class BrandComponent {

  @Input()
  public scale = 1;
}
