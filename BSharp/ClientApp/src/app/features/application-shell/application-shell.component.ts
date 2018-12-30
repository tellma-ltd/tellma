import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html',
  styleUrls: ['./application-shell.component.css']
})
export class ApplicationShellComponent implements OnInit {

  // For the menu on small screens
  public isCollapsed = true;

  constructor(public workspace: WorkspaceService, private route: ActivatedRoute, private translate: TranslateService) {

    this.route.paramMap.subscribe(e => {
      const tenantIdSring = e.get('tenantId');
      if (!!tenantIdSring) {
        const tenantId = +tenantIdSring;
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

  // TODO Remove
  onEnglish() {

    this.translate.use('en');
  }

  // TODO Remove
  onArabic() {

    this.translate.use('ar');
  }
}
