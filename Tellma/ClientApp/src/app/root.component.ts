import { Component, ApplicationRef, Inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from './data/workspace.service';
import { ApiService } from './data/api.service';
import { StorageService } from './data/storage.service';
import { Router, NavigationEnd } from '@angular/router';
import { SwUpdate } from '@angular/service-worker';
import { interval, concat } from 'rxjs';
import { first } from 'rxjs/operators';
import { ProgressOverlayService } from './data/progress-overlay.service';
import { NgbDropdownConfig } from '@ng-bootstrap/ng-bootstrap';
import { DOCUMENT } from '@angular/common';

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
    @Inject(DOCUMENT) private document: Document) {

    // This came at long last with ng-bootstrap v4.1.0 allowing us to specify that
    // all dropdowns should be appended to the body by default
    dropdownConfig.container = 'body';

    // If the user navigates to the base address '/', she
    // gets automatically redirected to the last visited url
    this.router.events.subscribe(e => {
      if (e instanceof NavigationEnd && e.url.indexOf('/app/') !== -1) {
        this.storage.setItem('last_visited_url', e.url);
      }
    });

    // check for a new version every 6 hours, taken from the official docs https://bit.ly/2VfkAgQ
    const appIsStable$ = appRef.isStable.pipe(first(isStable => isStable === true));
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

    // show a message if the user opens the app for the first time on Internet Explorer
    // tslint:disable-next-line:no-string-literal
    const isIE = (/*@cc_on!@*/false) || (document['documentMode']);
    const dismissedBefore = this.storage.getItem('ie_warning_dismissed');
    this.showIEWarning = isIE && !dismissedBefore;

    // Callback after the new app culture is loaded
    this.translate.onLangChange.subscribe((_: any) => {
      // After ngx-translate successfully loads the language
      // we set it in the workspace so that all our components
      // reflect the change too
      const culture = this.translate.currentLang;
      this.setWorkspaceCulture(culture);
      if (!!document) {
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

  public onDismissIEWarning() {
    this.showIEWarning = false;
    this.storage.setItem('ie_warning_dismissed', 'true');
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
    if (isRtl && !!document) {
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
}
