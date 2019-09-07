import { Component, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { User, UserForSave } from '~/app/data/entities/user';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-users-details',
  templateUrl: './users-details.component.html',
  styleUrls: ['./users-details.component.scss']
})
export class UsersDetailsComponent extends DetailsBaseComponent {

  @Input()
  showRoles = true;

  private notifyDestruct$ = new Subject<void>();
  private usersApi = this.api.usersApi(this.notifyDestruct$); // for intellisense

  public expand = 'Roles/Role,Agent';

  create = () => {
    const result = new UserForSave();
    result.Id = parseInt(this.route.snapshot.paramMap.get('agent_id'), null) || null;
    result.Email = this.initialText;
    result.Roles = [];
    return result;
  }

  clone: (item: User) => User = (item: User) => {
    if (!!item) {
      const clone = <User>JSON.parse(JSON.stringify(item));
      clone.Id = null;

      if (!!clone.Roles) {
        clone.Roles.forEach(e => {
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

  constructor(public workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private route: ActivatedRoute) {
    super();

    this.usersApi = this.api.usersApi(this.notifyDestruct$);
  }

  public onActivate = (model: User): void => {
    if (!!model && !!model.Id) {
      this.usersApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: User): void => {
    if (!!model && !!model.Id) {
      this.usersApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onInvite = (model: User): void => {
    if (!!model && !!model.Id) {
      this.usersApi.invite(model.Id).subscribe(() => {
        this.details.displayModalMessage(this.translate.instant('InvitationEmailSent'));
      }, this.details.handleActionError);
    }
  }
  public showInvite = (model: User) => !!model && !model.ExternalId;

  public canInvite = (model: User) => this.ws.canDo('users', 'ResendInvitationEmail', model.Id);
  public inviteTooltip = (model: User) => this.canInvite(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }

  showRolesError(model: User) {
    return !!model && !!model.Roles && model.Roles.some(r => !!r.serverErrors);
  }

  public showInvitationInfo(model: UserForSave): boolean {
    return !!model && !!model.Email && this.isNew;
  }

  isInactive: (model: User) => string = (model: User) => !!model && !!model.Id && !!this.ws.Agent[model.Id] &&
    !this.ws.Agent[model.Id].IsActive ? 'Error_CannotModifyInactiveItemPleaseActivate' : null

  public get isNew(): boolean {
    return this.route.snapshot.paramMap.get('id') === 'new';
  }
}
