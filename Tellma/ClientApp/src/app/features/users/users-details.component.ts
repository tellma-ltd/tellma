import { Component, Input } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { User, UserForSave } from '~/app/data/entities/user';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { supportedCultures } from '~/app/data/supported-cultures';

@Component({
  selector: 't-users-details',
  templateUrl: './users-details.component.html'
})
export class UsersDetailsComponent extends DetailsBaseComponent {

  @Input()
  showRoles = true;

  private _languageChoices: SelectorChoice[];
  private usersApi = this.api.usersApi(this.notifyDestruct$); // for intellisense

  public expand = 'Roles/Role';

  create = () => {
    const result: UserForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.PreferredLanguage = this.ws.settings.PrimaryLanguageId;

    result.Roles = [];
    return result;
  }

  clone: (item: User) => User = (item: User) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as User;
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

  constructor(
    public workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private route: ActivatedRoute) {
    super();

    this.usersApi = this.api.usersApi(this.notifyDestruct$);
  }

  get languageChoices(): SelectorChoice[] {

    if (!this._languageChoices) {
      this._languageChoices = [{ name: () => this.ws.settings.PrimaryLanguageName, value: this.ws.settings.PrimaryLanguageId }];
      if (!!this.ws.settings.SecondaryLanguageId) {
        this._languageChoices.push({
          name: () => this.ws.settings.SecondaryLanguageName,
          value: this.ws.settings.SecondaryLanguageId
        });
      }
      if (!!this.ws.settings.TernaryLanguageId) {
        this._languageChoices.push({
          name: () => this.ws.settings.TernaryLanguageName,
          value: this.ws.settings.TernaryLanguageId
        });
      }
    }

    return this._languageChoices;
  }

  public languageLookup(value: string) {
    return supportedCultures[value];
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

  public onActivate = (model: User): void => {
    if (!!model && !!model.Id) {
      this.usersApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: User): void => {
    if (!!model && !!model.Id) {
      this.usersApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: User) => !!model && !model.IsActive;
  public showDeactivate = (model: User) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: User) => this.ws.canDo('users', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: User) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')


  public get ws() {
    return this.workspace.currentTenant;
  }

  showRolesError(model: User) {
    return !!model && !!model.Roles && model.Roles.some(r => !!r.serverErrors);
  }

  public showInvitationInfo(model: UserForSave): boolean {
    return !!model && !!model.Email && this.isNew;
  }

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }

  public get showPreferredLanguage(): boolean {
    return !!this.ws.settings.SecondaryLanguageId || !!this.ws.settings.TernaryLanguageId;
  }
}
