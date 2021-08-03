import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { ProgressOverlayService } from './progress-overlay.service';
import { finalize, retry } from 'rxjs/operators';

export class CustomTranslationsLoader implements TranslateLoader {
    constructor(private http: HttpClient, private progress: ProgressOverlayService) { }

    firstLang = true;

    /**
     * Gets the translations from the assets folder and shows the rotators when you change the language
     */
    public getTranslation(lang: string): Observable<any> {
        const key = 'loading_lang_' + lang;

        // Start the rotator
        if (this.firstLang) {
            this.firstLang = false;
        } else {
            this.progress.startAsyncOperation(key);
        }

        // Load the translations and then turn off the rotator
        return this.http.get(`/assets/i18n/${lang}.json`).pipe(
            retry(3),
            finalize(() => this.progress.completeAsyncOperation(key))
        );
    }
}
