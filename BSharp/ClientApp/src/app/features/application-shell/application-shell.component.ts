import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { SettingsForClient } from '~/app/data/entities/settings';
import { AuthService } from '~/app/data/auth.service';
import { appconfig } from '~/app/data/appconfig';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { NavigationService } from '~/app/data/navigation.service';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html'
})
export class ApplicationShellComponent implements OnInit {

  // For the menu on small screens
  public isCollapsed = true;

  constructor(
    public workspace: WorkspaceService, public nav: NavigationService,
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

  onPrimary() {
    const lang = this.settings.PrimaryLanguageId;
    this.translate.use(lang);
  }

  onSecondary() {
    const lang = this.settings.SecondaryLanguageId;
    this.translate.use(lang);
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

  get myAccountDropdownPlacement() {
    return this.isRtl ? 'bottom-left' : 'bottom-right';
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  public onMyCompanyAccount(): void {
    alert('To be implemented');
  }

  public onMySystemAccount(): void {
    // TODO make these pages part of the SPA
    location.href = appconfig.identityAddress + '/identity/manage/';
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
