import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { MeasurementUnit, MeasurementUnitForSave, metadata_MeasurementUnit } from '~/app/data/entities/measurement-unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService, TenantWorkspace } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-measurement-units-details',
  templateUrl: './measurement-units-details.component.html'
})
export class MeasurementUnitsDetailsComponent extends DetailsBaseComponent {

  private _unitTypeChoices: SelectorChoice[];
  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result: MeasurementUnitForSave = { };
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

    this.measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$);
  }

  get unitTypeChoices(): SelectorChoice[] {

    if (!this._unitTypeChoices) {
      const descriptor = metadata_MeasurementUnit(this.workspace, this.translate, null).properties.UnitType as ChoicePropDescriptor;
      this._unitTypeChoices = descriptor.choices.map(c => ({ name: () => descriptor.format(c), value: c }));
    }

    return this._unitTypeChoices;
  }

  public unitTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    const descriptor = metadata_MeasurementUnit(this.workspace, this.translate, null).properties.UnitType as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: MeasurementUnit) => !!model && !model.IsActive;
  public showDeactivate = (model: MeasurementUnit) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: MeasurementUnit) => this.ws.canDo('measurement-units', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: MeasurementUnit) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
