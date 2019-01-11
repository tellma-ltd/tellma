import { Injectable } from '@angular/core';
import { getString, setString, remove } from 'tns-core-modules/application-settings';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  public getItem(key: string): string {
    return getString(key);
  }

  public setItem(key: string, value: string): void {
    setString(key, value);
  }

  public removeItem(key: string): void {
    remove(key);
  }
}
