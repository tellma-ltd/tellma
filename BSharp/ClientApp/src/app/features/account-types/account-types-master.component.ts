import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-account-types-master',
  templateUrl: './account-types-master.component.html',
  styles: []
})
export class AccountTypesMasterComponent extends MasterBaseComponent {


  private accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$);
  }

  private get view(): string {
    return `account-types`;
  }

  // UI Binding

  public get c() {
    return this.workspace.current.AccountType;
  }

  public get ws() {
    return this.workspace.current;
  }
  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountTypesApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountTypesApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.view, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

}
