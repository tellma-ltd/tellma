import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-account-types-details',
  templateUrl: './account-types-details.component.html',
  styles: []
})
export class AccountTypesDetailsComponent extends DetailsBaseComponent {

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private route: ActivatedRoute) {
    super();
  }

  public get ws() {
    return this.workspace.current;
  }

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }
}
