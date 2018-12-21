import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  public getItem(key: string): Observable<any> {
    return of(localStorage.getItem(key));
  }

  public setItem(key: string, value: any): Observable<void> {
    localStorage.setItem(key, value);
    return of();
  }

  public removeItem(key: string): Observable<void> {
    localStorage.removeItem(key);
    return of();
  }
}
