import { Component, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { LocalUser, LocalUserForSave } from '~/app/data/dto/local-user';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-local-users-details',
  templateUrl: './local-users-details.component.html',
  styleUrls: ['./local-users-details.component.scss']
})
export class LocalUsersDetailsComponent extends DetailsBaseComponent {

  @Input()
  showRoles = true;

  private notifyDestruct$ = new Subject<void>();
  private localUsersApi = this.api.localUsersApi(this.notifyDestruct$); // for intellisense

  public expand = 'Agent,Roles/Role';

  create = () => {
    const result = new LocalUserForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else {
      result.Name2 = this.initialText;
    }
    result.Roles = [];
    return result;
  }

  clone: (item: LocalUser) => LocalUser = (item: LocalUser) => {
    if (!!item) {
      const clone = <LocalUser>JSON.parse(JSON.stringify(item));
      clone.Id = null;
      clone.EntityState = 'Inserted';

      if (!!clone.Roles) {
        clone.Roles.forEach(e => {
          e.Id = null;
          e.EntityState = 'Inserted';
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(public workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.localUsersApi = this.api.localUsersApi(this.notifyDestruct$);
  }

  public onActivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onInvite = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.invite(model.Id).subscribe(() => {
        this.details.displayModalMessage(this.translate.instant('InvitationEmailSent'));
      }, this.details.handleActionError);
    }
  }
  public showActivate = (model: LocalUser) => !!model && !model.IsActive;
  public showDeactivate = (model: LocalUser) => !!model && model.IsActive;
  public showInvite = (model: LocalUser) => !!model && !model.ExternalId;

  public canAction = (model: LocalUser) => this.ws.canUpdate('local-users', model.Id);
  public actionTooltip = (model: LocalUser) => this.canAction(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }

  showRolesError(model: LocalUser) {
    return !!model && !!model.Roles && model.Roles.some(r => !!r.serverErrors);
  }

  public showInvitationInfo(model: LocalUserForSave): boolean {
    return !!model && (!!model.Email && !model.Id);
  }
}
