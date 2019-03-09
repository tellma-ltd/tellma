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
  this guard is responsible the base address (e.g. https://www.bsharp.online/)
  it simply checks if the user is authenticated and redirectes him/her as follows:
  - authenticated: application
  - not authenticated: welcome page
*/

  constructor(private auth: AuthService, private router: Router, private storage: StorageService) { }

  canActivate(): Observable<boolean> {

    return this.auth.isSignedIn$.pipe(
      map(isAuthenticated => {
        const lastVisited = this.storage.getItem('last_visited_url');
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
