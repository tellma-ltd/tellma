import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'b-spinner',
  templateUrl: './spinner.component.html',
})
export class SpinnerComponent {

  @Input()
  public scale = 1;
}
