import { Component, OnInit } from '@angular/core';
import { AdminWorkspace, WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-identity-server-clients-master',
  templateUrl: './identity-server-clients-master.component.html',
  styles: [
  ]
})
export class IdentityServerClientsMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.IdentityServerClient;
  }

  public get ws(): AdminWorkspace {
    return this.workspace.admin;
  }
}
