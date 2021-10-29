import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-notification-templates-master',
  templateUrl: './notification-templates-master.component.html',
  styles: [
  ]
})
export class NotificationTemplatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.NotificationTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
