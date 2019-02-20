import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from './data/workspace.service';
import { ApiService } from './data/api.service';
import { StorageService } from './data/storage.service';
import { AuthService } from './data/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-root',
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

  constructor(private translate: TranslateService, private workspace: WorkspaceService,
    private api: ApiService, private storage: StorageService, private route: ActivatedRoute) {

    // Callback after the new app culture is loaded
    this.translate.onLangChange.subscribe((_: any) => {
      // After ngx-translate successfully loads the language
      // we set it in the workspace so that all our components
      // reflect the change too
      const culture = this.translate.currentLang;
      this.setDocumentRTL(culture);
      if (!!document) {
        // TODO Load from configuration instead
        document.title = this.translate.instant('AppName');
      }

      // TODO Set in local storage properly
      this.storage.setItem('userCulture', culture);
    });

    // TODO load from app configuration
    // Fallback culture
    const defaultCulture = 'en';
    this.translate.setDefaultLang(defaultCulture);

    // TODO load from local storage properly
    const userCulture = this.storage.getItem('userCulture');
    console.log('UI Culture: ' + userCulture);
    if (!!userCulture) {
      this.translate.use(userCulture);
    }
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

  setDocumentRTL(culture: string) {
    this.workspace.ws.culture = culture;
    const isRtl = this.rtlLanguages.some(e => culture.startsWith(e));
    this.workspace.ws.isRtl = isRtl;
    if (isRtl && !!document) {
      document.body.classList.add('b-rtl');
    } else {
      document.body.classList.remove('b-rtl');
    }
  }

  get showOverlay(): boolean {
    // when there is a save in progress, block the user screen and prevent any navigation.
    return this.api.saveInProgress;
  }

}
