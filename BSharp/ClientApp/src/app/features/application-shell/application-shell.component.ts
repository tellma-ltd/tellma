import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { AuthService } from '~/app/data/auth.service';
import { appsettings } from '~/app/data/global-resolver.guard';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { NavigationService } from '~/app/data/navigation.service';
import { Subscription } from 'rxjs';
import { StorageService } from '~/app/data/storage.service';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html'
})
export class ApplicationShellComponent implements OnInit, OnDestroy {

  // For the menu on small screens
  private _subscription: Subscription;
  public isCollapsed = true;

  constructor(
    public workspace: WorkspaceService, public nav: NavigationService,
    private translate: TranslateService, private progress: ProgressOverlayService,
    private auth: AuthService, private storage: StorageService,
    @Inject(DOCUMENT) private document: Document) {
  }

  ngOnInit() {
    ////// These ensures that once in a company, the language is
    ////// restricted to one of the company's languages
    this.restrictToCompanyLanguages();
    this._subscription = this.workspace.stateChanged$
      .subscribe(() => this.restrictToCompanyLanguages()); // In case settings changed
  }

  ngOnDestroy() {
    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }

    ////// when we exit a company, reset the language how it was
    // IMPORTANT: also in root.component.ts, keep in sync
    const defaultCulture = this.document.documentElement.lang || 'en';
    const userCulture = this.storage.getItem('user_culture') || defaultCulture;
    this.translate.use(userCulture);
  }

  private restrictToCompanyLanguages() {
    const primary = this.settings.PrimaryLanguageId;
    const secondary = this.settings.SecondaryLanguageId;
    const ternary = this.settings.TernaryLanguageId;
    const current = this.translate.currentLang;

    if (current !== primary && current !== secondary && current !== ternary) {
      const preferred = this.ws.userSettings.PreferredLanguage;
      if (!!preferred && (preferred === primary || preferred === secondary || preferred === ternary)) {
        this.translate.use(preferred);
      } else {
        // The user's preferred language is not one of the company languages
        this.translate.use(primary);
      }
    }
  }

  private onSetLanguage(lang: string) {
    this.translate.use(lang);
    this.storage.setItem('user_culture', lang);

    // TODO: Set preferred langauge
  }

  // UI Binding

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }

  onPrimary() {
    this.onSetLanguage(this.settings.PrimaryLanguageId);
  }

  onSecondary() {
    this.onSetLanguage(this.settings.SecondaryLanguageId);
  }

  onTernary() {
    this.onSetLanguage(this.settings.TernaryLanguageId);
  }

  get settings(): SettingsForClient {
    return !!this.workspace.current ? this.workspace.current.settings : null;
  }

  get ws(): TenantWorkspace {
    return this.workspace.current;
  }

  get userName(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.userSettings, 'Name');
  }
  get companyName(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.settings, 'ShortCompanyName');
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  public onMyCompanyAccount(): void {
    alert('To be implemented');
  }

  public onMySystemAccount(): void {
    // TODO make these pages part of the SPA
    location.href = appsettings.identityAddress + '/identity/manage/';
  }

  public onSignOut(): void {
    this.progress.startAsyncOperation('sign_out', 'RedirectingToSignOut');
    this.auth.signOut();
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.isRtl ? 'horizontal' : null;
  }
}
