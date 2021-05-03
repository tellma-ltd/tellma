import { NgModule, Component } from '@angular/core';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { RootComponent } from './root.component';
import { CompaniesComponent } from './features/companies/companies.component';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { HttpClientModule, HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from './data/workspace.service';
import { RootHttpInterceptor } from './data/root-http-interceptor';
import { StorageService } from './data/storage.service';
import { ApiService } from './data/api.service';
import { Router, RouterModule, Routes, PreloadAllModules, NavigationStart } from '@angular/router';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { CleanerService } from './data/cleaner.service';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { faInternetExplorer } from '@fortawesome/free-brands-svg-icons';
import {
  faSpinner, faArrowRight, faArrowLeft, faChevronRight, faSyncAlt, faSearch,
  faHands, faCube, faCogs, faSignInAlt, faExclamationTriangle, faHome, faRedoAlt, faWifi
} from '@fortawesome/free-solid-svg-icons';
import { RootShellComponent } from './features/root-shell/root-shell.component';
import { NgbCollapseModule, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { LandingComponent } from './features/landing/landing.component';
import { ErrorComponent } from './features/error/error.component';
import { AuthGuard } from './data/auth.guard';
import { GlobalResolverGuard } from './data/global-resolver.guard';
import { SignInCallbackGuard } from './data/sign-in-callback.guard';
import { BaseAddressGuard } from './data/base-address.guard';
import { CustomTranslationsLoader } from './data/custom-translations-loader';
import { ProgressOverlayService } from './data/progress-overlay.service';
import { OverlayModule } from '@angular/cdk/overlay';
import { filter, tap } from 'rxjs/operators';

// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient, progress: ProgressOverlayService) {
  return new CustomTranslationsLoader(http, progress);
}

export function LoadApplicationModule() {
  const app = import('./features/application.module').then(m => m.ApplicationModule);
  return app;
}

export function LoadAdminModule() {
  const admin = import('./features/admin.module').then(m => m.AdminModule);
  return admin;
}

/**
 * this component is never displayed, we use it to do redirecting, we set it in a route and protect
 * it with a guard that always returns false, and the guard is responsible for redirecting the user
 *
 * -- example usage --
 *
 * (1) sign-in-callback the guard is responsible for parsing and validating the authentication tokens
 * that come from the identity server, and then redirecting the user to the originally requested URL,
 * or to the home page if there was no requested URL
 *
 * (2) base address '/', the guard checks if the user is authenticated, if s/he is the guard redirects
 * the user to the application module, otherwise to the landing module/welcome screen
 */
@Component({ template: '<div></div>' }) export class PlaceholderComponent { }

export const routes: Routes = [
  ///// Almost all routes require loading the global settings first
  {
    path: '',
    canActivate: [GlobalResolverGuard],
    children: [
      {
        path: 'root',
        component: RootShellComponent,
        children: [
          {
            path: 'welcome',
            component: LandingComponent,
          },
          {
            path: 'companies',
            component: CompaniesComponent,
            canActivate: [AuthGuard]
          },
          {
            path: 'error/:error',
            component: ErrorComponent
          },
          {
            path: '',
            redirectTo: 'welcome',
            pathMatch: 'full'
          },
          {
            path: '**',
            redirectTo: 'error/page-not-found',
          }
        ]
      },

      // Lazy loaded modules,
      {
        path: 'app',
        canActivate: [AuthGuard], // otherwise the tenant resolver can't work
        loadChildren: LoadApplicationModule,
        data: { preload: true }
      },
      {
        path: 'a',
        redirectTo: 'app'
      },
      {
        path: 'admin',
        canActivate: [AuthGuard],
        loadChildren: LoadAdminModule,
        data: { preload: false }
      },

      // those paths always end in a redirect
      { path: 'sign-in-callback', component: PlaceholderComponent, canActivate: [SignInCallbackGuard] },
      { path: '', component: PlaceholderComponent, canActivate: [BaseAddressGuard] },
    ]
  },

  ///// Error screens are accessible regardless of global settings

  // error screen
  { path: 'error/:error', component: ErrorComponent },

  // page not found
  {
    path: '**',
    redirectTo: 'error/page-not-found',
  }
];

@NgModule({
  declarations: [
    RootComponent,
    CompaniesComponent,
    PlaceholderComponent,
    RootShellComponent,
    LandingComponent,
    ErrorComponent
  ],
  imports: [
    NoopAnimationsModule,
    FontAwesomeModule,
    RouterModule.forRoot(routes, {
    preloadingStrategy: PreloadAllModules,
    enableTracing: false,
    relativeLinkResolution: 'legacy'
}),
    OverlayModule,
    HttpClientModule,
    NgbCollapseModule,
    NgbDropdownModule,
    OAuthModule.forRoot(),
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient, ProgressOverlayService]
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
    },
  ],
  bootstrap: [RootComponent]
})
export class RootModule {

  constructor(library: FaIconLibrary, router: Router, workspace: WorkspaceService) {
    library.addIcons(faInternetExplorer, faSpinner, faArrowRight, faArrowLeft, faChevronRight, faWifi,
      faSyncAlt, faSearch, faCube, faCogs, faHands, faSignInAlt, faExclamationTriangle, faHome, faRedoAlt);

    router.events.pipe(
      filter((e) => e instanceof NavigationStart),
      tap((e: NavigationStart) => workspace.lastNavigation = e.navigationTrigger),
    ).subscribe();
  }
}
