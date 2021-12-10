import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-notification-commands-master',
  templateUrl: './notification-commands-master.component.html',
  styles: [
  ]
})
export class NotificationCommandsMasterComponent extends MasterBaseComponent {

  private notificationCommandsApi = this.api.notificationCommandsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.notificationCommandsApi = this.api.notificationCommandsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.NotificationCommand;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
