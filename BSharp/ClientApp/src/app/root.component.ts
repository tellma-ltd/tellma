import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from './data/workspace.service';

@Component({
  selector: 'b-root',
  templateUrl: './root.component.html',
  styles: []
})
export class RootComponent {

  private QUERY_PARAM_NAME = 'ui-culture';

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

  constructor(private translate: TranslateService, private workspace: WorkspaceService) {

    // Callback after the new app culture is loaded
    this.translate.onLangChange.subscribe(_ => {
      // After ngx-translate successfully loads the language
      // we set it in the workspace so that all our components
      // reflect the change too
      const culture = this.translate.currentLang;
      this.setDocumentRTL(culture);

      // TODO Set in local storage properly
      sessionStorage.setItem('userCulture', culture);
    });

     // TODO load from app configuration
    // Fallback culture
    const defaultCulture = 'en';
    this.translate.setDefaultLang(defaultCulture);

    // TODO load from local storage properly
    const userCulture = sessionStorage.getItem('userCulture');
    if (!!userCulture) {
      this.translate.use(userCulture);
    }

    //// TEMPORARY FOR DEBUGGING
    //// This monitors changes on the query params and updates the app's culture accordingly
    //this.route.queryParamMap.pipe(
    //  map(e => e.get(this.QUERY_PARAM_NAME)),
    //  filter(culture => !!culture),
    //  switchMap(culture => {
    //    return this.translate.use(culture);
    //  })
    //).subscribe();
  }
  
  setDocumentRTL(culture: string) {
    this.workspace.ws.culture = culture;
    const isRtl = this.rtlLanguages.some(e => culture.startsWith(e));
    this.workspace.ws.isRtl = isRtl;
    if (isRtl && !!document) {
      document.body.classList.add('b-rtl');
    }
    else {
      document.body.classList.remove('b-rtl');
    }
  }
}
