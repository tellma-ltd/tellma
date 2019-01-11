import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  public getItem(key: string): string {
    return sessionStorage.getItem(key); // TODO Change to local storage
  }

  public setItem(key: string, value: string): void {
    sessionStorage.setItem(key, value);
  }

  public removeItem(key: string): void {
    sessionStorage.removeItem(key);
  }
}
