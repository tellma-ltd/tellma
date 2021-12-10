import { Component } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-notification-commands-details',
  templateUrl: './notification-commands-details.component.html',
  styles: [
  ]
})
export class NotificationCommandsDetailsComponent extends DetailsBaseComponent {

  private notificationCommandsApi = this.api.notificationCommandsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Template';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.notificationCommandsApi = this.api.notificationCommandsApi(this.notifyDestruct$);
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }
}
