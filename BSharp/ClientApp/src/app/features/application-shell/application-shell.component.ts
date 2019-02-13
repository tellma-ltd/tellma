import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { SettingsForClient } from '~/app/data/dto/settings';
import { UserSettingsForClient } from '~/app/data/dto/local-user';
import { AuthService } from '~/app/data/auth.service';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html'
})
export class ApplicationShellComponent implements OnInit {

  // For the menu on small screens
  public isCollapsed = true;

  constructor(public workspace: WorkspaceService, private translate: TranslateService, private auth: AuthService) {
  }

  ngOnInit() {
  }

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }

  // TODO Remove
  onPrimary() {
    const lang = this.settings.PrimaryLanguageId;
    this.translate.use(lang);
  }

  // TODO Remove
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
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public onMyAccount(): void {
    alert('To be implemented');
  }

  public onSignOut(): void {
    this.auth.signOut();
  }

  public onSignIn(): void {
    this.auth.initImplicitFlow();
  }
}
