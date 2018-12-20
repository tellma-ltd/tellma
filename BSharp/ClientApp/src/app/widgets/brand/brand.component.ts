import { Component, OnInit, Input, HostBinding } from '@angular/core';

@Component({
  selector: 'b-brand',
  templateUrl: './brand.component.html',
  styleUrls: ['./brand.component.css']
})
export class BrandComponent {

  @Input()
  public scale = 1;

  @HostBinding('class.navbar-brand')
  navbarBrand = true;
}
