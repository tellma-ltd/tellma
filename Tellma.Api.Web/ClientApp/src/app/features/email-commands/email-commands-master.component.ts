import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-email-commands-master',
  templateUrl: './email-commands-master.component.html',
  styles: [
  ]
})
export class EmailCommandsMasterComponent extends MasterBaseComponent {

  private emailCommandsApi = this.api.emailCommandsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.emailCommandsApi = this.api.emailCommandsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.EmailCommand;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
