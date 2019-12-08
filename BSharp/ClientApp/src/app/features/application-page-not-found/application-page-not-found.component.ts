import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 'b-application-page-not-found',
  templateUrl: './application-page-not-found.component.html'
})
export class ApplicationPageNotFoundComponent implements OnInit {

  constructor(private router: Router, private workspace: WorkspaceService) { }

  ngOnInit() {
  }

  public onHome() {
    const tenantId = this.workspace.ws.tenantId;
    this.router.navigate(['app', tenantId + '', 'main-menu']);
  }
}
