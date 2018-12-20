import { HttpClient } from "@angular/common/http";
import { TranslateLoader } from "@ngx-translate/core";
import { Observable } from 'rxjs';

export class ApiTranslateLoader implements TranslateLoader {


  constructor(private http: HttpClient) { }

  getTranslation(lang: string): Observable<any> {
    const address = appconfig.apiAddress;
    // TODO use local storage to to instantly load the app
    return this.http.get(address + `api/translations/client-translations/${lang}`); 
  }
}

export function ApiTranslateLoaderFactory(http: HttpClient)
{
  return new ApiTranslateLoader(http);
}
