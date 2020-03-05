import { Component, Input } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { AdminUserForSave, AdminUser } from '~/app/data/entities/admin-user';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { ActivatedRoute } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ADMIN_VIEWS_BUILT_IN, ACTIONS } from '~/app/data/views';
import { AdminPermission } from '~/app/data/entities/admin-permission';

interface ConcreteViewInfo {
  name: () => string;
  actions: { [action: string]: { supportsCriteria: boolean } };
}

@Component({
  selector: 't-admin-users-details',
  templateUrl: './admin-users-details.component.html',
  styles: []
})
export class AdminUsersDetailsComponent extends DetailsBaseComponent {

  @Input()
  showRoles = true;

  public expand = 'Permissions';

  private adminUsersApi = this.api.adminUsersApi(this.notifyDestruct$); // for intellisense
  private _permissionActionChoices: { [view: string]: SelectorChoice[] } = {};
  private _viewsDb: { [view: string]: ConcreteViewInfo } = null;
  private _currentLang: string;
  private _currentViewsDb: { [view: string]: ConcreteViewInfo } = null;
  private _viewsForSelector: SelectorChoice[] = null;


  create = () => {
    const result: AdminUserForSave = { };
    result.Name = this.initialText;
    result.Permissions = [];

    return result;
  }

  clone: (item: AdminUser) => AdminUser = (item: AdminUser) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as AdminUser;
      clone.Id = null;

      if (!!clone.Permissions) {
        clone.Permissions.forEach(e => {
          e.Id = null;
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    public workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private route: ActivatedRoute) {
    super();

    this.adminUsersApi = this.api.adminUsersApi(this.notifyDestruct$);
  }

  public onInvite = (model: AdminUser): void => {
    if (!!model && !!model.Id) {
      this.adminUsersApi.invite(model.Id).subscribe(() => {
        this.details.displayModalMessage(this.translate.instant('InvitationEmailSent'));
      }, this.details.handleActionError);
    }
  }
  public showInvite = (model: AdminUser) => !!model && !model.ExternalId;

  public canInvite = (model: AdminUser) => this.ws.canDo('admin-users', 'ResendInvitationEmail', model.Id);
  public inviteTooltip = (model: AdminUser) => this.canInvite(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public onActivate = (model: AdminUser): void => {
    if (!!model && !!model.Id) {
      this.adminUsersApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: AdminUser): void => {
    if (!!model && !!model.Id) {
      this.adminUsersApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: AdminUser) => !!model && !model.IsActive;
  public showDeactivate = (model: AdminUser) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: AdminUser) => this.ws.canDo('admin-users', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: AdminUser) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')


  public get ws() {
    return this.workspace.admin;
  }

  public showInvitationInfo(model: AdminUserForSave): boolean {
    return !!model && !!model.Email && this.isNew;
  }

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }

  showPermissionsError(model: AdminUserForSave) {
    return !!model && !!model.Permissions && model.Permissions.some(r => !!r.serverErrors);
  }

  public get viewsDb(): { [view: string]: ConcreteViewInfo } {

    if (this._currentLang !== this.translate.currentLang) {
      this._currentLang = this.translate.currentLang;

      this._viewsDb = {};
      for (const view of Object.keys(ADMIN_VIEWS_BUILT_IN)) {
        const viewInfo = ADMIN_VIEWS_BUILT_IN[view];
        const concreteView: ConcreteViewInfo = {
          name: () => this.translate.instant(viewInfo.name),
          actions: {}
        };

        concreteView.actions.All = { supportsCriteria: false };

        if (viewInfo.read) {
          concreteView.actions.Read = { supportsCriteria: true };
        }

        if (viewInfo.update) {
          concreteView.actions.Update = { supportsCriteria: true };
        }

        if (viewInfo.delete) {
          concreteView.actions.Delete = { supportsCriteria: true };
        }

        for (const action of viewInfo.actions) {
          concreteView.actions[action.action] = { supportsCriteria: action.criteria };
        }

        this._viewsDb[view] = concreteView;
      }
    }

    return this._viewsDb;
  }

  get permissionViewChoices(): SelectorChoice[] {

    if (this._currentViewsDb !== this.viewsDb) {
      const db = this.viewsDb;
      this._currentViewsDb = db;

      this._viewsForSelector = Object.keys(db).map(e => ({ value: e, name: db[e].name }));

      // Sort alphabetically
      this._viewsForSelector.sort((a, b) => a.name() < b.name() ? -1 : a.name() > b.name() ? 1 : 0);
    }

    return this._viewsForSelector;
  }

  permissionViewLookup(view: string): string {
    if (!view) {
      return '';
    } else if (!this.viewsDb[view]) {
      console.error(`missing view ${view}`);
      return '';
    } else {
      return this.viewsDb[view].name();
    }
  }

  public onPermissionChanged(item: AdminPermission) {
    // Here we clear away fields that are not compatible with other field values
    const choices = this.permissionActionChoices(item);
    if (choices.length === 1) {
      item.Action = choices[0].value;
    } else if (!!item.Action && choices.every(e => e.value !== item.Action)) {
      item.Action = null;
    }

    if (this.disableCriteria(item)) {
      item.Criteria = null;
    }
  }

  public disableCriteria(item: AdminPermission): boolean {
    if (!item || !item.View || !item.Action) {
      return true;
    }
    const view = this.viewsDb[item.View];
    if (!!view && !!view.actions) {
      const viewAction = view.actions[item.Action];
      return !(viewAction && viewAction.supportsCriteria);
    } else {
      return true;
    }
  }

  permissionActionChoices(item: AdminPermission): SelectorChoice[] {
    if (!item.View) {
      return [];
    }

    // Returns the permission actions only permitted by the specified view
    if (!this._permissionActionChoices[item.View]) {
      const view = this.viewsDb[item.View];
      if (!!view && !!view.actions) {
        this._permissionActionChoices[item.View] =
          Object.keys(view.actions).map(e => ({ name: () => this.translate.instant(ACTIONS[e]), value: e }));
      } else {
        this._permissionActionChoices[item.View] = [];
      }
    }

    return this._permissionActionChoices[item.View];
  }

  public permissionActionLookup(value: string): string {
    if (!value) {
      return '';
    }

    if (value === 'All') {
      return 'View_All';
    }

    return ACTIONS[value];
  }
}
