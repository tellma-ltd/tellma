import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
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
    return this.workspace.current.ReportDefinition;
  }

  public get ws() {
    return this.workspace.current;
  }
}
