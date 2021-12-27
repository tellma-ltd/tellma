import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-message-commands-master',
  templateUrl: './message-commands-master.component.html',
  styles: [
  ]
})
export class MessageCommandsMasterComponent extends MasterBaseComponent {

  private messageCommandsApi = this.api.messageCommandsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.messageCommandsApi = this.api.messageCommandsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.MessageCommand;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
