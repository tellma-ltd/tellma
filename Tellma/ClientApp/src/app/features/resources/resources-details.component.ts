import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ResourceForSave, Resource } from '~/app/data/entities/resource';
import { ResourceDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { Currency } from '~/app/data/entities/currency';

@Component({
  selector: 't-resources-details',
  templateUrl: './resources-details.component.html',
  styles: []
})
export class ResourcesDetailsComponent extends DetailsBaseComponent implements OnInit {

  private resourcesApi = this.api.resourcesApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this.resourcesApi = this.api.resourcesApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = `AssetType,RevenueType,ExpenseType,Currency,ExpenseEntryType,
Center,Lookup1,Lookup2,Lookup3,Lookup4,Units/Unit`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get view(): string {
    return `resources/${this.definitionId}`;
  }

  // UI Binding

  private get definition(): ResourceDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Resources[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result: ResourceForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    const defs = this.definition;

    result.Identifier = defs.IdentifierDefaultValue;
    result.CurrencyId = defs.CurrencyDefaultValue;
    result.MonetaryValue = defs.MonetaryValueDefaultValue;
    result.ReorderLevel = defs.ReorderLevelDefaultValue;
    result.EconomicOrderQuantity = defs.EconomicOrderQuantityDefaultValue;
    result.AvailableSince = defs.AvailableSinceDefaultValue;
    result.AvailableTill = defs.AvailableTillDefaultValue;
    result.Decimal1 = defs.Decimal1DefaultValue;
    result.Decimal2 = defs.Decimal2DefaultValue;
    result.Int1 = defs.Int1DefaultValue;
    result.Int2 = defs.Int2DefaultValue;
    result.Lookup1Id = defs.Lookup1DefaultValue;
    result.Lookup2Id = defs.Lookup2DefaultValue;
    result.Lookup3Id = defs.Lookup3DefaultValue;
    result.Lookup4Id = defs.Lookup4DefaultValue;
    // result.Lookup5Id = defs.Lookup5DefaultValue;
    result.Text1 = defs.Text1DefaultValue;
    result.Text2 = defs.Text2DefaultValue;
    result.Units = [];

    return result;
  }

  clone = (item: Resource): Resource => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Resource;
      clone.Id = null;

      if (!!clone.Units) {
        clone.Units.forEach(e => {
          e.Id = null;
          delete e.ResourceId;
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: Resource): void => {
    if (!!model && !!model.Id) {
      this.resourcesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Resource): void => {
    if (!!model && !!model.Id) {
      this.resourcesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Resource) => !!model && !model.IsActive;
  public showDeactivate = (model: Resource) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Resource) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Resource) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public get Identifier_isVisible(): boolean {
    return !!this.definition.IdentifierVisibility;
  }

  public get Identifier_isRequired(): boolean {
    return this.definition.IdentifierVisibility === 'Required';
  }

  public get Identifier_label(): string {
    return !!this.definition.IdentifierLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'IdentifierLabel') :
      this.translate.instant('Resource_Identifier');
  }

  public get Currency_isVisible(): boolean {
    return !!this.definition.CurrencyVisibility;
  }

  public get Currency_isRequired(): boolean {
    return this.definition.CurrencyVisibility === 'Required';
  }

  public get Currency_label(): string {
    return !!this.definition.CurrencyLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'CurrencyLabel') :
      this.translate.instant('Resource_Currency');
  }

  public get MonetaryValue_isVisible(): boolean {
    return !!this.definition.MonetaryValueVisibility;
  }

  public get MonetaryValue_isRequired(): boolean {
    return this.definition.MonetaryValueVisibility === 'Required';
  }

  public get MonetaryValue_label(): string {
    return !!this.definition.MonetaryValueLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'MonetaryValueLabel') :
      this.translate.instant('Resource_MonetaryValue');
  }

  public MonetaryValue_decimals(model: Resource): number {
    const currency = this.ws.get('Currency', model.CurrencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get Description_isVisible(): boolean {
    return !!this.definition.DescriptionVisibility;
  }

  public get Description_isRequired(): boolean {
    return this.definition.DescriptionVisibility === 'Required';
  }

  public get AssetType_isVisible(): boolean {
    return !!this.definition.AssetTypeVisibility;
  }

  public get AssetType_isRequired(): boolean {
    return this.definition.AssetTypeVisibility === 'Required';
  }

  public get RevenueType_isVisible(): boolean {
    return !!this.definition.RevenueTypeVisibility;
  }

  public get RevenueType_isRequired(): boolean {
    return this.definition.RevenueTypeVisibility === 'Required';
  }

  public get ExpenseType_isVisible(): boolean {
    return !!this.definition.ExpenseTypeVisibility;
  }

  public get ExpenseType_isRequired(): boolean {
    return this.definition.ExpenseTypeVisibility === 'Required';
  }

  public get ExpenseEntryType_isVisible(): boolean {
    return !!this.definition.ExpenseEntryTypeVisibility;
  }

  public get ExpenseEntryType_isRequired(): boolean {
    return this.definition.ExpenseEntryTypeVisibility === 'Required';
  }

  public get Center_isVisible(): boolean {
    return !!this.definition.CenterVisibility && this.ws.settings.IsMultiCenter;
  }

  public get Center_isRequired(): boolean {
    return this.definition.CenterVisibility === 'Required';
  }

  public ResidualMonetaryValue_isVisible(_: ResourceForSave): boolean {
    return !!this.definition.ResidualMonetaryValueVisibility;
  }

  public get ResidualMonetaryValue_isRequired(): boolean {
    return this.definition.ResidualMonetaryValueVisibility === 'Required';
  }

  public currencyPostfix(model: ResourceForSave): string {
    return !!model && !!model.CurrencyId ? ` (${this.ws.getMultilingualValue('Currency', model.CurrencyId, 'Name')})` :
      ` (${this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName')})`;
  }

  public ResidualValue_isVisible(model: ResourceForSave): boolean {
    // If the residual monetary value is visible: appears only when the currency is not functional
    // If the residual monetary value is invisible: appears anyway
    return !!this.definition.ResidualValueVisibility && ((!!model &&
      !!model.CurrencyId && model.CurrencyId !== this.ws.settings.FunctionalCurrencyId) ||
      !this.ResidualMonetaryValue_isVisible(model));
  }

  public get functionalDecimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functionalPostfix(): string {
    return ` (${this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName')})`;
  }

  public get ResidualValue_isRequired(): boolean {
    return this.definition.ResidualValueVisibility === 'Required';
  }

  public get ReorderLevel_isVisible(): boolean {
    return !!this.definition.ReorderLevelVisibility;
  }

  public get ReorderLevel_isRequired(): boolean {
    return this.definition.ReorderLevelVisibility === 'Required';
  }

  public get EconomicOrderQuantity_isVisible(): boolean {
    return !!this.definition.EconomicOrderQuantityVisibility;
  }

  public get EconomicOrderQuantity_isRequired(): boolean {
    return this.definition.EconomicOrderQuantityVisibility === 'Required';
  }

  public get AvailableSince_isVisible(): boolean {
    return !!this.definition.AvailableSinceVisibility;
  }

  public get AvailableSince_isRequired(): boolean {
    return this.definition.AvailableSinceVisibility === 'Required';
  }

  public get AvailableSince_label(): string {
    return !!this.definition.AvailableSinceLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'AvailableSinceLabel') :
      this.translate.instant('Resource_AvailableSince');
  }

  public get AvailableTill_isVisible(): boolean {
    return !!this.definition.AvailableTillVisibility;
  }

  public get AvailableTill_isRequired(): boolean {
    return this.definition.AvailableTillVisibility === 'Required';
  }

  public get AvailableTill_label(): string {
    return !!this.definition.AvailableTillLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'AvailableTillLabel') :
      this.translate.instant('Resource_AvailableTill');
  }

  public get Decimal1_isVisible(): boolean {
    return !!this.definition.Decimal1Visibility;
  }

  public get Decimal1_isRequired(): boolean {
    return this.definition.Decimal1Visibility === 'Required';
  }

  public get Decimal1_label(): string {
    return !!this.definition.Decimal1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Decimal1Label') :
      this.translate.instant('Resource_Decimal1');
  }

  public get Decimal2_isVisible(): boolean {
    return !!this.definition.Decimal2Visibility;
  }

  public get Decimal2_isRequired(): boolean {
    return this.definition.Decimal2Visibility === 'Required';
  }

  public get Decimal2_label(): string {
    return !!this.definition.Decimal2Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Decimal2Label') :
      this.translate.instant('Resource_Decimal2');
  }

  public get Int1_isVisible(): boolean {
    return !!this.definition.Int1Visibility;
  }

  public get Int1_isRequired(): boolean {
    return this.definition.Int1Visibility === 'Required';
  }

  public get Int1_label(): string {
    return !!this.definition.Int1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Int1Label') :
      this.translate.instant('Resource_Int1');
  }

  public get Int2_isVisible(): boolean {
    return !!this.definition.Int2Visibility;
  }

  public get Int2_isRequired(): boolean {
    return this.definition.Int2Visibility === 'Required';
  }

  public get Int2_label(): string {
    return !!this.definition.Int2Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Int2Label') :
      this.translate.instant('Resource_Int2');
  }

  public get Lookup1_isVisible(): boolean {
    return !!this.definition.Lookup1Visibility;
  }

  public get Lookup1_isRequired(): boolean {
    return this.definition.Lookup1Visibility === 'Required';
  }

  public get Lookup1_label(): string {
    return !!this.definition.Lookup1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup1Label') :
      this.translate.instant('Resource_Lookup1');
  }

  public get Lookup1_DefinitionId() {
    return this.definition.Lookup1DefinitionId;
  }

  public get Lookup2_isVisible(): boolean {
    return !!this.definition.Lookup2Visibility;
  }

  public get Lookup2_isRequired(): boolean {
    return this.definition.Lookup2Visibility === 'Required';
  }

  public get Lookup2_label(): string {
    return !!this.definition.Lookup2Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup2Label') :
      this.translate.instant('Resource_Lookup2');
  }

  public get Lookup2_DefinitionId() {
    return this.definition.Lookup2DefinitionId;
  }

  public get Lookup3_isVisible(): boolean {
    return !!this.definition.Lookup3Visibility;
  }

  public get Lookup3_isRequired(): boolean {
    return this.definition.Lookup3Visibility === 'Required';
  }

  public get Lookup3_label(): string {
    return !!this.definition.Lookup3Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup3Label') :
      this.translate.instant('Resource_Lookup3');
  }

  public get Lookup3_DefinitionId() {
    return this.definition.Lookup3DefinitionId;
  }

  public get Lookup4_isVisible(): boolean {
    return !!this.definition.Lookup4Visibility;
  }

  public get Lookup4_isRequired(): boolean {
    return this.definition.Lookup4Visibility === 'Required';
  }

  public get Lookup4_label(): string {
    return !!this.definition.Lookup4Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup4Label') :
      this.translate.instant('Resource_Lookup4');
  }

  public get Lookup4_DefinitionId() {
    return this.definition.Lookup4DefinitionId;
  }

  // public get Lookup5_DefinitionId() {
  //   return this.definition.Lookup5DefinitionId;
  // }

  public get Text1_isVisible(): boolean {
    return !!this.definition.Text1Visibility;
  }

  public get Text1_isRequired(): boolean {
    return this.definition.Text1Visibility === 'Required';
  }

  public get Text1_label(): string {
    return !!this.definition.Text1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Text1Label') :
      this.translate.instant('Resource_Text1');
  }

  public get Text2_isVisible(): boolean {
    return !!this.definition.Text2Visibility;
  }

  public get Text2_isRequired(): boolean {
    return this.definition.Text2Visibility === 'Required';
  }

  public get Text2_label(): string {
    return !!this.definition.Text2Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Text2Label') :
      this.translate.instant('Resource_Text2');
  }

  public Units_count(model: ResourceForSave): number {
    return !!model && !!model.Units ? model.Units.length : 0;
  }

  public Units_showError(model: ResourceForSave): boolean {
    return !!model && !!model.Units && model.Units.some(e => !!e.serverErrors);
  }
}
