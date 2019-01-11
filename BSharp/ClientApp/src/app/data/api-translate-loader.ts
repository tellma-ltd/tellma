import { HttpClient } from '@angular/common/http';
import { TranslateLoader } from '@ngx-translate/core';
import { Observable, throwError } from 'rxjs';
import { appconfig } from './appconfig';
import { catchError } from 'rxjs/operators';

// A custom loader for ngx-translate that loads the translation from the API
export class ApiTranslateLoader implements TranslateLoader {

  constructor(private http: HttpClient) { }

  getTranslation(lang: string): Observable<any> {
    const baseAddress = appconfig.apiAddress;
    const url = baseAddress + `api/translations/client-translations/${lang}`;
    // TODO use local storage to to instantly load the app
    return this.http.get(url)
    .pipe(catchError(err => {
      return throwError(err);
    }));
  }
}

export function ApiTranslateLoaderFactory(http: HttpClient) {
  return new ApiTranslateLoader(http);
}
