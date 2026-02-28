import { Component, OnInit, Input } from '@angular/core';

@Component({standalone: false, 
  selector: 't-spinner',
  templateUrl: './spinner.component.html',
})
export class SpinnerComponent {

  @Input()
  public scale = 1;
}
