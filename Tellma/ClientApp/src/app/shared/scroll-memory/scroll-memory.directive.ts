import { Directive, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { timer } from 'rxjs';
import { AdminWorkspace, TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';

@Directive({
  selector: '[tScrollMemory]'
})
export class ScrollMemoryDirective implements OnChanges, OnDestroy {

  private restore = false;

  /**
   * The key for storing the scroll position
   */
  @Input('tScrollMemory')
  scrollKey: string;

  /**
   * Don't restore scroll position until this value goes back to what it was
   */
  @Input('tScrollMemoryTrigger')
  trigger: any;

  constructor(private element: ElementRef, private workspace: WorkspaceService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.key && !changes.key.isFirstChange()) {
      // When the key changes, treat like a destroy and create
      // i.e. store the old scroll value, and restore the new one
      const key = changes.key.previousValue;
      const trigger = !!changes.trigger ? changes.trigger.previousValue : this.trigger;
      this.rememberScroll(key, trigger);
    }

    this.restore = true;
  }

  ngOnDestroy(): void {
    this.rememberScroll(this.scrollKey, this.trigger);
  }

  // tslint:disable-next-line:use-lifecycle-interface
  ngAfterContentChecked() {
    if (this.restore) {
      this.restore = false;
      this.restoreScroll();
    }
  }

  // Scroll Memory in action

  private get ws(): TenantWorkspace | AdminWorkspace {
    return this.workspace.current;
  }

  private restoreScroll() {
    if (this.workspace.lastNavigation !== 'popstate') {
      // Scroll restoration only works in popstate navigation (browser back and forward)
      return;
    }
    const key = this.scrollKey;
    if (!!key) {
      const ws = this.ws;
      if (ws.scrollTriggers[key] === this.trigger) {
        const scrollPosition = ws.scrollPositions[key] || 0;
        // Do it synchronously to minimize the chance of jerky movement
        // And do it after a timeout to guarantee it will apply
        this.element.nativeElement.scrollTop = scrollPosition;
        timer(0).subscribe(() => this.element.nativeElement.scrollTop = scrollPosition);
      }
    }
  }

  public rememberScroll(key: string, trigger: any) {
    if (!!key) {
      const ws = this.ws;
      const scrollPosition = this.element.nativeElement.scrollTop;
      ws.scrollPositions[key] = scrollPosition;
      if (trigger === undefined) {
        delete ws.scrollTriggers[key];
      } else {
        ws.scrollTriggers[key] = trigger;
      }
    }
  }
}
