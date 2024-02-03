import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';
import { map, catchError } from 'rxjs/operators';
import { OAuthErrorEvent } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: 'root'
})
export class SignInCallbackGuard {

  // this guard parses the authentication tokens from the hash fragmentin the URL
  // and then redirects the user to the originally requested url

  constructor(private auth: AuthService, private router: Router) { }

  canActivate(): Observable<boolean> {

    const errorUrl = '/root/welcome?error=422';
    return this.auth.parseUrlToken().pipe(
      map(_ => {
        const isAuthenticated = this.auth.isAuthenticated;
        const url = isAuthenticated ? (this.auth.state || '') : errorUrl;
        this.router.navigateByUrl(url);

        return false;
      }),
      catchError(err => {

        if (err instanceof OAuthErrorEvent && err.type === 'invalid_nonce_in_state') {
          // Some users bookmark the sign in page itself (including the 1 time nonce in the URL)
          // Causing the nonce check to fail, we handle this here by attempting a fallback silent refresh
          return this.auth.refreshSilently().pipe(
            map(_ => {
              const isAuthenticated = this.auth.isAuthenticated;
              const url = isAuthenticated ? '' : errorUrl;
              this.router.navigateByUrl(url);

              return false;
            }),
            catchError(secondErr => {
              console.error(secondErr);
              this.router.navigateByUrl(errorUrl);
              return of(false);
            })
          );
        } else {
          console.error(err);
          this.router.navigateByUrl(errorUrl);
          return of(false);
        }
      })
    );
  }
}
