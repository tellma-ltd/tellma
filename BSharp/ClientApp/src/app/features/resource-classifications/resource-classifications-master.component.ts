import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';
import { metadata_ResourceClassification } from '~/app/data/entities/resource-classification';

@Component({
  selector: 'b-resource-classifications-master',
  templateUrl: './resource-classifications-master.component.html'
})
export class ResourceClassificationsMasterComponent extends MasterBaseComponent {

  private resourceClassificationsApi = this.api.resourceClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.resourceClassificationsApi = this.api.resourceClassificationsApi(this.notifyDestruct$);
  }

  private get viewId(): string {
    return `resource-classifications`;
  }

  // UI Binding

  public get c() {
    return this.workspace.current.ResourceClassification;
  }

  public get ws() {
    return this.workspace.current;
  }
  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourceClassificationsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourceClassificationsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.viewId, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
