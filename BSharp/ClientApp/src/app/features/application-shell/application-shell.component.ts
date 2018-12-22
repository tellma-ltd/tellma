import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html',
  styleUrls: ['./application-shell.component.css']
})
export class ApplicationShellComponent implements OnInit {

  // For the menu
  public isCollapsed = true;

  constructor(public workspace: WorkspaceService, private route: ActivatedRoute) {
    this.route.paramMap.subscribe(e => {

      let tenantIdSring = e.get('tenantId');
      if (!!tenantIdSring) {
        let tenantId = +tenantIdSring;
        if (!!tenantId) {
          workspace.ws.tenantId = tenantId;
        }
      }
    });
  }

  ngOnInit() {
  }

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }
}
