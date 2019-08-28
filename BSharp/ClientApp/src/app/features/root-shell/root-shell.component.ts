import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NavigationService } from '~/app/data/navigation.service';
import { TranslateService } from '@ngx-translate/core';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { AuthService } from '~/app/data/auth.service';
import { appconfig } from '~/app/data/appconfig';
import { Culture } from '~/app/data/dto/culture';
import { DOCUMENT } from '@angular/platform-browser';
import { supportedCultures } from '~/app/data/supported-cultures';

@Component({
  selector: 'b-root-shell',
  templateUrl: './root-shell.component.html',
  styleUrls: ['./root-shell.component.scss']
})
export class RootShellComponent implements OnInit, OnDestroy {

  // For the menu on small screens
  public isCollapsed = true;

  private _activeLanguages: Culture[];

  constructor(public workspace: WorkspaceService, public nav: NavigationService, @Inject(DOCUMENT) private document: Document,
    private translate: TranslateService, private progress: ProgressOverlayService, private auth: AuthService) {
  }

  ngOnInit() {

    // this adds a cool background to the main menu, unaffected by scrolling
    this.document.body.classList.add('b-banner');
  }

  ngOnDestroy() {

    // this adds a cool background to the main menu, unaffected by scrolling
    this.document.body.classList.remove('b-banner');
  }

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }

  get userName(): string {
    return this.auth.userName;
  }

  get myAccountDropdownPlacement() {
    return this.isRtl ? 'bottom-left' : 'bottom-right';
  }

  get languageDropdownPlacement() {
    return this.isRtl ? 'bottom-left' : 'bottom-right';
  }

  get activeLanguages() {
    if  (!this._activeLanguages) {
      const keys = Object.keys(supportedCultures);
      this._activeLanguages = keys.map(key => supportedCultures[key]);
    }
    return this._activeLanguages;
  }

  public onSetLanguage(lang: string) {
    this.onCollapse();
    this.translate.use(lang);
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  get currentLanguage(): string {
    const cultureName = this.workspace.ws.culture || this.translate.currentLang || this.translate.defaultLang || 'en';
    return cultureName;
  }

  get currentLanguageDisplay(): string {
    const cultureName = this.currentLanguage;
    const culture = supportedCultures[this.currentLanguage];
    return !!culture ? culture.Name : cultureName;
  }

  public onMySystemAccount(): void {
    // TODO make these pages part of the SPA
    location.href = appconfig.identityAddress + '/identity/manage/';
  }

  public onSignOut(): void {
    // show rotator
    this.progress.startAsyncOperation('sign_out', 'RedirectingToSignOut');

    // clean local state and send the user to identity server
    this.auth.signOut();
  }

  public onSignIn(): void {
    // show rotator
    this.progress.startAsyncOperation('sign_in', 'RedirectingToSignIn');

    // start the OIDC dance with identity server
    this.auth.initImplicitFlow('/root/companies');
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.isRtl ? 'horizontal' : null;
  }

  public get isAuthenticated() {
    return this.auth.isAuthenticated;
  }

}
