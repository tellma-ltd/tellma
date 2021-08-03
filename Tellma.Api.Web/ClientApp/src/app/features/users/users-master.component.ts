// tslint:disable:member-ordering
import { Component, TemplateRef } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { GlobalSettingsForClient } from '~/app/data/dto/global-settings';

@Component({
  selector: 't-users-master',
  templateUrl: './users-master.component.html'
})
export class UsersMasterComponent extends MasterBaseComponent {

  private usersApi = this.api.usersApi(this.notifyDestruct$); // for intellisense

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.usersApi = this.api.usersApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.User;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onInvite = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.usersApi.invite(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.usersApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.usersApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('users', 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public canInvite = (_: (number | string)[]) => this.ws.canDo('users', 'SendInvitationEmail', null);

  public inviteTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public showInvite = (_: (number | string)[]) => this.workspace.globalSettings.CanInviteUsers;

  private _globalSettings: GlobalSettingsForClient;
  private _filterStateFiltersInput: { template: TemplateRef<any>, expression: string }[];
  private _filterStateFiltersResult: { template: TemplateRef<any>, expression: string }[];

  public filterStateFilters(arr: { template: TemplateRef<any>, expression: string }[]): any[] {
    const globalSettings = this.workspace.globalSettings;
    if (this._filterStateFiltersInput !== arr || this._globalSettings !== globalSettings) {
      this._filterStateFiltersInput = arr;

      if (globalSettings.CanInviteUsers) {
        this._filterStateFiltersResult = arr;
      } else if (!!arr) {
        this._filterStateFiltersResult = arr.filter(e => e.expression !== 'State eq 1');
      } else {
        delete this._filterStateFiltersResult;
      }
    }

    return this._filterStateFiltersResult;
  }
}
