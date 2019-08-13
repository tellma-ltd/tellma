import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { MeasurementUnit, MeasurementUnitForSave, MeasurementUnit_UnitType } from '~/app/data/dto/measurement-unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-measurement-units-details',
  templateUrl: './measurement-units-details.component.html',
  styleUrls: ['./measurement-units-details.component.scss']
})
export class MeasurementUnitsDetailsComponent extends DetailsBaseComponent {

  private _unitTypeChoices: { name: string, value: any }[];
  private notifyDestruct$ = new Subject<void>();
  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result = new MeasurementUnitForSave();
    result.UnitAmount = 1;
    result.BaseAmount = 1;
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$);
  }

  get unitTypeChoices(): { name: string, value: any }[] {

    if (!this._unitTypeChoices) {
      this._unitTypeChoices = Object.keys(MeasurementUnit_UnitType)
        .filter(e => e !== 'Money').map(
          key => ({ name: MeasurementUnit_UnitType[key], value: key }));
    }

    return this._unitTypeChoices;
  }

  public unitTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    return MeasurementUnit_UnitType[value];
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: MeasurementUnit) => !!model && !model.IsActive;
  public showDeactivate = (model: MeasurementUnit) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: MeasurementUnit) => this.ws.canDo('measurement-units', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: MeasurementUnit) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
