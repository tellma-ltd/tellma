import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-admin-main-menu',
  templateUrl: './admin-main-menu.component.html',
  styles: []
})
export class AdminMainMenuComponent implements OnInit {

  public mainMenuItems = [
    { label: () => this.translate.instant('AdminUsers'), icon: 'users', view: 'admin-users', link: '../admin-users' },
    {
      label: () => this.translate.instant('IdentityServerUsers'),
      icon: 'shield-alt', view: 'identity-server-users', link: '../identity-server-users'
    }
  ];

  constructor(private workspace: WorkspaceService, private translate: TranslateService) { }

  ngOnInit() {
  }

  public canView(view: string) {
    return this.workspace.admin.canRead(view);
  }
}
