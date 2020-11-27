import { Component, OnInit, OnDestroy } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { supportedCultures } from '~/app/data/supported-cultures';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-general-settings',
  templateUrl: './general-settings.component.html'
})
export class GeneralSettingsComponent implements OnInit, OnDestroy {

  private _cultures: SelectorChoice[];

  constructor(private workspace: WorkspaceService) {
  }

  ngOnInit() {
  }

  ngOnDestroy() {
  }

  ////////// UI Bindings

  get primaryPostfix(): string {
    return this.workspace.currentTenant.primaryPostfix;
  }

  get secondaryPostfix(): string {
    return this.workspace.currentTenant.secondaryPostfix;
  }

  get ternaryPostfix(): string {
    return this.workspace.currentTenant.ternaryPostfix;
  }

  public cultureName(culture: string): string {
    return supportedCultures[culture];
  }

  get cultures(): SelectorChoice[] {

    if (!this._cultures) {
      this._cultures = Object.keys(supportedCultures)
        .map(key => ({ name: () => supportedCultures[key], value: key }));
    }

    return this._cultures;
  }
}
