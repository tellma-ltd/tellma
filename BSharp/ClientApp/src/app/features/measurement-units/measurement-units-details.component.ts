import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { MeasurementUnit_UnitType, MeasurementUnitForSave, MeasurementUnit } from 'src/app/data/dto/measurement-unit';
import { DetailsBaseComponent } from 'src/app/shared/details-base/details-base.component';
import { tap } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { ApiService } from 'src/app/data/api.service';
import { addToWorkspace } from 'src/app/data/util';

@Component({
  selector: 'b-measurement-units-details',
  templateUrl: './measurement-units-details.component.html',
  styleUrls: ['./measurement-units-details.component.css']
})
export class MeasurementUnitsDetailsComponent extends DetailsBaseComponent {

  create = () => {
    const result = new MeasurementUnitForSave();
    result.UnitAmount = 1;
    result.BaseAmount = 1;
    return result;
  }

  private _unitTypeChoices: { name: string, value: any }[];
  private notifyDestruct$ = new Subject<void>();
  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  constructor(private workspace: WorkspaceService, private api: ApiService) {
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


  public onActivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.activate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: MeasurementUnit): void => {
    if (!!model && !!model.Id) {
      this.measurementUnitsApi.deactivate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: MeasurementUnit) => !!model && !model.IsActive;
  public showDeactivate = (model: MeasurementUnit) => !!model && model.IsActive;
}
