import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { SettingsForClient } from '~/app/data/dto/settings';

@Component({
  selector: 'b-application-shell',
  templateUrl: './application-shell.component.html'
})
export class ApplicationShellComponent implements OnInit {

  // For the menu on small screens
  public isCollapsed = true;

  constructor(public workspace: WorkspaceService, private translate: TranslateService) {
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
  onPrimary() {
    const lang = this.settings.PrimaryLanguageId;
    this.translate.use(lang);
  }

  // TODO Remove
  onSecondary() {
    const lang = this.settings.SecondaryLanguageId;
    this.translate.use(lang);
  }

  get settings(): SettingsForClient {
return this.workspace.current.settings;
  }
}
