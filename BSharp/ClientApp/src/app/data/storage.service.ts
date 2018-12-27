import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  public getItem(key: string): any {
    return localStorage.getItem(key);
  }

  public setItem(key: string, value: any): void {
    localStorage.setItem(key, value);
  }

  public removeItem(key: string): void {
    localStorage.removeItem(key);
  }
}
