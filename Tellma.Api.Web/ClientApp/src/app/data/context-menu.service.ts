import { Injectable, TemplateRef } from '@angular/core';

export type ShowMenu = ($event: MouseEvent, menu: TemplateRef<any>, context: any) => void;

/**
 * A simple relay between any component in the app that wishes to display a context menu and the root component
 */
@Injectable({
  providedIn: 'root'
})
export class ContextMenuService {

  private showMenuInner: ShowMenu;

  /**
   * Invoked once by the root component upon startup
   */
  public registerShowMenu(showMenu: ShowMenu) {
    this.showMenuInner = showMenu;
  }

  /**
   * Routes the command to the root component, which displays the given template and passes to it the given context
   */
  public showMenu($event: MouseEvent, menu: TemplateRef<any>, context: any) {
    this.showMenuInner($event, menu, context);
  }
}
