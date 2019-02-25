import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '~/app/data/auth.service';

@Component({
  selector: 'b-companies',
  templateUrl: './companies.component.html',
  styleUrls: ['./companies.component.scss']
})
export class CompaniesComponent implements OnInit {

  constructor(public router: Router, public auth: AuthService) { }

  ngOnInit() {
  }

  onRefresh() {
    this.auth.refreshSilently().subscribe();
  }
}
