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
  selector: 'b-resources-details',
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

  public expand = `ResourceClassification,Currency,CountUnit,MassUnit,VolumeUnit,TimeUnit,Lookup1,Lookup2`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute) {
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
    return !!this.definitionId ? this.workspace.current.definitions.Resources[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result = new ResourceForSave();
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
    result.CountUnitId = defs.CountUnitDefaultValue;
    result.Count = defs.CountDefaultValue;
    result.MassUnitId = defs.MassUnitDefaultValue;
    result.Mass = defs.MassDefaultValue;
    result.VolumeUnitId = defs.VolumeUnitDefaultValue;
    result.Volume = defs.VolumeDefaultValue;
    result.TimeUnitId = defs.TimeUnitDefaultValue;
    result.Time = defs.TimeDefaultValue;
    result.AvailableSince = defs.AvailableSinceDefaultValue;
    result.AvailableTill = defs.AvailableTillDefaultValue;
    result.Lookup1Id = defs.Lookup1DefaultValue;
    result.Lookup2Id = defs.Lookup2DefaultValue;
    // result.Lookup3Id = defs.Lookup3DefaultValue;
    // result.Lookup4Id = defs.Lookup4DefaultValue;
    // result.Lookup5Id = defs.Lookup5DefaultValue;

    return result;
  }

  public get ws() {
    return this.workspace.current;
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
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals; // TODO: Use functional currency
  }

  public get CountUnit_isVisible(): boolean {
    return !!this.definition.CountUnitVisibility;
  }

  public get CountUnit_isRequired(): boolean {
    return this.definition.CountUnitVisibility === 'Required';
  }

  public get CountUnit_label(): string {
    return !!this.definition.CountUnitLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'CountUnitLabel') :
      this.translate.instant('Resource_CountUnit');
  }

  public get Count_isVisible(): boolean {
    return !!this.definition.CountVisibility;
  }

  public get Count_isRequired(): boolean {
    return this.definition.CountVisibility === 'Required';
  }

  public get Count_label(): string {
    return !!this.definition.CountLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'CountLabel') :
      this.translate.instant('Resource_Count');
  }

  public get MassUnit_isVisible(): boolean {
    return !!this.definition.MassUnitVisibility;
  }

  public get MassUnit_isRequired(): boolean {
    return this.definition.MassUnitVisibility === 'Required';
  }

  public get MassUnit_label(): string {
    return !!this.definition.MassUnitLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'MassUnitLabel') :
      this.translate.instant('Resource_MassUnit');
  }

  public get Mass_isVisible(): boolean {
    return !!this.definition.MassVisibility;
  }

  public get Mass_isRequired(): boolean {
    return this.definition.MassVisibility === 'Required';
  }

  public get Mass_label(): string {
    return !!this.definition.MassLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'MassLabel') :
      this.translate.instant('Resource_Mass');
  }

  public get VolumeUnit_isVisible(): boolean {
    return !!this.definition.VolumeUnitVisibility;
  }

  public get VolumeUnit_isRequired(): boolean {
    return this.definition.VolumeUnitVisibility === 'Required';
  }

  public get VolumeUnit_label(): string {
    return !!this.definition.VolumeUnitLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'VolumeUnitLabel') :
      this.translate.instant('Resource_VolumeUnit');
  }

  public get Volume_isVisible(): boolean {
    return !!this.definition.VolumeVisibility;
  }

  public get Volume_isRequired(): boolean {
    return this.definition.VolumeVisibility === 'Required';
  }

  public get Volume_label(): string {
    return !!this.definition.VolumeLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'VolumeLabel') :
      this.translate.instant('Resource_Volume');
  }

  public get TimeUnit_isVisible(): boolean {
    return !!this.definition.TimeUnitVisibility;
  }

  public get TimeUnit_isRequired(): boolean {
    return this.definition.TimeUnitVisibility === 'Required';
  }

  public get TimeUnit_label(): string {
    return !!this.definition.TimeUnitLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TimeUnitLabel') :
      this.translate.instant('Resource_TimeUnit');
  }

  public get Time_isVisible(): boolean {
    return !!this.definition.TimeVisibility;
  }

  public get Time_isRequired(): boolean {
    return this.definition.TimeVisibility === 'Required';
  }

  public get Time_label(): string {
    return !!this.definition.TimeLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TimeLabel') :
      this.translate.instant('Resource_Time');
  }

  public get Description_isVisible(): boolean {
    return !!this.definition.DescriptionVisibility;
  }

  public get Description_isRequired(): boolean {
    return this.definition.DescriptionVisibility === 'Required';
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

  // public get Lookup3_DefinitionId() {
  //   return this.definition.Lookup3DefinitionId;
  // }

  // public get Lookup4_DefinitionId() {
  //   return this.definition.Lookup4DefinitionId;
  // }

  // public get Lookup5_DefinitionId() {
  //   return this.definition.Lookup5DefinitionId;
  // }
}
