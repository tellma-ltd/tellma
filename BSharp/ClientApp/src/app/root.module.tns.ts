import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { NativeScriptModule } from 'nativescript-angular/nativescript.module';
import { NativeScriptHttpClientModule } from 'nativescript-angular/http-client';

import { RootRoutingModule } from './root-routing.module';
import { RootComponent } from './root.component';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';
import { ApiTranslateLoaderFactory } from './data/api-translate-loader';

// Uncomment and add to NgModule imports if you need to use two-way binding
// import { NativeScriptFormsModule } from 'nativescript-angular/forms';


@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent
  ],
  imports: [
    NativeScriptModule,
    RootRoutingModule,
    NativeScriptHttpClientModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: ApiTranslateLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [],
  bootstrap: [RootComponent],
  schemas: [NO_ERRORS_SCHEMA]
})
export class RootModule { }
