import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { RootRoutingModule } from './root-routing.module';
import { RootComponent } from './root.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { ApiTranslateLoaderFactory } from './data/api-translate-loader';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ApplicationHttpInterceptor } from './data/application-http-interceptor';
import { WorkspaceService } from './data/workspace.service';

@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent
  ],
  imports: [
    BrowserModule,
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
      useClass: ApplicationHttpInterceptor,
      deps: [WorkspaceService]
      multi: true
    }
  ],
  bootstrap: [RootComponent]
})
export class RootModule { }
