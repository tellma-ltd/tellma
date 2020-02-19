import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-application-page-not-found',
  templateUrl: './application-page-not-found.component.html'
})
export class ApplicationPageNotFoundComponent {

  constructor(private router: Router, private workspace: WorkspaceService) { }


  public onHome() {
    const tenantId = this.workspace.ws.tenantId;
    this.router.navigate(['app', tenantId + '', 'main-menu']);
  }
}
