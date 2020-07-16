import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-application-page-not-found',
  templateUrl: './application-page-not-found.component.html'
})
export class ApplicationPageNotFoundComponent {

  /**
   * In popup mode we hide the home button as it won't work properly and is pointless anyways
   */
  @Input()
  showHome = true;

  constructor(private router: Router, private workspace: WorkspaceService) { }

  public onHome() {
    const tenantId = this.workspace.ws.tenantId;
    this.router.navigate(['app', tenantId + '', 'main-menu']);
  }
}
