// tslint:disable:member-ordering
import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { ResourceForSave, Resource } from '~/app/data/entities/resource';
import { ResourceDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { Currency } from '~/app/data/entities/currency';
import { LatLngLiteral } from '@agm/core';

@Component({
  selector: 't-resources-details',
  templateUrl: './resources-details.component.html',
  styles: []
})
export class ResourcesDetailsComponent extends DetailsBaseComponent implements OnInit {

  private resourcesApi = this.api.resourcesApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.resourcesApi = this.api.resourcesApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
    return this._definitionId;
  }

  @Input()
  previewDefinition: ResourceDefinitionForClient; // Used in preview mode

  public expand = `Currency,Center,Lookup1,Lookup2,Lookup3,Lookup4,Participant,Unit,UnitMassUnit,Units/Unit`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = +params.get('definitionId');

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
    return this.previewDefinition || (!!this.definitionId ? this.ws.definitions.Resources[this.definitionId] : null);
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

    // result.Identifier = defs.IdentifierDefaultValue;
    // result.CurrencyId = defs.CurrencyDefaultValue;
    // result.MonetaryValue = defs.MonetaryValueDefaultValue;
    // result.ReorderLevel = defs.ReorderLevelDefaultValue;
    // result.EconomicOrderQuantity = defs.EconomicOrderQuantityDefaultValue;
    // result.FromDate = defs.FromDateDefaultValue;
    // result.ToDate = defs.ToDateDefaultValue;
    // result.Decimal1 = defs.Decimal1DefaultValue;
    // result.Decimal2 = defs.Decimal2DefaultValue;
    // result.Int1 = defs.Int1DefaultValue;
    // result.Int2 = defs.Int2DefaultValue;
    // result.Lookup1Id = defs.Lookup1DefaultValue;
    // result.Lookup2Id = defs.Lookup2DefaultValue;
    // result.Lookup3Id = defs.Lookup3DefaultValue;
    // result.Lookup4Id = defs.Lookup4DefaultValue;
    // result.Lookup5Id = defs.Lookup5DefaultValue;
    // result.Text1 = defs.Text1DefaultValue;
    // result.Text2 = defs.Text2DefaultValue;
    result.VatRate = defs.DefaultVatRate;
    result.UnitId = defs.DefaultUnitId;
    result.UnitMassUnitId = defs.DefaultUnitMassUnitId;
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
          delete e.CreatedAt;
          delete e.CreatedById;
          delete e.ModifiedAt;
          delete e.ModifiedById;
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

  public onEditDefinition = (_: Resource) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../resource-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public showActivate = (model: Resource) => !!model && !model.IsActive;
  public showDeactivate = (model: Resource) => !!model && model.IsActive;
  public showEditDefinition = (_: Resource) => this.ws.canDo('resource-definitions', 'Update', null);

  public canActivateDeactivateItem = (model: Resource) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Resource) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  // Shared with Relation

  public get Currency_isVisible(): boolean {
    return !!this.definition.CurrencyVisibility;
  }

  public get Currency_isRequired(): boolean {
    return this.definition.CurrencyVisibility === 'Required';
  }

  public get Currency_label(): string {
    return this.translate.instant('Entity_Currency');
  }

  public get Center_isVisible(): boolean {
    return !!this.definition.CenterVisibility;
  }

  public get Center_isRequired(): boolean {
    return this.definition.CenterVisibility === 'Required';
  }

  public get Center_label(): string {
    return this.translate.instant('Entity_Center');
  }

  public get Image_isVisible(): boolean {
    return !!this.definition.ImageVisibility;
  }

  public get Description_isVisible(): boolean {
    return !!this.definition.DescriptionVisibility;
  }

  public get Description_isRequired(): boolean {
    return this.definition.DescriptionVisibility === 'Required';
  }

  public get FromDate_isVisible(): boolean {
    return !!this.definition.FromDateVisibility;
  }

  public get FromDate_isRequired(): boolean {
    return this.definition.FromDateVisibility === 'Required';
  }

  public get FromDate_label(): string {
    return !!this.definition.FromDateLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'FromDateLabel') :
      this.translate.instant('Entity_FromDate');
  }

  public get ToDate_isVisible(): boolean {
    return !!this.definition.ToDateVisibility;
  }

  public get ToDate_isRequired(): boolean {
    return this.definition.ToDateVisibility === 'Required';
  }

  public get ToDate_label(): string {
    return !!this.definition.ToDateLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'ToDateLabel') :
      this.translate.instant('Entity_ToDate');
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
      this.translate.instant('Entity_Decimal1');
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
      this.translate.instant('Entity_Decimal2');
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
      this.translate.instant('Entity_Int1');
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
      this.translate.instant('Entity_Int2');
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
      this.translate.instant('Entity_Lookup1');
  }

  public get Lookup1_DefinitionId(): number {
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
      this.translate.instant('Entity_Lookup2');
  }

  public get Lookup2_DefinitionId(): number {
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
      this.translate.instant('Entity_Lookup3');
  }

  public get Lookup3_DefinitionId(): number {
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
      this.translate.instant('Entity_Lookup4');
  }

  public get Lookup4_DefinitionId(): number {
    return this.definition.Lookup4DefinitionId;
  }

  // public get Lookup5_DefinitionId(): number {
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
      this.translate.instant('Entity_Text1');
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
      this.translate.instant('Entity_Text2');
  }

  // Resource Only

  public get showTabs(): boolean {
    return this.Units_isVisible || this.Location_isVisible;
  }

  public get Unit_isVisible(): boolean {
    return !!this.definition.UnitCardinality;
  }

  public get Unit_isRequired(): boolean {
    return !this.definition.DefaultUnitId;
  }

  public get Unit_default(): number {
    return this.definition.DefaultUnitId;
  }

  public get UnitMass_isVisible(): boolean {
    return !!this.definition.UnitMassVisibility;
  }

  public get UnitMass_isRequired(): boolean {
    return this.definition.UnitMassVisibility === 'Required';
  }

  public get Units_isVisible(): boolean {
    return this.definition.UnitCardinality === 'Multiple';
  }

  public Units_count(model: ResourceForSave): number {
    return !!model && !!model.Units ? model.Units.length : 0;
  }

  public Units_showError(model: ResourceForSave): boolean {
    return !!model && !!model.Units && model.Units.some(e => !!e.serverErrors);
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

  public get VatRate_isVisible(): boolean {
    return !!this.definition.VatRateVisibility;
  }

  public get VatRate_isRequired(): boolean {
    return this.definition.VatRateVisibility === 'Required';
  }

  public currencyPostfix(model: ResourceForSave): string {
    return !!model && !!model.CurrencyId ? ` (${this.ws.getMultilingualValue('Currency', model.CurrencyId, 'Name')})` :
      ` (${this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName')})`;
  }

  public get functionalDecimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functionalPostfix(): string {
    return ` (${this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName')})`;
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

  public get MonetaryValue_isVisible(): boolean {
    return !!this.definition.MonetaryValueVisibility;
  }

  public get MonetaryValue_isRequired(): boolean {
    return this.definition.MonetaryValueVisibility === 'Required';
  }

  public get MonetaryValue_label(): string {
    return this.translate.instant('Resource_MonetaryValue');
  }

  public MonetaryValue_decimals(model: Resource): number {
    const currency = this.ws.get('Currency', model.CurrencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get Participant_isVisible(): boolean {
    return !!this.definition.ParticipantVisibility;
  }

  public get Participant_isRequired(): boolean {
    return this.definition.ParticipantVisibility === 'Required';
  }

  public get Participant_label(): string {
    const def = this.definition;
    const participantDefId = def.ParticipantDefinitionId;
    const participantDef = this.ws.definitions.Relations[participantDefId];
    if (!!participantDef) {
      return this.ws.getMultilingualValueImmediate(participantDef, 'TitleSingular');
    } else {
      return this.translate.instant('Resource_Participant');
    }
  }

  public get Participant_definitionIds(): number[] {
    return [this.definition.ParticipantDefinitionId];
  }

  // Location + Map stuff

  public get Location_isVisible(): boolean {
    return !!this.definition.LocationVisibility;
  }

  public Map_showError(model: ResourceForSave): boolean {
    return !!model && !!model.serverErrors && !!model.serverErrors.LocationJson;
  }

  public get zoom(): number {
    // console.log(+localStorage.map_zoom);
    return +localStorage.map_zoom || 2;
  }

  public set zoom(v: number) {
    localStorage.map_zoom = v;
  }

  private _lat: number;
  private _lng: number;

  public get latitude(): number {
    if (this._lat === undefined) {
      this._lat = +localStorage.map_latitude || 0;
    }
    return this._lat;
  }

  public get longitude(): number {
    if (this._lng === undefined) {
      this._lng = +localStorage.map_longitude || 0;
    }
    return this._lng;
  }

  public onCenterChange(event: LatLngLiteral) {
    localStorage.map_latitude = event.lat;
    localStorage.map_longitude = event.lng;
  }

  public styleFunc = (feature: any) => {

    // This is the result object
    const styleOptions = {
      clickable: false,
    };

    // https://developers.google.com/maps/documentation/javascript/reference/data#Data.StyleOptions
    const propNames = ['fillColor', 'fillOpacity', 'icon', 'strokeColor', 'shape', 'strokeOpacity', 'strokeWeight', 'visible'];

    // Go over the properties and copy them across
    for (const propName of propNames) {
      const propValue = feature.getProperty(propName);
      if (propValue !== undefined && propValue !== null) {
        styleOptions[propName] = propValue;
      }
    }

    // Return the style options
    return styleOptions;
  }

  private parseJsonString: string;
  private parseJsonResult: any;
  public parseJsonError: string;

  public parseJson(json: string) {
    json = json || undefined;
    if (this.parseJsonString !== json) {
      this.parseJsonString = json;
      if (!json) {
        delete this.parseJsonResult;
        delete this.parseJsonError;
      } else {
        try {
          this.parseJsonResult = JSON.parse(json);
          delete this.parseJsonError;
        } catch (err) {
          this.parseJsonError = err;
          delete this.parseJsonResult;
        }
      }
    }

    return this.parseJsonResult;
  }

  public locationView: 'map' | 'json' = 'map';

  public onView(view: 'map' | 'json'): void {
    this.locationView = view;
  }

  public isView(view: 'map' | 'json'): boolean {
    return this.locationView === view;
  }

  public savePreprocessing = (entity: ResourceForSave) => {
    // Server validation on hidden properties will be confusing to the user
  }
}
