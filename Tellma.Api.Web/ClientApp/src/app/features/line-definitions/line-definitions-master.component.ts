import { Component } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-line-definitions-master',
  templateUrl: './line-definitions-master.component.html',
  styles: []
})
export class LineDefinitionsMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
  }

  public get c() {
    return this.ws.LineDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
