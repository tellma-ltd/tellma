import { Component, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { IdentityServerClient, IdentityServerClientForSave } from '~/app/data/entities/identity-server-client';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 't-identity-server-clients-details',
  templateUrl: './identity-server-clients-details.component.html',
  styles: [
  ]
})
export class IdentityServerClientsDetailsComponent extends DetailsBaseComponent {

  private clientsApi = this.api.identityServerClientsApi(this.notifyDestruct$); // for intellisense

  private modelForShowClientSecret: IdentityServerClient;
  private modelRef: IdentityServerClient;
  public expand = '';

  @ViewChild('resetSecretConfirmModal', { static: true })
  unsavedChangesModal: TemplateRef<any>;

  create = () => {
    const result: IdentityServerClientForSave = {};
    result.Name = this.initialText;

    return result;
  }

  clone: (item: IdentityServerClient) => IdentityServerClient = (item: IdentityServerClient) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as IdentityServerClient;
      delete clone.Id;
      delete clone.ClientId;
      delete clone.ClientSecret;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    private workspace: WorkspaceService,
    private route: ActivatedRoute,
    private api: ApiService,
    private translate: TranslateService,
    private modalService: NgbModal) {
    super();

    this.clientsApi = this.api.identityServerClientsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.admin;
  }

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }

  public isInactive(): boolean {
    return false;
  }

  public showSecret(model: IdentityServerClient) {
    if (this.modelForShowClientSecret === model) {
      return true;
    } else {
      delete this.modelForShowClientSecret;
      return false;
    }
  }

  public onShowSecret(model: IdentityServerClient) {
    this.modelForShowClientSecret = model;
  }

  public onHideSecret() {
    delete this.modelForShowClientSecret;
  }

  public onResetSecret = (model: IdentityServerClient): void => {
    this.modelRef = model;

    // IF there are unsaved changes, prompt the user asking if they would like them discarded
    this.modalService.open(this.unsavedChangesModal);
  }

  public doResetSecret = (): void => {
    const model = this.modelRef;
    if (this.showResetSecret(model)) {
      this.clientsApi.resetSecret({ id: model.Id, returnEntities: true }).pipe(
        tap(res => {
          addToWorkspace(res, this.workspace);
          this.details.displaySuccessMessage(this.translate.instant('ResetSecretSuccessMessage'));
        })
      ).subscribe({ error: this.details.handleActionError });
    }
  }
  public showResetSecret = (model: IdentityServerClient) => !!model && !!model.Id;

  public canResetSecret = (model: IdentityServerClient) => this.ws.canDo('identity-server-clients', 'Update', model.Id);

  public resetSecretTooltip = (model: IdentityServerClient) => this.canResetSecret(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
