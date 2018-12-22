import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
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

  constructor(private translate: TranslateService, private route: ActivatedRoute) {

    const defaultLang = 'en'; // TODO load from app configuration
    this.translate.setDefaultLang(defaultLang);
    this.translate.use('en'); // TODO load from query string or from company/user configuration, should set it here to avoid flickr

    // this.store.culture = defaultLang;

    this.route.queryParamMap.subscribe(e => {
      const culture = e.get('ui-culture');
      if (!!culture) {
        this.setLanguage(culture);
      }
    });
  }

  setLanguage(lang: string) {
    this.translate.use(lang).subscribe(e => {
      // this.store.culture = lang; // TODO set a centralized value (not company specific)
    });
  }

  get isRTL(): boolean {
    const lang = 'en-US'; // TODO: read from centralized location
    return this.rtlLanguages.some(e => lang.startsWith(e));
  }
}
