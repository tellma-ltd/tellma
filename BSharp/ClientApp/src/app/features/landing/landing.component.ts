import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { AuthService } from '~/app/data/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DOCUMENT } from '@angular/platform-browser';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';

@Component({
  selector: 'b-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent implements OnInit, OnDestroy {

  public error: string = null;

  constructor(private auth: AuthService, private route: ActivatedRoute, private router: Router,
    private workspace: WorkspaceService, private progress: ProgressOverlayService) { }

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

  ngOnDestroy() {
  }

  get showError(): boolean {
    return !!this.error;
  }

  get isAuthenticated(): boolean {
    return this.auth.isAuthenticated;
  }

  public onSignIn() {
    // show rotator
    this.progress.startAsyncOperation('sign_in', 'RedirectingToSignIn');

    // start the OIDC dance with identity server
    this.auth.initImplicitFlow('/root/companies');
  }

  get showSignIn(): boolean {
    return !this.auth.isAuthenticated;
  }

  public onGoToApp() {
    this.router.navigate(['/']);
  }

  get showGoToApp(): boolean {
    return this.auth.isAuthenticated;
  }

  get flip(): string {
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  get year(): number {
    return new Date().getFullYear();
  }
}
