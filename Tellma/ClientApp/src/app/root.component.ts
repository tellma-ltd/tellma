// tslint:disable:member-ordering
import { Component, ApplicationRef, Inject, ViewContainerRef, TemplateRef, NgZone } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from './data/workspace.service';
import { ApiService } from './data/api.service';
import { StorageService } from './data/storage.service';
import { Router, NavigationEnd, NavigationStart } from '@angular/router';
import { SwUpdate } from '@angular/service-worker';
import { interval, concat, fromEvent, Subscription, Subject } from 'rxjs';
import { first, filter, take, tap } from 'rxjs/operators';
import { ProgressOverlayService } from './data/progress-overlay.service';
import { NgbDropdownConfig } from '@ng-bootstrap/ng-bootstrap';
import { DOCUMENT } from '@angular/common';
import { ContextMenuService } from './data/context-menu.service';
import { Overlay, OverlayRef, ConnectedPosition } from '@angular/cdk/overlay';
import { TemplatePortal } from '@angular/cdk/portal';

@Component({
  selector: 't-root',
  templateUrl: './root.component.html',
  styles: []
})
export class RootComponent {

  // If the selected langauge is any of the below
  // the entire application is swapped to RTL layout
  private rtlLanguages = [
    'ae',	/* Avestan */
    'ar',   /* 'العربية', Arabic */
    'arc',  /* Aramaic */
    'bcc',  /* 'بلوچی مکرانی', Southern Balochi */
    'bqi',  /* 'بختياري', Bakthiari */
    'ckb',  /* 'Soranî / کوردی', Sorani */
    'dv',   /* Dhivehi */
    'fa',   /* 'فارسی', Persian */
    'glk',  /* 'گیلکی', Gilaki */
    'he',   /* 'עברית', Hebrew */
    'ku',   /* 'Kurdî / كوردی', Kurdish */
    'mzn',  /* 'مازِرونی', Mazanderani */
    'nqo',  /* N'Ko */
    'pnb',  /* 'پنجابی', Western Punjabi */
    'ps',   /* 'پښتو', Pashto, */
    'sd',   /* 'سنڌي', Sindhi */
    'ug',   /* 'Uyghurche / ئۇيغۇرچە', Uyghur */
    'ur',   /* 'اردو', Urdu */
    'yi'    /* 'ייִדיש', Yiddish */
  ];

  public showNewUpdateIsAvailable = false;
  public showIEWarning = false;

  constructor(
    private translate: TranslateService, private workspace: WorkspaceService, private router: Router,
    private api: ApiService, private storage: StorageService, private progress: ProgressOverlayService,
    private serviceWorker: SwUpdate, appRef: ApplicationRef, dropdownConfig: NgbDropdownConfig,
    @Inject(DOCUMENT) private document: Document, private overlay: Overlay, contextMenu: ContextMenuService,
    private viewContainerRef: ViewContainerRef, private zone: NgZone) {

    // This came at long last with ng-bootstrap v4.1.0 allowing us to specify that
    // all dropdowns should be appended to the body by default
    dropdownConfig.container = 'body';

    // If the user navigates to the base address '/', she
    // gets automatically redirected to the last visited url
    this.router.events.subscribe(e => {
      if (e instanceof NavigationEnd && e.url.indexOf('/app/') !== -1) {
        this.storage.setItem('last_visited_url_v2', e.url);
      }

      // Hide any active context menu before navigating
      if (e instanceof NavigationStart) {
        this.hideContextMenu();
      }
    });

    // When the entire window loses focus, hide context menus
    window.onblur = this.hideContextMenu;

    // Hide the context menu if any scrolling whatsoever takes place
    document.addEventListener('scroll', this.hideContextMenu, true);

    // Track the browser tab visibility, certain processes (such as dashboard polling) go to sleep when the page is invisible
    workspace.visibility = document.visibilityState || 'visible';
    document.addEventListener('visibilitychange', () => {
      workspace.visibility = document.visibilityState;
      (workspace.visibilityChanged$ as Subject<void>).next();
    });

    // Track the page size (above medium or below medium)
    const mediaQuery = window.matchMedia('(min-width: 768px)');
    workspace.mediumDevice = mediaQuery.matches;
    mediaQuery.onchange = ev => {
      // Run in the zone, so Angular change detection kicks in
      this.zone.run(() => {
        workspace.mediumDevice = ev.matches;
        (workspace.mediumDeviceChanged$ as Subject<void>).next();
      });
    };
    // This allows any component in the app to display a context menu with ease
    contextMenu.registerShowMenu(this.showContextMenu);

    // check for a new version every 6 hours, taken from the official docs https://bit.ly/2VfkAgQ
    const appIsStable$ = appRef.isStable.pipe(
      first(isStable => isStable === true),
      tap(_ => console.log('App is stable...'))
    );
    const everySixHours$ = interval(6 * 60 * 60 * 1000);
    const everySixHoursOnceAppIsStable$ = concat(appIsStable$, everySixHours$);
    everySixHoursOnceAppIsStable$.subscribe(() => {
      if (serviceWorker.isEnabled) {
        serviceWorker.checkForUpdate();
      }
    });

    // listen for notifications from the service worker that a new version of the client is available
    this.serviceWorker.available.subscribe(_ => {
      this.showNewUpdateIsAvailable = true;
    });

    // show a message if the user opens the app on Internet Explorer
    // tslint:disable-next-line:no-string-literal
    const isIE = (/*@cc_on!@*/false) || (document['documentMode']);
    this.showIEWarning = isIE;

    // Callback after the new app culture is loaded
    this.translate.onLangChange.subscribe((_: any) => {
      // After ngx-translate successfully loads the language
      // we set it in the workspace so that all our components
      // reflect the change too
      const culture = this.translate.currentLang;
      this.setWorkspaceCulture(culture);
      if (!!this.document) {
        // TODO Load from configuration instead
        this.document.title = this.translate.instant('AppName');
      }
    });

    // IMPORTANT: also in application-shell.component.ts, keep in sync
    const defaultCulture = this.document.documentElement.lang || 'en';
    this.translate.setDefaultLang(defaultCulture);

    const userCulture = this.storage.getItem('user_culture') || defaultCulture;
    this.translate.use(userCulture);
  }

