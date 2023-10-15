// tslint:disable:member-ordering
import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace, colorFromExtension, downloadBlob, fileSizeDisplay, iconFromExtension, onFileSelected, openOrDownloadBlob } from '~/app/data/util';
import { catchError, finalize, tap } from 'rxjs/operators';
import { ReportStore, WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { ResourceForSave, Resource } from '~/app/data/entities/resource';
import {
  DefinitionReportDefinitionForClient, ReportDefinitionForClient, ResourceDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { Currency } from '~/app/data/entities/currency';
import { ReportView } from '../report-results/report-results.component';
import { ResourceAttachment, ResourceAttachmentForSave } from '~/app/data/entities/resource-attachment';
import { of } from 'rxjs';

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

  public expand = `Currency,Center,Lookup1,Lookup2,Lookup3,Lookup4,Agent1,Agent2,Unit,UnitMassUnit,
Units.Unit,Resource1,Resource2,Attachments.CreatedBy`;

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
    // result.Decimal3 = defs.Decimal3DefaultValue;
    // result.Decimal4 = defs.Decimal4DefaultValue;
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
    result.Attachments = [];

    return result;
  }

  clone = (item: Resource): Resource => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Resource;
      delete clone.Id;
      clone.Attachments = []; // Attachments can't be cloned

      if (!!clone.Units) {
        clone.Units.forEach(e => {
          delete e.Id;
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

  // Shared with Agent

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

  public filterCenter(_: Resource): string {
    const ws = this.ws;
    if (!!ws.settings.FeatureFlags && ws.settings.FeatureFlags.BusinessUnitGoneWithTheWind) {
      return `IsLeaf`;
    } else {
      return null;
    }
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

  public get Date1_isVisible(): boolean {
    return !!this.definition.Date1Visibility;
  }

  public get Date1_isRequired(): boolean {
    return this.definition.Date1Visibility === 'Required';
  }

  public get Date1_label(): string {
    return !!this.definition.Date1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Date1Label') :
      this.translate.instant('Entity_Date1');
  }

  public get Date2_isVisible(): boolean {
    return !!this.definition.Date2Visibility;
  }

  public get Date2_isRequired(): boolean {
    return this.definition.Date2Visibility === 'Required';
  }

  public get Date2_label(): string {
    return !!this.definition.Date2Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Date2Label') :
      this.translate.instant('Entity_Date2');
  }

  public get Date3_isVisible(): boolean {
    return !!this.definition.Date3Visibility;
  }

  public get Date3_isRequired(): boolean {
    return this.definition.Date3Visibility === 'Required';
  }

  public get Date3_label(): string {
    return !!this.definition.Date3Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Date3Label') :
      this.translate.instant('Entity_Date3');
  }

  public get Date4_isVisible(): boolean {
    return !!this.definition.Date4Visibility;
  }

  public get Date4_isRequired(): boolean {
    return this.definition.Date4Visibility === 'Required';
  }

  public get Date4_label(): string {
    return !!this.definition.Date4Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Date4Label') :
      this.translate.instant('Entity_Date4');
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

  public get Decimal3_isVisible(): boolean {
    return !!this.definition.Decimal3Visibility;
  }

  public get Decimal3_isRequired(): boolean {
    return this.definition.Decimal3Visibility === 'Required';
  }

  public get Decimal3_label(): string {
    return !!this.definition.Decimal3Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Decimal3Label') :
      this.translate.instant('Entity_Decimal3');
  }

  public get Decimal4_isVisible(): boolean {
    return !!this.definition.Decimal4Visibility;
  }

  public get Decimal4_isRequired(): boolean {
    return this.definition.Decimal4Visibility === 'Required';
  }

  public get Decimal4_label(): string {
    return !!this.definition.Decimal4Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Decimal4Label') :
      this.translate.instant('Entity_Decimal4');
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

  public showTabs(isEdit: boolean, model: Resource): boolean {
    return this.Units_isVisible || this.Location_isVisible || this.Attachments_isVisible || 
      (this.reports.length > 0 && this.showReports(isEdit, model));
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

  public get Agent1_isVisible(): boolean {
    return !!this.definition.Agent1Visibility;
  }

  public get Agent1_isRequired(): boolean {
    return this.definition.Agent1Visibility === 'Required';
  }

  public get Agent1_label(): string {
    const def = this.definition;

    let result = this.ws.getMultilingualValueImmediate(def, 'Agent1Label');
    if (!result) {
      const agent1DefId = def.Agent1DefinitionId;
      const agent1Def = this.ws.definitions.Agents[agent1DefId];
      if (!!agent1Def) {
        result = this.ws.getMultilingualValueImmediate(agent1Def, 'TitleSingular');
      } else {
        result = this.translate.instant('Resource_Agent1');
      }
    }

    return result;
  }

  public get Agent1_definitionIds(): number[] {
    return [this.definition.Agent1DefinitionId];
  }

  public get Agent2_isVisible(): boolean {
    return !!this.definition.Agent2Visibility;
  }

  public get Agent2_isRequired(): boolean {
    return this.definition.Agent2Visibility === 'Required';
  }

  public get Agent2_label(): string {
    const def = this.definition;

    let result = this.ws.getMultilingualValueImmediate(def, 'Agent2Label');
    if (!result) {
      const agent2DefId = def.Agent2DefinitionId;
      const agent2Def = this.ws.definitions.Agents[agent2DefId];
      if (!!agent2Def) {
        result = this.ws.getMultilingualValueImmediate(agent2Def, 'TitleSingular');
      } else {
        result = this.translate.instant('Resource_Agent2');
      }
    }

    return result;
  }

  public get Agent2_definitionIds(): number[] {
    return [this.definition.Agent2DefinitionId];
  }

  public get Resource1_isVisible(): boolean {
    return !!this.definition.Resource1Visibility;
  }

  public get Resource1_isRequired(): boolean {
    return this.definition.Resource1Visibility === 'Required';
  }

  public get Resource1_label(): string {
    const def = this.definition;

    let result = this.ws.getMultilingualValueImmediate(def, 'Resource1Label');
    if (!result) {
      const resource1DefId = def.Resource1DefinitionId;
      const resource1Def = this.ws.definitions.Resources[resource1DefId];
      if (!!resource1Def) {
        result = this.ws.getMultilingualValueImmediate(resource1Def, 'TitleSingular');
      } else {
        result = this.translate.instant('Resource_Resource1');
      }
    }

    return result;
  }

  public get Resource1_DefinitionIds(): number[] {
    if (!!this.definition.Resource1DefinitionId) {
      return [this.definition.Resource1DefinitionId];
    } else {
      return [];
    }
  }

  public get Resource2_isVisible(): boolean {
    return !!this.definition.Resource2Visibility;
  }

  public get Resource2_isRequired(): boolean {
    return this.definition.Resource2Visibility === 'Required';
  }

  public get Resource2_label(): string {
    const def = this.definition;

    let result = this.ws.getMultilingualValueImmediate(def, 'Resource2Label');
    if (!result) {
      const resource2DefId = def.Resource2DefinitionId;
      const resource2Def = this.ws.definitions.Resources[resource2DefId];
      if (!!resource2Def) {
        result = this.ws.getMultilingualValueImmediate(resource2Def, 'TitleSingular');
      } else {
        result = this.translate.instant('Resource_Resource2');
      }
    }

    return result;
  }

  public get Resource2_DefinitionIds(): number[] {
    if (!!this.definition.Resource2DefinitionId) {
      return [this.definition.Resource2DefinitionId];
    } else {
      return [];
    }
  }
  
  // Attachments

  public get Attachments_isVisible(): boolean {
    return !!this.definition.HasAttachments;
  }

  public Attachments_count(model: ResourceForSave): number {
    return !!model && !!model.Attachments ? model.Attachments.length : 0;
  }

  public Attachments_showError(model: ResourceForSave): boolean {
    return !!model && !!model.Attachments && model.Attachments.some(e => !!e.serverErrors);
  }

  // Location + Map stuff

  public get Location_isVisible(): boolean {
    return !!this.definition.LocationVisibility;
  }

  public Map_showError(model: ResourceForSave): boolean {
    return !!model && !!model.serverErrors && !!model.serverErrors.LocationJson;
  }

  // public get zoom(): number {
  //   // console.log(+localStorage.map_zoom);
  //   return +localStorage.map_zoom || 2;
  // }

  // public set zoom(v: number) {
  //   localStorage.map_zoom = v;
  // }

  // private _lat: number;
  // private _lng: number;

  // public get latitude(): number {
  //   if (this._lat === undefined) {
  //     this._lat = +localStorage.map_latitude || 0;
  //   }
  //   return this._lat;
  // }

  // public get longitude(): number {
  //   if (this._lng === undefined) {
  //     this._lng = +localStorage.map_longitude || 0;
  //   }
  //   return this._lng;
  // }

  // public onCenterChange(event: LatLngLiteral) {
  //   localStorage.map_latitude = event.lat;
  //   localStorage.map_longitude = event.lng;
  // }

  // public styleFunc = (feature: any) => {

  //   // This is the result object
  //   const styleOptions = {
  //     clickable: false,
  //   };

  //   // https://developers.google.com/maps/documentation/javascript/reference/data#Data.StyleOptions
  //   const propNames = ['fillColor', 'fillOpacity', 'icon', 'strokeColor', 'shape', 'strokeOpacity', 'strokeWeight', 'visible'];

  //   // Go over the properties and copy them across
  //   for (const propName of propNames) {
  //     const propValue = feature.getProperty(propName);
  //     if (propValue !== undefined && propValue !== null) {
  //       styleOptions[propName] = propValue;
  //     }
  //   }

  //   // Return the style options
  //   return styleOptions;
  // }

  // private parseJsonString: string;
  // private parseJsonResult: any;
  // public parseJsonError: string;

  // public parseJson(json: string) {
  //   json = json || undefined;
  //   if (this.parseJsonString !== json) {
  //     this.parseJsonString = json;
  //     if (!json) {
  //       delete this.parseJsonResult;
  //       delete this.parseJsonError;
  //     } else {
  //       try {
  //         this.parseJsonResult = JSON.parse(json);
  //         delete this.parseJsonError;
  //       } catch (err) {
  //         this.parseJsonError = err;
  //         delete this.parseJsonResult;
  //       }
  //     }
  //   }

  //   return this.parseJsonResult;
  // }

  // public locationView: 'map' | 'json' = 'map';

  // public onView(view: 'map' | 'json'): void {
  //   this.locationView = view;
  // }

  // public isView(view: 'map' | 'json'): boolean {
  //   return this.locationView === view;
  // }


  /////////////// Attachments - START

  private _pristineModel: string;

  public showAttachmentsErrors(model: ResourceForSave) {
    return !!model && !!model.Attachments &&
      model.Attachments.some(att => !!att.serverErrors);
  }

  private _attachmentsAttachments: ResourceAttachmentForSave[];
  private _attachmentsResult: AttachmentWrapper[];

  public attachmentWrappers(model: ResourceForSave) {
    if (!model || !model.Attachments) {
      return [];
    }

    if (this._attachmentsAttachments !== model.Attachments) {
      this._attachmentsAttachments = model.Attachments;

      this._attachmentsResult = model.Attachments.map(attachment => ({ attachment }));
    }

    return this._attachmentsResult;
  }

  public onFileSelected(input: HTMLInputElement, model: ResourceForSave) {

    const pendingFileSize = this.attachmentWrappers(model)
      .map(a => !!a.file ? a.file.size : 0)
      .reduce((total, v) => total + v, 0);

    onFileSelected(input, pendingFileSize, this.translate).subscribe(wrappers => {
      for (const wrapper of wrappers) {
        // Push it in both the model attachments and the wrapper collection
        model.Attachments.push(wrapper.attachment);
        this.attachmentWrappers(model).push(wrapper);
      }
    }, (errorMsg) => {
      this.details.displayErrorModal(errorMsg);
    });
  }

  public onDeleteAttachment(model: ResourceForSave, index: number) {
    this.attachmentWrappers(model).splice(index, 1);
    model.Attachments.splice(index, 1);
  }

  public onDownloadAttachment(model: ResourceForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.downloading = true; // show a little spinner
      this.resourcesApi.getAttachment(docId, wrapper.attachment.Id).pipe(
        tap(blob => {
          delete wrapper.downloading;
          downloadBlob(blob, this.fileName(wrapper));
        }),
        catchError(friendlyError => {
          delete wrapper.downloading;
          this.details.handleActionError(friendlyError);
          return of(null);
        }),
        finalize(() => {
          delete wrapper.downloading;
        })
      ).subscribe();

    } else if (!!wrapper.file) {
      downloadBlob(wrapper.file, this.fileName(wrapper));
    }
  }

  public onPreviewAttachment(model: ResourceForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.previewing = true; // show a little spinner
      this.resourcesApi.getAttachment(docId, wrapper.attachment.Id).pipe(
        tap(blob => {
          delete wrapper.previewing;
          openOrDownloadBlob(blob, this.fileName(wrapper));
        }),
        catchError(friendlyError => {
          delete wrapper.previewing;
          this.details.handleActionError(friendlyError);
          return of(null);
        }),
        finalize(() => {
          delete wrapper.previewing;
        })
      ).subscribe();

    } else if (!!wrapper.file) {
      openOrDownloadBlob(wrapper.file, this.fileName(wrapper));
    }
  }

  public fileName(wrapper: AttachmentWrapper) {
    const att = wrapper.attachment;
    return !!att.FileName && !!att.FileExtension ? `${att.FileName}.${att.FileExtension}` :
      (att.FileName || (!!wrapper.file ? wrapper.file.name : 'Attachment'));
  }

  public size(wrapper: AttachmentWrapper): string {
    const att = wrapper.attachment;
    return fileSizeDisplay(att.Size || (!!wrapper.file ? wrapper.file.size : null));
  }

  public colorFromExtension(extension: string): string {
    return colorFromExtension(extension);
  }

  public iconFromExtension(extension: string): string {
    return iconFromExtension(extension);
  }

  public registerPristineFunc = (pristineModel: ResourceForSave) => {
    this._pristineModel = JSON.stringify(pristineModel);
  }

  public isDirtyFunc = (model: ResourceForSave) => {
    if (!!model && !!model.Attachments && model.Attachments.some(e => !!e.File)) {
      return true; // Optimization so as not to JSON.stringify large files sized in the megabytes every change detector cycle
    }

    return this._pristineModel !== JSON.stringify(model);
  }

  /////////////// Attachments - END

  public savePreprocessing = (entity: ResourceForSave) => {
    // Server validation on hidden properties will be confusing to the user
    if (this.definition.UnitCardinality !== 'Multiple') {
      entity.Units = [];
    }
  }

  // Embedded Reports
  public showReports(isEdit: boolean, model: Resource) {
    return !!model && !!model.Id;
  }

  public get reports(): DefinitionReportDefinitionForClient[] {
    return this.definition.ReportDefinitions;
  }

  public reportDefinition(e: DefinitionReportDefinitionForClient): ReportDefinitionForClient {
    return this.ws.definitions.Reports[e.ReportDefinitionId];
  }

  public reportTitle(e: DefinitionReportDefinitionForClient): string {
    return this.ws.getMultilingualValueImmediate(e, 'Name') ||
      this.ws.getMultilingualValueImmediate(this.reportDefinition(e), 'Title')
      || this.translate.instant('Untitled');
  }

  public state(e: DefinitionReportDefinitionForClient): ReportStore {
    const stateKey = `resources_details_${this.definitionId}_${e.ReportDefinitionId}`;

    const rs = this.workspace.currentTenant.reportState;
    if (!rs[stateKey]) {
      rs[stateKey] = new ReportStore();
    }

    return rs[stateKey];
  }

  public reportView(e: DefinitionReportDefinitionForClient): ReportView {
    const reportDef = this.reportDefinition(e);
    return !!reportDef && !!reportDef.Chart && reportDef.DefaultsToChart ? ReportView.chart : ReportView.pivot;
  }

  private get activeTabKey(): string {
    return `resources_details_${this.definitionId}_activeTab`;
  }

  public get activeTab(): string {
    const key = this.activeTabKey;
    const miscState = this.ws.miscState;
    if (!miscState[key]) {
      if (this.Units_isVisible) {
        miscState[key] = 'units';
      } else if (this.Attachments_isVisible) {
        miscState[key] = 'attachments';
      } else if (this.Location_isVisible) {
        miscState[key] = 'location';
      } else if (this.reports.length > 0) {
        miscState[key] = this.reports[0].ReportDefinitionId;
      } else {
        miscState[key] = '<unknown>';
      }
    }

    return miscState[key];
  }

  public set activeTab(v: string) {
    this.ws.miscState[this.activeTabKey] = v;
  }

  public onExpandReport(reportId: number, model: Resource) {
    this.router.navigate(['../../../report', reportId, { id: model.Id }], { relativeTo: this.route });
  }
}

interface AttachmentWrapper {
  attachment: ResourceAttachment;
  file?: File;
  downloading?: boolean;
  previewing?: boolean;
}