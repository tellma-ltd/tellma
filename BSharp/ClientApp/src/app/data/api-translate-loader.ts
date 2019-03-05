import { TranslateLoader } from '@ngx-translate/core';
import { Observable, throwError, Subject, of } from 'rxjs';
import { catchError, map, finalize } from 'rxjs/operators';
import { StorageService } from './storage.service';
import { ApiService } from './api.service';
import { saveTranslationsInStorage, translationStorageKey } from './root-http-interceptor';
import { ProgressOverlayService } from './progress-overlay.service';

// A custom loader for ngx-translate that loads the translation from the API
export class ApiTranslateLoader implements TranslateLoader {

  constructor(private api: ApiService, private progress: ProgressOverlayService, private storage: StorageService) { }

  public getTranslation(cultureName: string): Observable<any> {

    // for dazzling performance, load the translations immediately from local storage if available
    const key = translationStorageKey(cultureName);
    const translationsString = this.storage.getItem(key);
    if (!!translationsString) {
      try {
        const translations = JSON.parse(translationsString);
        return of(translations);
      } catch (err) {
        return of({});
      }
    } else {

      const progressKey = `translations_${cultureName}`;
      this.progress.startAsyncOperation(progressKey); // To show the rotator
      const obs$ = this.api.tranlationsApi(new Subject()).getForClient(cultureName)
        .pipe(
          map(dwv => {
            this.progress.completeAsyncOperation(progressKey);
            saveTranslationsInStorage(cultureName, dwv.Version, dwv.Data, this.storage);
            return dwv.Data;
          }),
          catchError(err => {
            this.progress.completeAsyncOperation(progressKey);
            return throwError(err);
          }),
          finalize(() => {
            this.progress.completeAsyncOperation(progressKey);
          }));

      // refresh the translations anyways on app startup or on switching to a new language
      return obs$;
    }
  }
}

// Make sure only a single instance is ever returned
export function apiTranslateLoaderFactory(api: ApiService, progress: ProgressOverlayService, storage: StorageService) {
  return new ApiTranslateLoader(api, progress, storage);
}
