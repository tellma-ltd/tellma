
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivateChild, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService, AuthEvent } from './auth.service';
import { tap, catchError } from 'rxjs/operators';
import { CleanerService } from './cleaner.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(private auth: AuthService, private cleaner: CleanerService, private router: Router) {
    this.auth.setupAutomaticSilentRefresh();
    this.handleAuthEvents();
  }

  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const returnUrl = state.url;
    return this.can(returnUrl);
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const returnUrl = state.url;
    return this.can(returnUrl);
  }

  private can(returnUrl: string): Observable<boolean> {
    return this.auth.isSignedIn$.pipe(
      tap(isSignedIn => {
        // IF the user is not signed in -> delete app state and initiate implicit flow
        if (!isSignedIn) {
          this.cleaner.cleanState();
          this.auth.initImplicitFlow(returnUrl);
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
  // singleton too
  handleAuthEvents(): void {
    this.auth.events$.subscribe(e => {
      switch (e) {
        case AuthEvent.SignedOutFromAuthority:
          this.goToLandingPage();
          this.cleaner.cleanState();
          break;
        case AuthEvent.SignedInAsDifferentUser:
          // cleaning the state before navigating is necessary here, to force loading the new user's token
          // it will throw log errors for all the bindings on display, but this is a rare marginal event anyways
          this.cleaner.cleanState();
          this.router.navigateByUrl('/companies');
          break;
        case AuthEvent.StorageIsCleared:
          this.goToLandingPage();
          this.cleaner.cleanWorkspace();
          break;
      }
    });
  }

  goToLandingPage(): void {
    this.router.navigateByUrl('/welcome');
  }
}

