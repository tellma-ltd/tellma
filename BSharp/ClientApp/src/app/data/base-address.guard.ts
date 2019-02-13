import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class BaseAddressGuard implements CanActivate {

/*
  this guard is responsible the base address (e.g. https://www.bsharp.online/)
  it simply checks if the user is authenticated and redirectes him/her as follows:
  - authenticated: application
  - not authenticated: welcome page
*/

  constructor(private auth: AuthService, private router: Router) { }

  canActivate(): Observable<boolean> {

    return this.auth.isSignedIn$.pipe(
      map(isAuthenticated => {
        const url = isAuthenticated ? '/companies' : '/welcome';
        this.router.navigateByUrl(url);

        return false;
      }),
      catchError(_ => {
        this.router.navigateByUrl('/welcome');
        return of(false);
      })
    );
  }
}
