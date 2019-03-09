import { NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { NativeScriptModule } from 'nativescript-angular/nativescript.module';
import { NativeScriptHttpClientModule } from 'nativescript-angular/http-client';

import { RootRoutingModule } from './root-routing.module';
import { RootComponent } from './root.component';
import { PageNotFoundComponent } from './features/page-not-found/page-not-found.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { apiTranslateLoaderFactory } from './data/api-translate-loader';
import { WorkspaceService } from './data/workspace.service';
import { RootHttpInterceptor } from './data/root-http-interceptor';
import { NativeScriptLocalizeModule } from 'nativescript-localize/angular';
import { UnauthorizedForCompanyComponent } from './features/unauthorized-for-company/unauthorized-for-company.component';
import { ErrorLoadingCompanyComponent } from './features/error-loading-company/error-loading-company.component';
import { SignOutComponent } from './features/sign-out/sign-out.component';
import { ErrorLoadingSettingsComponent } from './features/error-loading-settings/error-loading-settings.component';
import { RootShellComponent } from './features/root-shell/root-shell.component';

@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PageNotFoundComponent,
    UnauthorizedForCompanyComponent,
    ErrorLoadingCompanyComponent,
    SignOutComponent,
    ErrorLoadingSettingsComponent,
    RootShellComponent,
  ],
  imports: [
    NativeScriptModule,
    RootRoutingModule,
    NativeScriptHttpClientModule,
    NativeScriptLocalizeModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: apiTranslateLoaderFactory,
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
  bootstrap: [RootComponent],
  schemas: [NO_ERRORS_SCHEMA]
})
export class RootModule { }
