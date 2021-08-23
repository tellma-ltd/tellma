import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-sms-messages-master',
  templateUrl: './sms-messages-master.component.html',
  styles: []
})
export class SmsMessagesMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.SmsMessageForQuery;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
