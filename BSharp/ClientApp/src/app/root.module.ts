import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { RootRoutingModule, PlaceholderComponent } from './root-routing.module';
import { RootComponent } from './root.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { apiTranslateLoaderFactory } from './data/api-translate-loader';
import { WorkspaceService } from './data/workspace.service';
import { RootHttpInterceptor } from './data/root-http-interceptor';
import { StorageService } from './data/storage.service';
import { ApiService } from './data/api.service';
import { UnauthorizedForCompanyComponent } from './features/unauthorized-for-company/unauthorized-for-company.component';
import { ErrorLoadingCompanyComponent } from './features/error-loading-company/error-loading-company.component';
import { Router } from '@angular/router';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { CleanerService } from './data/cleaner.service';
import { SignOutComponent } from './features/sign-out/sign-out.component';
import { ErrorLoadingSettingsComponent } from './features/error-loading-settings/error-loading-settings.component';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faInternetExplorer } from '@fortawesome/free-brands-svg-icons';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
import { ProgressOverlayService } from './data/progress-overlay.service';
import { RootShellComponent } from './features/root-shell/root-shell.component';

library.add(faInternetExplorer, faSpinner);

@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent,
    UnauthorizedForCompanyComponent,
    ErrorLoadingCompanyComponent,
    PlaceholderComponent,
    SignOutComponent,
    ErrorLoadingSettingsComponent,
    RootShellComponent,
  ],
  imports: [
    BrowserModule,
    FontAwesomeModule,
    RootRoutingModule,
    HttpClientModule,
    OAuthModule.forRoot(),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: apiTranslateLoaderFactory,
        deps: [ApiService, ProgressOverlayService, StorageService]
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
