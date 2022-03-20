import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-email-templates-master',
  templateUrl: './email-templates-master.component.html',
  styles: [
  ]
})
export class EmailTemplatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.EmailTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
