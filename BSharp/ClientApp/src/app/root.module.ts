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


@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent
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
      deps: [WorkspaceService],
      multi: true
    }
  ],
  bootstrap: [RootComponent]
})
export class RootModule { }
