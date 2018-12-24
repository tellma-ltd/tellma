import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'b-spinner',
  template: `<fa-icon icon="spinner" [spin]="true" [style.font-size]="(scale * 100) + '%'"></fa-icon>`,
})
export class SpinnerComponent {

  @Input()
  public scale = 1;
}
