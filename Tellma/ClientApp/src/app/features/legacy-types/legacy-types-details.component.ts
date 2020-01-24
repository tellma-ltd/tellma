import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 'b-legacy-types-details',
  templateUrl: './legacy-types-details.component.html'
})
export class LegacyTypesDetailsComponent extends DetailsBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get ws() {
    return this.workspace.current;
  }
}
