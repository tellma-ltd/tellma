import { Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { Subject, Observable } from 'rxjs';

interface IStorageEvent {
  key: string;
  newValue: string;
  oldValue: string;
}

@Injectable({
  providedIn: 'root'
})
export class StorageService implements OAuthStorage {

  private _changed$ = new Subject<IStorageEvent>();

  constructor() {
    if (!!addEventListener) {
      addEventListener('storage', (e: StorageEvent) => {

        // Let the world know
        this._changed$.next(e);

      }, false);
    }
  }

  public get changed$(): Observable<IStorageEvent> {
    return this._changed$;
  }

  public getItem(key: string): string {
    return localStorage.getItem(key); // TODO Change to local storage
  }

  public setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  public removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  public clear(): void {
    localStorage.clear();
  }
}
