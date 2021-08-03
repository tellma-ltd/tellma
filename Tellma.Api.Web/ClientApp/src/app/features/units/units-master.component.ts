import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { metadata_Unit } from '~/app/data/entities/unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-units-master',
  templateUrl: './units-master.component.html'
})
export class UnitsMasterComponent extends MasterBaseComponent {

  private unitsApi = this.api.unitsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.unitsApi = this.api.unitsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.Unit;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public unitTypeLookup(value: string): string {
    const descriptor = metadata_Unit(this.workspace, this.translate).properties.UnitType as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.unitsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.unitsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('units', 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
