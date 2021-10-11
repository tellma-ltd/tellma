import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-printing-templates-master',
  templateUrl: './printing-templates-master.component.html',
  styles: []
})
export class PrintingTemplatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.PrintingTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
