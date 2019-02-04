import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  public getItem(key: string): string {
    return localStorage.getItem(key); // TODO Change to local storage
  }

  public setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  public removeItem(key: string): void {
    localStorage.removeItem(key);
  }
}
