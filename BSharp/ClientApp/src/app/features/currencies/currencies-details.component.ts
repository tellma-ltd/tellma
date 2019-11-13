import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { CurrencyForSave, metadata_Currency, Currency } from '~/app/data/entities/currency';
import { ActivatedRoute } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 'b-currencies-details',
  templateUrl: './currencies-details.component.html',
  styles: []
})
export class CurrenciesDetailsComponent extends DetailsBaseComponent {

  private _decimalPlacesChoices: SelectorChoice[];
  private currenciesApi = this.api.currenciesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result = new CurrencyForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    result.E = 2;
    return result;
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute) {
    super();

    this.currenciesApi = this.api.currenciesApi(this.notifyDestruct$);
  }

  get decimalPlacesChoices(): SelectorChoice[] {

    if (!this._decimalPlacesChoices) {
      const descriptor = metadata_Currency(this.ws, this.translate, null).properties.E as ChoicePropDescriptor;
      this._decimalPlacesChoices = descriptor.choices.map(c => ({ name: () => descriptor.format(c), value: c }));
    }

    return this._decimalPlacesChoices;
  }

  public decimalPlacesLookup(value: any): string {
    const descriptor = metadata_Currency(this.ws, this.translate, null).properties.E as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: Currency): void => {
    if (!!model && !!model.Id) {
      this.currenciesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Currency): void => {
    if (!!model && !!model.Id) {
      this.currenciesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Currency) => !!model && !model.IsActive;
  public showDeactivate = (model: Currency) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Currency) => this.ws.canDo('currencies', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Currency) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }
}
