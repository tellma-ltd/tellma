import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService, AdminWorkspace } from '~/app/data/workspace.service';

@Component({
  selector: 't-identity-server-users-master',
  templateUrl: './identity-server-users-master.component.html',
  styles: []
})
export class IdentityServerUsersMasterComponent  extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c()  {
    return this.ws.IdentityServerUser;
  }

  public get ws(): AdminWorkspace {
    return this.workspace.admin;
  }
}
