import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-custom-classifications-master',
  templateUrl: './custom-classifications-master.component.html',
  styles: []
})
export class CustomClassificationsMasterComponent extends MasterBaseComponent {

  private customClassificationApi = this.api.customClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.customClassificationApi = this.api.customClassificationsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.CustomClassification;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.customClassificationApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeprecate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.customClassificationApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeprecateItem = (_: (number | string)[]) => this.ws.canDo('custom-classifications', 'IsDeprecated', null);

  public activateDeprecateTooltip = (ids: (number | string)[]) => this.canActivateDeprecateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
