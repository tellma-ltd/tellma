import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-messages-master',
  templateUrl: './messages-master.component.html',
  styles: []
})
export class MessagesMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.MessageForQuery;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
