import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { RootRoutingModule, PlaceholderComponent } from './root-routing.module';
import { RootComponent } from './root.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { ApiTranslateLoaderFactory } from './data/api-translate-loader';
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


@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent,
    UnauthorizedForCompanyComponent,
    ErrorLoadingCompanyComponent,
    PlaceholderComponent,
    SignOutComponent
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
        useFactory: ApiTranslateLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    { provide: OAuthStorage, useValue: localStorage },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: RootHttpInterceptor,
      deps: [WorkspaceService, ApiService, StorageService, Router, OAuthStorage, CleanerService],
      multi: true
    }
  ],
  bootstrap: [RootComponent]
})
export class RootModule { }
