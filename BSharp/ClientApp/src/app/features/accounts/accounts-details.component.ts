import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor, NavigationPropDescriptor } from '~/app/data/entities/base/metadata';
import { AccountDefinitionForClient, AccountVisibility } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 'b-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private accountsApi = this.api.accountsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  definitionPreview: AccountDefinitionForClient;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.accountsApi = this.api.accountsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = `ResponsibilityCenter,Custodian,Resource,Location`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!this.definition(definitionId)) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  public definition(definitionId: string): AccountDefinitionForClient {
    return this.definitionPreview || this.workspace.current.definitions.Accounts[definitionId];
  }

  get viewId(): string {
    return `accounts/${this.definitionId}`;
  }

  // UI Binding

  create = () => {
    const result = new AccountForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    const defs = this.definition(this.definitionId);

    result.CustodianId = defs.Custodian_DefaultValue;
    result.ResourceId = defs.Resource_DefaultValue;
    result.LocationId = defs.Location_DefaultValue;
    result.ResponsibilityCenterId = defs.ResponsibilityCenter_DefaultValue;

    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public get p(): { [prop: string]: PropDescriptor } {
    return metadata_Account(this.ws, this.translate, this.definitionId).properties;
  }

  public get d(): AccountDefinitionForClient {
    return this.ws.definitions.Accounts[this.definitionId];
  }

  public onActivate = (model: Account): void => {
    if (!!model && !!model.Id) {
      this.accountsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeprecate = (model: Account): void => {
    if (!!model && !!model.Id) {
      this.accountsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Account) => !!model && model.IsDeprecated;
  public showDeprecate = (model: Account) => !!model && model.IsDeprecated;

  public canActivateDeprecateItem = (model: Account) => this.ws.canDo(this.viewId, 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: Account) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Accounts[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }

  public isRequired(visibility: AccountVisibility) {
    return visibility === 'RequiredInAccounts';
  }
}
