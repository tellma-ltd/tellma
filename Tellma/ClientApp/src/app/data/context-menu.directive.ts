// tslint:disable:member-ordering
import { Directive, Input, TemplateRef, HostListener } from '@angular/core';
import { ContextMenuService } from './context-menu.service';

@Directive({
  selector: '[tContextMenu]'
})
export class ContextMenuDirective {

  private mouseDownTimeoutId: any;

  /**
   * The contents of the context menu
   */
  @Input()
  tContextMenu: TemplateRef<any>;

  /**
   * The context of the context menu
   */
  @Input()
  tContext: any;

  /**
   * Stops any context menu from appearing
   */
  @Input()
  tDisableMenu = false;

  /**
   * In milliseconds: How long to keep pressing on a touch screen before the context menu is triggered
   */
  @Input()
  longpressDuration = 750;

  constructor(private ctx: ContextMenuService) { }

  @HostListener('contextmenu', ['$event'])
  handleMenu($event: MouseEvent) {
    if (!this.tDisableMenu && !$event.ctrlKey) {
      this.ctx.showMenu($event, this.tContextMenu, this.tContext);
    }
  }

  // For mobile users
  @HostListener('touchstart', ['$event'])
  handleTouchStart($event: any) {
    if (this.longpressDuration >= 0) {
      $event.stopPropagation();
      $event.clientY = $event.touches[0].clientY;
      $event.clientX = $event.touches[0].clientX;

      this.mouseDownTimeoutId = setTimeout(
        () => this.handleMenu($event),
        this.longpressDuration,
      );
    }
  }

  @HostListener('touchend')
  handleTouchEnd() {
    clearTimeout(this.mouseDownTimeoutId);
  }

  @HostListener('touchmove') // Drag n Drop
  handleTouchMove() {
    clearTimeout(this.mouseDownTimeoutId);
  }

  @HostListener('touchcancel')
  handleTouchCancel() {
    clearTimeout(this.mouseDownTimeoutId);
  }
}
