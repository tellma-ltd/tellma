import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-ifrs-concepts-master',
  templateUrl: './ifrs-concepts-master.component.html',
  styles: []
})
export class IfrsConceptsMasterComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.IfrsConcept;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
