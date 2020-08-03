// tslint:disable:member-ordering
import { Component, Input, OnInit } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Custody, CustodyForSave } from '~/app/data/entities/custody';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { CustodyDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { LatLngLiteral } from '@agm/core';

@Component({
  selector: 't-custodies-details',
  templateUrl: './custodies-details.component.html'
})
export class CustodiesDetailsComponent extends DetailsBaseComponent implements OnInit {

  private custodiesApi = this.api.custodiesApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.custodiesApi = this.api.custodiesApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
    return this._definitionId;
  }

  @Input()
  previewDefinition: CustodyDefinitionForClient; // Used in preview mode

  public expand = 'Currency,Center,Lookup1,Lookup2,Lookup3,Lookup4,Agent,Custodian';

  create = () => {
    const result: CustodyForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    // TODO Set defaults from definition

    return result;
  }

  clone = (item: Custody): Custody => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Custody;
      clone.Id = null;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private router: Router,
    private route: ActivatedRoute) {
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
    return `custodies/${this.definitionId}`;
  }

  private get definition(): CustodyDefinitionForClient {
    return this.previewDefinition || (!!this.definitionId ? this.ws.definitions.Custodies[this.definitionId] : null);
  }

  // UI Bindings

  public get found(): boolean {
    return !!this.definition;
  }

  public onActivate = (model: Custody): void => {
    if (!!model && !!model.Id) {
      this.custodiesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Custody): void => {
    if (!!model && !!model.Id) {
      this.custodiesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onEditDefinition = (_: Custody) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../custody-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public showActivate = (model: Custody) => !!model && !model.IsActive;
  public showDeactivate = (model: Custody) => !!model && model.IsActive;
  public showEditDefinition = (model: Custody) => this.ws.canDo('custody-definitions', 'Update', null);

  public canActivateDeactivateItem = (model: Custody) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Custody) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  // Shared with Resource

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

  public get Custodian_isVisible(): boolean {
    return !!this.definition.CustodianVisibility;
  }

  public get Custodian_isRequired(): boolean {
    return this.definition.CustodianVisibility === 'Required';
  }

  public get Custodian_label(): string {
    const def = this.definition;
    const custodianDefId = def.CustodianDefinitionId;
    const custodianDef = this.ws.definitions.Relations[custodianDefId];
    if (!!custodianDef) {
      return this.ws.getMultilingualValueImmediate(custodianDef, 'TitleSingular');
    } else {
      return this.translate.instant('Custody_Custodian');
    }
  }

  public get Custodian_definitionIds(): number[] {
    return [this.definition.CustodianDefinitionId];
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

  // Custody Only

  public get Agent_isVisible(): boolean {
    return !!this.definition.AgentVisibility;
  }

  public get Agent_isRequired(): boolean {
    return this.definition.AgentVisibility === 'Required';
  }

  public get TaxIdentificationNumber_isVisible(): boolean {
    return !!this.definition.TaxIdentificationNumberVisibility;
  }

  public get TaxIdentificationNumber_isRequired(): boolean {
    return this.definition.TaxIdentificationNumberVisibility === 'Required';
  }

  public get Job_isVisible(): boolean {
    return !!this.definition.JobVisibility;
  }

  public get Job_isRequired(): boolean {
    return this.definition.JobVisibility === 'Required';
  }

  public get BankAccountNumber_isVisible(): boolean {
    return !!this.definition.BankAccountNumberVisibility;
  }

  public get BankAccountNumber_isRequired(): boolean {
    return this.definition.BankAccountNumberVisibility === 'Required';
  }

  public get showTabs(): boolean {
    return this.Location_isVisible;
  }

  // Location + Map stuff

  public get Location_isVisible(): boolean {
    return !!this.definition.LocationVisibility;
  }

  public Map_showError(model: CustodyForSave): boolean {
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

}
