import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-details-entries',
  templateUrl: './details-entries.component.html',
  styles: []
})
export class DetailsEntriesComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  public get c() {
    return this.ws.DetailsEntry;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
