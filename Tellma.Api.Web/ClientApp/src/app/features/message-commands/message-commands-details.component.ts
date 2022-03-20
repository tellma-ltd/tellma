import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 't-message-commands-details',
  templateUrl: './message-commands-details.component.html',
  styles: [
  ]
})
export class MessageCommandsDetailsComponent extends DetailsBaseComponent {

  private messageCommandsApi = this.api.emailCommandsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Template';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.messageCommandsApi = this.api.emailCommandsApi(this.notifyDestruct$);
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }
}
