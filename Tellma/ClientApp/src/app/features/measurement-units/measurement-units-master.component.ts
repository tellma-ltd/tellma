import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { metadata_MeasurementUnit } from '~/app/data/entities/measurement-unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-measurement-units-master',
  templateUrl: './measurement-units-master.component.html'
})
export class MeasurementUnitsMasterComponent extends MasterBaseComponent {

  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.MeasurementUnit;
  }

  public get ws() {
    return this.workspace.current;
  }

  public unitTypeLookup(value: string): string {
    const descriptor = metadata_MeasurementUnit(this.ws, this.translate, null).properties.UnitType as ChoicePropDescriptor;
    return descriptor.format(value);
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

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('measurement-units', 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
