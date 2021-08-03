import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-dashboard-definitions-master',
  templateUrl: './dashboard-definitions-master.component.html',
  styles: []
})
export class DashboardDefinitionsMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.DashboardDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
