import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-markup-templates-master',
  templateUrl: './markup-templates-master.component.html',
  styles: []
})
export class MarkupTemplatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.MarkupTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
