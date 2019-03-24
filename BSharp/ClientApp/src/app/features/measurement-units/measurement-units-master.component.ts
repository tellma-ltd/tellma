import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { MeasurementUnit_UnitType } from '~/app/data/dto/measurement-unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';

@Component({
  selector: 'b-measurement-units-master',
  templateUrl: './measurement-units-master.component.html',
  styleUrls: ['./measurement-units-master.component.scss']
})
export class MeasurementUnitsMasterComponent extends MasterBaseComponent {

  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.MeasurementUnits;
  }

  public get ws() {
    return this.workspace.current;
  }

  public unitTypeLookup(value: string): string {
    return MeasurementUnit_UnitType[value];
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.measurementUnitsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.measurementUnitsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
