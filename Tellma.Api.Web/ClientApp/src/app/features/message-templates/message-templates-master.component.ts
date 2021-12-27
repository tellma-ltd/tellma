import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 't-message-templates-master',
  templateUrl: './message-templates-master.component.html',
  styles: [
  ]
})
export class MessageTemplatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.ws.MessageTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
