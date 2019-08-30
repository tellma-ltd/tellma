import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { RootRoutingModule, PlaceholderComponent } from './root-routing.module';
import { RootComponent } from './root.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import {TranslateHttpLoader} from '@ngx-translate/http-loader';
import { WorkspaceService } from './data/workspace.service';
import { RootHttpInterceptor } from './data/root-http-interceptor';
import { StorageService } from './data/storage.service';
import { ApiService } from './data/api.service';
import { Router } from '@angular/router';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { CleanerService } from './data/cleaner.service';
import { SignOutComponent } from './features/sign-out/sign-out.component';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faInternetExplorer } from '@fortawesome/free-brands-svg-icons';
import {
  faSpinner, faArrowRight, faArrowLeft, faChevronRight, faSyncAlt, faSearch,
  faHands, faCube, faCogs, faSignInAlt, faExclamationTriangle, faHome, faRedoAlt
} from '@fortawesome/free-solid-svg-icons';
import { ProgressOverlayService } from './data/progress-overlay.service';
import { RootShellComponent } from './features/root-shell/root-shell.component';
import { NgbCollapseModule, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { LandingComponent } from './features/landing/landing.component';
import { ErrorComponent } from './features/error/error.component';

library.add(faInternetExplorer, faSpinner, faArrowRight, faArrowLeft, faChevronRight,
   faSyncAlt, faSearch, faCube, faCogs, faHands, faSignInAlt, faExclamationTriangle, faHome, faRedoAlt);

   // AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http);
}

@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PlaceholderComponent,
    SignOutComponent,
    RootShellComponent,
    LandingComponent,
    ErrorComponent
  ],
  imports: [
    BrowserModule,
    FontAwesomeModule,
    RootRoutingModule,
    HttpClientModule,
    NgbCollapseModule,
    NgbDropdownModule,
    OAuthModule.forRoot(),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    }),
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production })
  ],
  providers: [
    { provide: OAuthStorage, useValue: localStorage },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: RootHttpInterceptor,
      deps: [WorkspaceService, ApiService, StorageService, Router, OAuthStorage, CleanerService, TranslateService],
      multi: true
    }
  ],
  bootstrap: [RootComponent]
})
export class RootModule { }
