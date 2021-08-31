import { Component, Input } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { User, UserForSave } from '~/app/data/entities/user';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute } from '@angular/router';
import { tap } from 'rxjs/operators';
import { addToWorkspace, daysDiff, FriendlyError } from '~/app/data/util';
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

  public expand = 'Roles.Role';

  create = () => {
    const result: UserForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.EmailNewInboxItem = false;
    result.SmsNewInboxItem = false;
    result.PushNewInboxItem = false;
    result.IsService = false;
    result.PreferredLanguage = this.ws.settings.PrimaryLanguageId;

    result.Roles = [];
    return result;
  }

  clone: (item: User) => User = (item: User) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as User;
      delete clone.Id;
      delete clone.State;

      if (!!clone.Roles) {
        clone.Roles.forEach(e => {
          delete e.Id;
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
      this.usersApi.invite([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showInvite = (model: User) => !!model && model.State <= 1 && this.showInvitedState(model);

  public showInvitedState(model: User) {
    return this.workspace.globalSettings.CanInviteUsers && !model.IsService;
  }

  public canInvite = (model: User) => this.ws.canDo('users', 'SendInvitationEmail', model.Id);
  public inviteTooltip = (model: User) => this.canInvite(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public showUserNewNotice(model: User, isEdit: boolean): boolean {
    return !isEdit && !!model && !!model.Id && !model.State && !model.IsService && this.workspace.globalSettings.CanInviteUsers;
  }

  public showUserFreshInvitedNotice(model: User): boolean {
    return !!model && !!model.Id && model.State === 1 &&
      daysDiff(new Date(model.InvitedAt), new Date()) < this.workspace.globalSettings.TokenExpiryInDays;
  }

  public showUserExpiredInvitedNotice(model: User, isEdit: boolean): boolean {
    return !isEdit && !!model && !!model.Id && model.State === 1 &&
      daysDiff(new Date(model.InvitedAt), new Date()) >= this.workspace.globalSettings.TokenExpiryInDays;
  }

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

  public showRolesError(model: User) {
    return !!model && !!model.Roles && model.Roles.some(r => !!r.serverErrors);
  }

  public showNotificationsError(model: User) {
    return !!model && !!model.serverErrors && (
      !!model.serverErrors.PreferredLanguage ||
      !!model.serverErrors.ContactEmail ||
      !!model.serverErrors.ContactMobile ||
      !!model.serverErrors.EmailNewInboxItem ||
      !!model.serverErrors.SmsNewInboxItem ||
      !!model.serverErrors.PushNewInboxItem
    );
  }

  public userName(model: UserForSave): string {
    if (!model) {
      return '';
    }

    return this.ws.localize(model.Name, model.Name2, model.Name3);
  }

  get companyName(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.settings, 'ShortCompanyName');
  }

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }

  public get showPreferredLanguage(): boolean {
    return !!this.ws.settings.SecondaryLanguageId || !!this.ws.settings.TernaryLanguageId;
  }

  public showNotifications(model: User): boolean {
    return !model.IsService && (this.showEmail || this.showSms || this.showPush);
  }

  public showTabs(model: User): boolean {
    return this.showRoles || this.showNotifications(model);
  }

  public get showSms(): boolean {
    return this.ws.settings.SmsEnabled && this.workspace.globalSettings.SmsEnabled;
  }

  public get showEmail(): boolean {
    return this.workspace.globalSettings.EmailEnabled;
  }

  public get showPush(): boolean {
    return this.workspace.globalSettings.PushEnabled;
  }

  public showNotificationTriggers(model: User): boolean {
    return this.showEmailColumn(model) || this.showSmsColumn(model) || this.showPushColumn(model);
  }

  public showEmailColumn(model: User): boolean {
    return this.showEmail && !!model && !!model.ContactEmail;
  }

  public showSmsColumn(model: User): boolean {
    return this.showSms && !!model && !!model.ContactMobile;
  }

  public showPushColumn(model: User): boolean {
    return this.showPush && !!model && !!model.PushEnabled;
  }

  public getActiveTab(model: User): string {
    const tab = this.ws.miscState.users_details_activeTab;
    if (tab === 'notifications' && !model.IsService) {
      return tab;
    }

    return 'roles';
  }

  public setActiveTab(v: string) {
    this.ws.miscState.users_details_activeTab = v;
  }

  public testEmail(email: string): void {
    const details = this.details;
    this.usersApi.testEmail(email).subscribe(
      (msg: { Message: string }) => details.displayModalMessage(msg.Message),
      (friendly: FriendlyError) => details.displayErrorModal(friendly.error)
    );
  }

  public testPhoneNumber(phone: string): void {
    const details = this.details;
    this.usersApi.testPhone(phone).subscribe(
      (msg: { Message: string }) => details.displayModalMessage(msg.Message),
      (friendly: FriendlyError) => details.displayErrorModal(friendly.error)
    );
  }

  public savePreprocessing = (user: UserForSave): void => {
    // We delete hidden fields cause they may trigger structural validation errors
    if (user.IsService) {
      delete user.Email;

      // Notifications tab
      delete user.PreferredLanguage;
      delete user.ContactEmail;
      delete user.ContactMobile;

    } else {
      delete user.ClientId;
    }
  }
}
