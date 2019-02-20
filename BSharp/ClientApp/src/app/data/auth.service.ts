import { Injectable } from '@angular/core';
import { AuthConfig, OAuthService, JwksValidationHandler, OAuthEvent, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { appconfig } from './appconfig';
import { Subject, Observable, timer, of, from, ReplaySubject } from 'rxjs';
import { catchError, filter, map, flatMap } from 'rxjs/operators';
import { StorageService } from './storage.service';

const authConfig: AuthConfig = {

  // url of the Identity Provider
  issuer: appconfig.identityAddress,

  // url of the SPA to redirect the user to after login
  redirectUri: window.location.origin + '/sign-in-callback',

  // url of the SPA to redirect the hidden iFrame to after a silent refresh
  silentRefreshRedirectUri: window.location.origin + '/assets/silent-refresh-callback.html',

  // enable OpenID Connect session management
  sessionChecksEnabled: true,

  // without this the angular router was getting thrown off and refused to redirect in SignInCallbackGuard
  clearHashAfterLogin: false,

  // the SPA's id. The SPA is registerd with this id at the auth-server
  clientId: 'WebClient',

  // the scope for the permissions the client should request
  scope: 'openid profile email bsharp',

  // these can be null and if they are they will be retrieved by loading the discovery document
  // setting them in the appconfig is just an optimization to allow instant startup of the app
  jwks: appconfig.identityConfig ? appconfig.identityConfig.jwks : null,
  loginUrl: appconfig.identityConfig ? appconfig.identityConfig.loginUrl : null,
  logoutUrl: appconfig.identityConfig ? appconfig.identityConfig.logoutUrl : null,
  sessionCheckIFrameUrl: appconfig.identityConfig ? appconfig.identityConfig.sessionCheckIFrameUrl : null,
};

// a set of events that various services in the application are interested in knowing about
export enum AuthEvent {

  // the user independently navigated to Identity Server and signed out from there
  SignedOutFromAuthority = 'signed_out_from_authority',

  // the user independently navigated to Identity Server and signed in as a different user
  SignedInFromAuthorityAsDifferentUser = 'signed_in_from_authority_as_different_user',

  // misc_error_while_checking_session_state
  SessionError = 'session_error',

  // the user cleared the access token in the storage from another tab or manually or replaced it with an invalid one
  StorageIsCleared = 'storage_is_cleared',

  // a valid token has been added to local storage or the token has been replaced with a new valid token
  StorageHasNewValidToken = 'storage_has_new_valid_token'
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  // Note: this service is a wrapper around the 'angular-oauth2-oidc' library + extra functionality

  private nonce: string;
  private discoveryDocumentLoaded$ = new ReplaySubject<boolean>();

  constructor(private oauth: OAuthService, private storage: StorageService) {
    this.init();
  }

  private _events$ = new Subject<AuthEvent>();
  public get events$(): Observable<AuthEvent> {
    return this._events$;
  }

  private init(): void {

    // configure the oidc library
    this.oauth.configure(authConfig);

    // set the validation handler
    this.oauth.tokenValidationHandler = new JwksValidationHandler();

    // relay some events from the storage system
    this.storage.changed$.subscribe(e => {
      if (e.key === 'access_token') {
        if (!e.newValue) {
          this._events$.next(AuthEvent.StorageIsCleared);
        } else {
          this._events$.next(AuthEvent.StorageHasNewValidToken);
        }
      }
    });

    // relay some events already provided by angular-oauth2-oidc
    (<Observable<OAuthEvent>>this.oauth['events']).subscribe(e => {

      if (e.type === 'session_terminated') {
        this._events$.next(AuthEvent.SignedOutFromAuthority);
      }

      if (e instanceof OAuthErrorEvent && e.type === 'token_validation_error' && !!e.reason &&
        e.reason.toString().startsWith('After refreshing, we got an id_token for another user (sub)')) {
        // this is a little hacky but the library provides no other API to distinguish the token validation errors
        this._events$.next(AuthEvent.SignedInFromAuthorityAsDifferentUser);
      }

      if (e.type === 'session_error') {
        this._events$.next(AuthEvent.SessionError);
      }

      // capture the discovery document events in a replay subject
      // so that all events that require the discovery documents can
      // reliably wait for it to be available
      if (this.isDiscoveryDocumentNeeded) {

        if (e.type === 'discovery_document_loaded') {
          if (!!e['info']) {
            this.discoveryDocumentLoaded$.next(true);
          }
        }

        if (e.type === 'discovery_document_validation_error') {
          this.discoveryDocumentLoaded$.error('validation error');
        }

        if (e.type === 'discovery_document_load_error') {
          this.discoveryDocumentLoaded$.error('loading error');
        }
      }
    });


    if (this.isDiscoveryDocumentNeeded) {
      // if the configuration is not complete then we must load the discovery document
      // it will be needed for: sign-in, sign-out, url token validation and silent refresh
      this.oauth.loadDiscoveryDocument();
    } else {
      // if no discovery document is needed, return true immediately
      // whenever someone is waiting for it
      this.discoveryDocumentLoaded$.next(true);
    }
  }

  public setupAutomaticSilentRefresh() {

    // setup periodic silent refresh
    const basePeriodInSecondsFromConfig = appconfig.identityConfig ? appconfig.identityConfig.tokenRefreshPeriodInSeconds : null;
    const basePeriod = (basePeriodInSecondsFromConfig || (60 * 60)) * 1000; // Default is 1 hour
    const rand = (Math.random() * 2) - 1; // between 1 and -1
    const period = basePeriod + (rand * basePeriod / 2); // 1 hour +/-30 minutes
    timer(0, period)
      .pipe(
        filter(_ => this.isAuthenticated),
        catchError(_ => of(0))
      )
      .subscribe(n => {

        // the code below tries to minimize waste if the user opens the system
        // on many tabs at the same time, by checking if the 'nonce' has changed
        // it means only one of the tabs will be refreshing the access token

        const nonce_key = 'nonce';
        const storageNonce = this.storage.getItem(nonce_key);
        const localNonce = this.nonce;

        // always refresh first time you open, then refresh
        // periodically if none of the other tabs has refreshed already
        if (n === 0 || storageNonce === localNonce) {
          this.refreshSilently().subscribe(
            _ => {
              this.nonce = this.storage.getItem(nonce_key);
            }, _ => {
              this.nonce = this.storage.getItem(nonce_key);
            });
        }

        this.nonce = this.storage.getItem(nonce_key);
      });
  }

  private get isDiscoveryDocumentNeeded(): boolean {
    return !authConfig.jwks || !authConfig.loginUrl || !authConfig.logoutUrl || !authConfig.sessionCheckIFrameUrl;
  }

  public get isSignedIn$(): Observable<boolean> {

    // this method performs 2 checks in sequence
    // if both fail then the user is not signed in

    // (1) check if the user already has a valid token in memory
    if (this.isAuthenticated) {
      return of(true);
    } else {

      // (2) Check that the user has an active session with the identity server
      const obs$ =
        this.discoveryDocumentLoaded$.pipe(
          flatMap(isLoaded => {
            if (!isLoaded) {
              return of(false);
            } else {
              return from(this.oauth.silentRefresh().catch(_ => false))
                .pipe(map(_ => this.isAuthenticated));
            }
          })
        );

      return obs$;
    }
  }

  public parseUrlToken(): Observable<boolean> {

    // this method attempts to load a valid token from the URL
    // NOT redirect the user to the identity server, for that
    // we use 'initImplicitFlow' method

    const obs$ = this.discoveryDocumentLoaded$.pipe(
      flatMap(_ => from(this.oauth.tryLogin()))
    );

    return obs$;
  }

  public get state(): string {
    return this.oauth.state;
  }

  public initImplicitFlow(returnUrl: string = null) {
    this.oauth.initImplicitFlow(returnUrl);
  }

  public refreshSilently(): Observable<OAuthEvent> {
    return this.discoveryDocumentLoaded$.pipe(
      flatMap(_ => from(this.oauth.silentRefresh()))
    );
  }

  public signOut() {
    // when the user requests a sign out, it is important to
    // clear the state of the application for security, especially localstorage

    // clean the app state (keep the id token since it is needed for the subsequent logout)
    const id_token = this.storage.getItem('id_token');
    this.storage.removeItem('access_token'); // to trigger storage event more reliably
    this.storage.clear();
    this.storage.setItem('id_token', id_token);

    // go to identity server and sign out from there
    this.oauth.logOut();
  }

  public get email(): string {
    const claims = this.oauth.getIdentityClaims();
    if (!claims) {
      return null;
    }
    return claims['email'];
  }

  public get isAuthenticated(): boolean {
    return this.oauth.hasValidAccessToken();
  }
}
