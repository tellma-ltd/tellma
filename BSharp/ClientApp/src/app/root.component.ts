import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { WorkspaceService } from './data/workspace.service';

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

  constructor(private translate: TranslateService, private route: ActivatedRoute, private workspace : WorkspaceService) {

    const defaultLang = 'en'; // TODO load from app configuration
    this.translate.setDefaultLang(defaultLang);
    this.setCulture('en'); // TODO load from query string or from company/user configuration, should set it here to avoid flickr

    // this.store.culture = defaultLang;

    this.route.queryParamMap.subscribe(e => {
      const culture = e.get('ui-culture');
      if (!!culture) {
        this.setCulture(culture);
      }
    });
  }

  setCulture(culture: string) {
    this.translate.use(culture).subscribe(e => {
      this.workspace.ws.culture = culture;
      this.workspace.ws.isRtl = this.rtlLanguages.some(e => culture.startsWith(e));
    });
  }

  get isRTL(): boolean {
    return this.workspace.ws.isRtl;
  }
}
