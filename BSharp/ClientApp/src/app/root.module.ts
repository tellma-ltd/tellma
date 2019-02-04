import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { RootRoutingModule } from './root-routing.module';
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


@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent,
    UnauthorizedForCompanyComponent,
    ErrorLoadingCompanyComponent
  ],
  imports: [
    BrowserModule,
    FontAwesomeModule,
    RootRoutingModule,
    HttpClientModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: ApiTranslateLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: RootHttpInterceptor,
      deps: [WorkspaceService, ApiService, StorageService, Router],
      multi: true
    }
  ],
  bootstrap: [RootComponent]
})
export class RootModule { }
