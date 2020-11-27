import { Component, OnDestroy, OnInit } from '@angular/core';
import { TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-financial-settings',
  templateUrl: './financial-settings.component.html',
  styles: []
})
export class FinancialSettingsComponent implements OnInit, OnDestroy {

  constructor(private workspace: WorkspaceService) {
  }

  ngOnInit() {
  }

  ngOnDestroy() {
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }
}
