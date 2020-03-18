import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Unit, UnitForSave, metadata_Unit } from '~/app/data/entities/unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-units-details',
  templateUrl: './units-details.component.html'
})
export class UnitsDetailsComponent extends DetailsBaseComponent {

  private _unitTypeChoices: SelectorChoice[];
  private unitsApi = this.api.unitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result: UnitForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    result.UnitAmount = 1;
    result.BaseAmount = 1;
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.unitsApi = this.api.unitsApi(this.notifyDestruct$);
  }

  get unitTypeChoices(): SelectorChoice[] {

    if (!this._unitTypeChoices) {
      const descriptor = metadata_Unit(this.workspace, this.translate, null).properties.UnitType as ChoicePropDescriptor;
      this._unitTypeChoices = descriptor.choices.map(c => ({ name: () => descriptor.format(c), value: c }));
    }

    return this._unitTypeChoices;
  }

  public unitTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    const descriptor = metadata_Unit(this.workspace, this.translate, null).properties.UnitType as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: Unit): void => {
    if (!!model && !!model.Id) {
      this.unitsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Unit): void => {
    if (!!model && !!model.Id) {
      this.unitsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Unit) => !!model && !model.IsActive;
  public showDeactivate = (model: Unit) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Unit) => this.ws.canDo('units', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Unit) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
