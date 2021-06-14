import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class IdService {

  private lastId = 0;

  public getId(): number {
    this.lastId++;
    return this.lastId;
  }
}
