import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-report-definitions-master',
  templateUrl: './report-definitions-master.component.html',
  styles: []
})
export class ReportDefinitionsMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.ReportDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
