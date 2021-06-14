import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 't-ifrs-concepts-details',
  templateUrl: './ifrs-concepts-details.component.html',
  styles: []
})
export class IfrsConceptsDetailsComponent extends DetailsBaseComponent {

  public expand = '';

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
