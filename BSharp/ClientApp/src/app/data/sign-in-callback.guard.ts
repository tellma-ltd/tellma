import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class SignInCallbackGuard implements CanActivate {

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

        console.error(err);
        this.router.navigateByUrl(errorUrl);

        return of(false);
      })
    );
  }
}