  public onRefresh() {
    this.document.location.reload();
  }

  private getUrlUiCulture(): string {
    // this is an ugly hack since we can't retrieve the url parameters on startup
    if (!!location && !!location.href) {
      const href = location.href;
      const paramName = 'ui-culture';
      const i = href.indexOf(paramName);
      if (i !== -1) {
        const uiCulture = href.substr(i + paramName.length + 1);
        return decodeURIComponent(uiCulture);
      }
    }

    return null;
  }

  setWorkspaceCulture(culture: string) {

    // set the culture
    this.workspace.ws.culture = culture;

    // set isRTL in workspace
    const isRtl = this.rtlLanguages.some(e => culture.startsWith(e));
    this.workspace.ws.isRtl = isRtl;

    // notify everyone about the change
    this.workspace.notifyStateChanged();

    // set RTL on the DOM document
    if (isRtl && !!this.document) {
      this.document.body.classList.add('t-rtl');
    } else {
      this.document.body.classList.remove('t-rtl');
    }
  }

  get showOverlay(): boolean {
    // when there is a save in progress, block the user screen and prevent any navigation.
    return this.api.showRotator || this.progress.asyncOperationInProgress;
  }

  get showOfflineIndicator(): boolean {
    return this.workspace.offline;
  }

  get labelNames(): string[] {
    return this.progress.labelNames;
  }

  // Context menu stuff

  private overlayRef: OverlayRef;
  private sub: Subscription;

  private showContextMenu = ($event: MouseEvent, ctxMenu: TemplateRef<any>, ctx: any) => {
    if (!ctxMenu) {
      return;
    }

    // Close any existing context menu, this unsubscribes sub
    this.hideContextMenu();

    // Prevents the browser context menu
    $event.preventDefault();

    const positions: ConnectedPosition[] = [
      {
        originX: 'start',
        originY: 'bottom',
        overlayX: 'start',
        overlayY: 'top',
      },
      {
        originX: 'start',
        originY: 'top',
        overlayX: 'start',
        overlayY: 'bottom',
      },
      {
        originX: 'end',
        originY: 'top',
        overlayX: 'start',
        overlayY: 'top',
      },
      {
        originX: 'start',
        originY: 'top',
        overlayX: 'end',
        overlayY: 'top',
      },
      {
        originX: 'end',
        originY: 'center',
        overlayX: 'start',
        overlayY: 'center',
      },
      {
        originX: 'start',
        originY: 'center',
        overlayX: 'end',
        overlayY: 'center',
      },
    ];

    const positionStrategy = this.overlay.position()
      .flexibleConnectedTo({ x: $event.clientX, y: $event.clientY, width: 0, height: 0 })
      .withPositions(positions)
      .withFlexibleDimensions(false)
      .withPush(false);

    // Create the overlay in the screen where the context menu will be hosted
    this.overlayRef = this.overlay.create({ positionStrategy });

    // Handle RTL UI
    if (this.workspace.ws.isRtl) {
      this.overlayRef.setDirection('rtl');
    }

    // Add the context menu to the overlay
    this.overlayRef.attach(new TemplatePortal(ctxMenu, this.viewContainerRef, { $implicit: ctx, close: this.hideContextMenu }));

    // Make sure it closes when clicking outside of it
    this.sub = new Subscription();
    this.sub.add(fromEvent<MouseEvent>(this.document, 'mousedown')
      .pipe(
        filter(event => {
          const clickTarget = event.target as HTMLElement;
          return !this.insideContextMenu(clickTarget);
        }),
        take(1)
      ).subscribe(() => this.hideContextMenu()));

    // Prevent the context menu itself from launching the browser context menu
    this.sub.add(fromEvent<MouseEvent>(this.document, 'contextmenu')
      .pipe(
        tap(e => {
          const clickTarget = e.target as HTMLElement;
          if (this.insideContextMenu(clickTarget)) {
            // Right click inside the context menu does nothing
            e.preventDefault();
          } else if (e !== $event) {
            // Right click anywhere else dismisses the context menu
            this.hideContextMenu();
          }
        }),
      ).subscribe());

    // TODO: Other events that are expected to dismiss the context menu
  }

  private hideContextMenu = () => {
    if (!!this.overlayRef) {
      this.overlayRef.dispose();
      delete this.overlayRef;
    }

    if (!!this.sub) {
      this.sub.unsubscribe();
      delete this.sub;
    }
  }

  private insideContextMenu = (target: HTMLElement): boolean => {
    return !!this.overlayRef && !!this.overlayRef.overlayElement && this.overlayRef.overlayElement.contains(target);
  }
}
