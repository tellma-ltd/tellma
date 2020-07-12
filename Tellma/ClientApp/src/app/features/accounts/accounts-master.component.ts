import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-accounts-master',
  templateUrl: './accounts-master.component.html',
  styles: []
})
export class AccountsMasterComponent extends MasterBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) {
    super();

    this.accountsApi = this.api.accountsApi(this.notifyDestruct$);
  }

  private get view(): string {
    return `accounts`;
  }

  // UI Binding

  public get c() {
    return this.workspace.currentTenant.Account;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountsApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountsApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.view, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
