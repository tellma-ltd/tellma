import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 'b-legacy-types-master',
  templateUrl: './legacy-types-master.component.html'
})
export class LegacyTypesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get c() {
    return this.workspace.current.AccountType;
  }

  public get ws() {
    return this.workspace.current;
  }
}
