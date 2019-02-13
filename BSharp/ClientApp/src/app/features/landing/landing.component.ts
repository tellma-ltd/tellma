import { Component, OnInit } from '@angular/core';
import { AuthService } from '~/app/data/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent implements OnInit {

  public error: string = null;

  constructor(private auth: AuthService, private route: ActivatedRoute) { }

  ngOnInit() {

    this.route.queryParamMap.subscribe(e => {
      const errorId = +e.get('error');
      switch (errorId) {
        case 401:
          this.error = `Error_LoginSessionExpired`;
          break;
        case 422:
          this.error = `Error_UnableToValidateYourCredentials`;
          break;
        default:
          this.error = null;
          break;
      }
    });
  }

  get showError(): boolean {
    return !!this.error;
  }

  get isAuthenticated(): boolean {
    return this.auth.isAuthenticated;
  }
}
