import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-emails-master',
  templateUrl: './emails-master.component.html',
  styles: []
})
export class EmailsMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.EmailForQuery;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
