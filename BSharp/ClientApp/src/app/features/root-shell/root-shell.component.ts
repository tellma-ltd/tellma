import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NavigationService } from '~/app/data/navigation.service';
import { TranslateService } from '@ngx-translate/core';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { AuthService } from '~/app/data/auth.service';
import { appconfig } from '~/app/data/appconfig';
import { SettingsForClient } from '~/app/data/dto/settings';
import { Culture } from '~/app/data/dto/culture';

@Component({
  selector: 'b-root-shell',
  templateUrl: './root-shell.component.html',
  styleUrls: ['./root-shell.component.scss']
})
export class RootShellComponent implements OnInit {

  // For the menu on small screens
  public isCollapsed = true;

  private _oldActiveLanguages: { [key: string]: Culture };
  private _activeLanguages: Culture[];

  constructor(public workspace: WorkspaceService, public nav: NavigationService,
    private translate: TranslateService, private progress: ProgressOverlayService, private auth: AuthService) {
  }

  ngOnInit() {
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
    const activeCultures = this.workspace.globalSettings.ActiveCultures;
    if  (this._oldActiveLanguages !== activeCultures) {
      const keys = Object.keys(activeCultures);
      this._activeLanguages = keys.map(key => activeCultures[key]);
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
    const culture = this.workspace.globalSettings.ActiveCultures[this.currentLanguage];
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
