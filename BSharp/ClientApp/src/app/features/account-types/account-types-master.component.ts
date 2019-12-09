import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 'b-account-types-master',
  templateUrl: './account-types-master.component.html',
  styles: []
})
export class AccountTypesMasterComponent extends MasterBaseComponent {

  public expand = 'Parent';

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
