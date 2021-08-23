import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { IdentityServerUser } from '~/app/data/entities/identity-server-user';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ResetPasswordArgs } from '~/app/data/dto/reset-password-args';
import { tap, catchError } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 't-identity-server-users-details',
  templateUrl: './identity-server-users-details.component.html',
  styles: []
})
export class IdentityServerUsersDetailsComponent extends DetailsBaseComponent {

  public expand = '';
  public newPassword: string;
  public resetPasswordError: string;
  private _showPassword = false;

  private identityServerUsersApi = this.api.identityServerUsersApi(this.notifyDestruct$); // for intellisense
  public modelRef: IdentityServerUser;

  @ViewChild('resetPasswordModal', { static: true })
  public resetPasswordModal: TemplateRef<any>;

  constructor(
    public workspace: WorkspaceService, private api: ApiService, private translate: TranslateService, private modalService: NgbModal) {
    super();

    this.identityServerUsersApi = this.api.identityServerUsersApi(this.notifyDestruct$);
  }

  public onResetPassword = (model: IdentityServerUser): void => {
    if (!!model && !!model.Id) {
      this.modelRef = model;
      this.modalService.open(this.resetPasswordModal)
        .result.then(
          () => {
            this.resetPasswordError = null;
            this.newPassword = null;
            this._showPassword = false;
          },
          (_: any) => {
            this.resetPasswordError = null;
            this.newPassword = null;
            this._showPassword = false;
          }
        );
    }
  }
  public showResetPassword = (model: IdentityServerUser) => !!model;

  public canResetPassword = (model: IdentityServerUser) => this.ws.canDo('identity-server-users', 'ResetPassword', model.Id);
  public resetPasswordTooltip = (model: IdentityServerUser) => this.canResetPassword(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.admin;
  }

  public resetPasswordClick(modal: any) {

    this.resetPasswordError = null;

    const args: ResetPasswordArgs = {
      userId: this.modelRef.Id,
      password: this.newPassword
    };

    this.identityServerUsersApi.resetPassword(args)
      .pipe(
        tap(res => {
          addToWorkspace(res, this.workspace);
          modal.close();
          this.details.displayModalMessage(this.translate.instant('PasswordWasSuccessfullyReset'));
        }),
        catchError((friendlyError) => this.resetPasswordError = friendlyError.error)
      ).subscribe();
  }

  public togglePasswordVisibility(): void {
    this._showPassword = !this._showPassword;
  }

  public get isPasswordShown(): boolean {
    return this._showPassword;
  }

  public get passwordFieldType(): string {
    return this.isPasswordShown ? 'text' : 'password';
  }
}
