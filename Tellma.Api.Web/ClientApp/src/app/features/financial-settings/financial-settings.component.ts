import { Component } from '@angular/core';
import { TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';
import { SettingsBaseComponent } from '~/app/shared/settings-base/settings-base';

@Component({
  selector: 't-financial-settings',
  templateUrl: './financial-settings.component.html',
  styles: []
})
export class FinancialSettingsComponent extends SettingsBaseComponent {

  constructor(private workspace: WorkspaceService) {
    super();
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }
}
