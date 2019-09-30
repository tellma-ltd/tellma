import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-account-classifications-master',
  templateUrl: './account-classifications-master.component.html',
  styles: []
})
export class AccountClassificationsMasterComponent extends MasterBaseComponent {

  private accountClassificationApi = this.api.accountClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.accountClassificationApi = this.api.accountClassificationsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.AccountClassification;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountClassificationApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeprecate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountClassificationApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeprecateItem = (_: (number | string)[]) => this.ws.canDo('account-classifications', 'IsDeprecated', null);

  public activateDeprecateTooltip = (ids: (number | string)[]) => this.canActivateDeprecateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
