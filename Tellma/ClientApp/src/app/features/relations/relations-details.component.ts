// tslint:disable:member-ordering
import { Component, Input, OnInit } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Relation, RelationForSave } from '~/app/data/entities/relation';
import { addToWorkspace } from '~/app/data/util';
import { ReportStore, WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import {
  DefinitionReportDefinitionForClient,
  RelationDefinitionForClient, ReportDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { LatLngLiteral } from '@agm/core';
import { ReportView } from '../report-results/report-results.component';

@Component({
  selector: 't-relations-details',
  templateUrl: './relations-details.component.html'
})
export class RelationsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private relationsApi = this.api.relationsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.relationsApi = this.api.relationsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
    return this._definitionId;
  }

  @Input()
  previewDefinition: RelationDefinitionForClient; // Used in preview mode

  // public expand = 'User,Rates/Resource,Rates/Unit,Rates/Currency';
  public expand = 'Currency,Center,Lookup1,Lookup2,Lookup3,Lookup4,Agent,Users/User';

  create = () => {
    const result: RelationForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    // TODO Set defaults from definition

    result.Users = [];
    return result;
  }

  clone = (item: Relation): Relation => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Relation;
      clone.Id = null;

      if (!!clone.Users) {
        clone.Users.forEach(e => {
          e.Id = null;
          delete e.RelationId;
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
    return `relations/${this.definitionId}`;
  }

  private get definition(): RelationDefinitionForClient {
    return this.previewDefinition || (!!this.definitionId ? this.ws.definitions.Relations[this.definitionId] : null);
  }

  // UI Bindings

  public get found(): boolean {
    return !!this.definition;
  }

  public onActivate = (model: Relation): void => {
    if (!!model && !!model.Id) {
      this.relationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Relation): void => {
    if (!!model && !!model.Id) {
      this.relationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onEditDefinition = (_: Relation) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../relation-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public showActivate = (model: Relation) => !!model && !model.IsActive;
  public showDeactivate = (model: Relation) => !!model && model.IsActive;
  public showEditDefinition = (model: Relation) => this.ws.canDo('relation-definitions', 'Update', null);

  public canActivateDeactivateItem = (model: Relation) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Relation) => this.canActivateDeactivateItem(model) ? '' :
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

  // Relation Only

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

  public get User_isVisible(): boolean {
    return this.definition.UserCardinality === 'Single';
  }

  public getUserId(model: RelationForSave): number {
    if (!!model && !!model.Users && !!model.Users[0]) {
      return model.Users[0].UserId;
    }

    return undefined;
  }

  public setUserId(model: RelationForSave, userId: number): void {
    if (!!model) {
      if (!!userId) {
        model.Users = [{ UserId: userId }];
      } else {
        model.Users = [];
      }
    }
  }

  public get Users_isVisible(): boolean {
    return this.definition.UserCardinality === 'Multiple';
  }

  public Users_count(model: RelationForSave): number {
    return !!model && !!model.Users ? model.Users.length : 0;
  }

  public Users_showError(model: RelationForSave): boolean {
    return !!model && !!model.Users && model.Users.some(e => !!e.serverErrors);
  }

  public showTabs(isEdit: boolean, model: Relation): boolean {
    return this.Users_isVisible || this.Location_isVisible || (this.reports.length > 0 && this.showReports(isEdit, model));
  }

  // Location + Map stuff

  public get Location_isVisible(): boolean {
    return !!this.definition.LocationVisibility;
  }

  public Map_showError(model: RelationForSave): boolean {
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

  // Embedded Reports
  public showReports(isEdit: boolean, model: Relation) {
    return true; // !isEdit && !!model && !!model.Id;
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
    const stateKey = `relations_details_${this.definitionId}_${e.ReportDefinitionId}`;

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
    return `relations_details_${this.definitionId}_activeTab`;
  }

  public get activeTab(): string {
    const key = this.activeTabKey;
    const miscState = this.ws.miscState;
    if (!miscState[key]) {
      if (this.Users_isVisible) {
        miscState[key] = 'users';
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

  public onExpandReport(reportId: number, model: Relation) {
    this.router.navigate(['../../../report', reportId, { Id: model.Id }], { relativeTo: this.route });
  }
}
