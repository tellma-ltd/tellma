import { Component } from '@angular/core';
import { MenuComponent, ContextMenuService, MenuPackage } from '@ctrl/ngx-rightclick';

@Component({
  selector: 't-context-menu',
  templateUrl: './context-menu.component.html',
  styles: []
})
export class ContextMenuComponent extends MenuComponent {
  // this module does not have animations, set lazy false
  lazy = false;

  public context: any;

  constructor(public menuPackage: MenuPackage, public contextMenuService: ContextMenuService) {
    super(menuPackage, contextMenuService);
    // grab any required menu context passed via menuContext input
    this.context = menuPackage.context;
  }

  handleClick() {
    // IMPORTANT! tell the menu to close, anything passed in here is given to (menuAction)
    this.contextMenuService.closeAll();
  }
}
