import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 't-admin-page-not-found',
  templateUrl: './admin-page-not-found.component.html',
  styles: []
})
export class AdminPageNotFoundComponent {

  constructor(private router: Router) { }

  public onHome() {
    this.router.navigate(['admin', 'console', 'main-menu']);
  }
}
