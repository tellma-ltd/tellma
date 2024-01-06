
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService, AuthEvent } from './auth.service';
import { tap, catchError } from 'rxjs/operators';
import { CleanerService } from './cleaner.service';
import { ProgressOverlayService } from './progress-overlay.service';
import { ServerNotificationsService } from './server-notifications.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard {
  constructor(
    private auth: AuthService, private cleaner: CleanerService,
    private router: Router, private progress: ProgressOverlayService,
    private serverNotifications: ServerNotificationsService) {
    this.auth.setupAutomaticSilentRefresh();
    this.handleAuthEvents();
  }

  canActivateChild(state: RouterStateSnapshot): Observable<boolean> {
    const returnUrl = state.url;
    return this.can(returnUrl);
  }

  canActivate(state: RouterStateSnapshot): Observable<boolean> {
    const returnUrl = state.url;
    return this.can(returnUrl);
  }

  private can(returnUrl: string): Observable<boolean> {
    return this.auth.isSignedIn$.pipe(
      tap(isSignedIn => {
        // IF the user is not signed in -> delete app state and initiate implicit flow
        if (!isSignedIn) {
          this.cleaner.cleanState();
          this.progress.startAsyncOperation('sign_in', 'RedirectingToSignIn');
          this.serverNotifications.signout();
          this.auth.initImplicitFlow(returnUrl);
        } else {
          this.serverNotifications.signin();
        }
      }), catchError(err => {
        console.error(err);
        this.router.navigateByUrl('welcome?error=422');
        return of(false);
      })
    );
  }

  // this method listens to auth events and does the needful,
  // we put it in the auth guard since it won't be needed if
  // the auth guard is not instantiated, and the guard is a
  // singleton too just like auth.service
  handleAuthEvents(): void {
    this.auth.events$.subscribe(e => {
      switch (e) {
        case AuthEvent.SignedOutFromAuthority:
          this.goToLandingPage().then(_ => {
            this.cleaner.cleanState();
          });
          break;
        case AuthEvent.SignedInAsDifferentUser:
          // if a different user signed in we clean the state (including local storage)
          // otherwise the error will keep firing no matter how many times the user
          // refreshes the app then we automatically reload the app to fetch the new
          // token, clearing the storage will prompt other tabs to clean their state
          // and go to the landing page too
          this.cleaner.cleanState();
          document.location.reload();
          break;

        case AuthEvent.StorageIsCleared:
          this.goToLandingPage().then(_ => {
            this.cleaner.cleanWorkspace();
            this.cleaner.cleanServerNotifications();
          });
          break;
      }
    });
  }

  goToLandingPage(): Promise<boolean> {
    return this.router.navigateByUrl('/root/welcome');
  }
}
