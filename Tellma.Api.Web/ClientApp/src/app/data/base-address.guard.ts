import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { AuthService } from './auth.service';
import { map, catchError } from 'rxjs/operators';
import { StorageService } from './storage.service';

@Injectable({
  providedIn: 'root'
})
export class BaseAddressGuard implements CanActivate {

/*
  This guard is responsible for the base address (e.g. https://web.tellma.com/).
  It simply checks if the user is authenticated and redirectes him/her as follows:
  - Authenticated: last visited page (default to My Companies)
  - Not authenticated: welcome page
*/

  constructor(private auth: AuthService, private router: Router, private storage: StorageService) { }

  canActivate(): Observable<boolean> {

    return this.auth.isSignedIn$.pipe(
      map(isAuthenticated => {
        const lastVisited = this.storage.getItem('last_visited_url_v2');
        const url = isAuthenticated ? (lastVisited || '/root/companies') : '/root/welcome';
        this.router.navigateByUrl(url);

        return false;
      }),
      catchError(_ => {
        this.router.navigateByUrl('/root/welcome');
        return of(false);
      })
    );
  }
}
