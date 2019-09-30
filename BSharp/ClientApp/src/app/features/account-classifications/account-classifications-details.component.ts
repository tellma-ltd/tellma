import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { ActivatedRoute } from '@angular/router';
import {
  AccountClassificationForSave, metadata_AccountClassification,
  AccountClassification
} from '~/app/data/entities/account-classification';

@Component({
  selector: 'b-account-classifications-details',
  templateUrl: './account-classifications-details.component.html',
  styles: []
})
export class AccountClassificationsDetailsComponent extends DetailsBaseComponent {

  private _decimalPlacesChoices: { name: string, value: any }[];
  private accountClassificationsApi = this.api.accountClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  create = () => {
    const result = new AccountClassificationForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    return result;
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute) {
    super();

    this.accountClassificationsApi = this.api.accountClassificationsApi(this.notifyDestruct$);
  }

  get decimalPlacesChoices(): { name: string, value: any }[] {

    if (!this._decimalPlacesChoices) {
      const descriptor = metadata_AccountClassification(this.ws, this.translate, null).properties.E as ChoicePropDescriptor;
      this._decimalPlacesChoices = descriptor.choices.map(c => ({ name: descriptor.format(c), value: c }));
    }

    return this._decimalPlacesChoices;
  }

  public decimalPlacesLookup(value: any): string {
    const descriptor = metadata_AccountClassification(this.ws, this.translate, null).properties.E as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: AccountClassification): void => {
    if (!!model && !!model.Id) {
      this.accountClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeprecate = (model: AccountClassification): void => {
    if (!!model && !!model.Id) {
      this.accountClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: AccountClassification) => !!model && model.IsDeprecated;
  public showDeprecate = (model: AccountClassification) => !!model && !model.IsDeprecated;

  public canActivateDeprecateItem = (model: AccountClassification) => this.ws.canDo('account-classifications', 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: AccountClassification) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }
}
