import { Injectable } from '@angular/core';
import { Location } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {

  // the logic in this service handles forward and backward navigation
  // which should be provided in every screen of every module when in standalone mode

  constructor(private location: Location) { }

  public onForward() {
    this.location.forward();
  }

  public get canForward(): boolean {
    return true;
  }

  public get showForward(): boolean {
    return this.isStandalone;
  }

  public onBack() {
    this.location.back();
  }

  public get canBack(): boolean {
    return true;
  }

  public get showBack(): boolean {
    return this.isStandalone;
  }

  private get isStandalone() {
    return window.navigator['standalone'] || window.matchMedia('(display-mode: standalone)').matches;
  }
}
