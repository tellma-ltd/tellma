// tslint:disable:member-ordering
import { Component, Input, OnInit } from '@angular/core';
import { catchError, finalize, tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave } from '~/app/data/entities/agent';
import {
  addToWorkspace, colorFromExtension, openOrDownloadBlob, fileSizeDisplay, iconFromExtension, onFileSelected, downloadBlob
} from '~/app/data/util';
import { ReportStore, WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import {
  DefinitionReportDefinitionForClient,
  AgentDefinitionForClient, ReportDefinitionForClient, MessageTemplateForClient, DefinitionsForClient
} from '~/app/data/dto/definitions-for-client';
import { ReportView } from '../report-results/report-results.component';
import { AgentAttachment, AgentAttachmentForSave } from '~/app/data/entities/agent-attachment';
import { Observable, of } from 'rxjs';
import { IdResult } from '~/app/data/dto/id-result';
import { MessageCommandPreview } from '~/app/data/dto/message-command-preview';

@Component({
  selector: 't-agents-details',
  templateUrl: './agents-details.component.html'
})
export class AgentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private agentsApi = this.api.agentsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.agentsApi = this.api.agentsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
    return this._definitionId;
  }

  @Input()
  previewDefinition: AgentDefinitionForClient; // Used in preview mode

  public expand = `Currency,Center,Lookup1,Lookup2,Lookup3,Lookup4,Lookup5,Lookup6,Lookup7,Lookup8,
User,Agent1,Users.User,Attachments.Category,Attachments.CreatedBy`;

  create = () => {
    const result: AgentForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    // TODO Set defaults from definition

    result.Users = [];
    result.Attachments = [];
    return result;
  }

  clone = (item: Agent): Agent => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Agent;
      delete clone.Id;
      clone.Attachments = []; // Attachments can't be cloned

      if (!!clone.Users) {
        clone.Users.forEach(e => {
          delete e.Id;
          delete e.AgentId;
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
    return `agents/${this.definitionId}`;
  }

  private get definition(): AgentDefinitionForClient {
    return this.previewDefinition || (!!this.definitionId ? this.ws.definitions.Agents[this.definitionId] : null);
  }

  // UI Bindings

  public get found(): boolean {
    return !!this.definition;
  }

  public onActivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onEditDefinition = (_: Agent) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../agent-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public showActivate = (model: Agent) => !!model && !model.IsActive;
  public showDeactivate = (model: Agent) => !!model && model.IsActive;
  public showEditDefinition = (model: Agent) => this.ws.canDo('agent-definitions', 'Update', null);

  public canActivateDeactivateItem = (model: Agent) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Agent) => this.canActivateDeactivateItem(model) ? '' :
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

  public get Lookup5_isVisible(): boolean {
    return !!this.definition.Lookup5Visibility;
  }

  public get Lookup5_isRequired(): boolean {
    return this.definition.Lookup5Visibility === 'Required';
  }

  public get Lookup5_label(): string {
    return !!this.definition.Lookup5Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup5Label') :
      this.translate.instant('Entity_Lookup5');
  }

  public get Lookup5_DefinitionId(): number {
    return this.definition.Lookup5DefinitionId;
  }

  public get Lookup6_isVisible(): boolean {
    return !!this.definition.Lookup6Visibility;
  }

  public get Lookup6_isRequired(): boolean {
    return this.definition.Lookup6Visibility === 'Required';
  }

  public get Lookup6_label(): string {
    return !!this.definition.Lookup6Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup6Label') :
      this.translate.instant('Entity_Lookup6');
  }

  public get Lookup6_DefinitionId(): number {
    return this.definition.Lookup6DefinitionId;
  }

  public get Lookup7_isVisible(): boolean {
    return !!this.definition.Lookup7Visibility;
  }

  public get Lookup7_isRequired(): boolean {
    return this.definition.Lookup7Visibility === 'Required';
  }

  public get Lookup7_label(): string {
    return !!this.definition.Lookup7Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup7Label') :
      this.translate.instant('Entity_Lookup7');
  }

  public get Lookup7_DefinitionId(): number {
    return this.definition.Lookup7DefinitionId;
  }

  public get Lookup8_isVisible(): boolean {
    return !!this.definition.Lookup8Visibility;
  }

  public get Lookup8_isRequired(): boolean {
    return this.definition.Lookup8Visibility === 'Required';
  }

  public get Lookup8_label(): string {
    return !!this.definition.Lookup8Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Lookup8Label') :
      this.translate.instant('Entity_Lookup8');
  }

  public get Lookup8_DefinitionId(): number {
    return this.definition.Lookup8DefinitionId;
  }

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

  public get Text3_isVisible(): boolean {
    return !!this.definition.Text3Visibility;
  }

  public get Text3_isRequired(): boolean {
    return this.definition.Text3Visibility === 'Required';
  }

  public get Text3_label(): string {
    return !!this.definition.Text3Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Text3Label') :
      this.translate.instant('Entity_Text3');
  }

  public get Text4_isVisible(): boolean {
    return !!this.definition.Text4Visibility;
  }

  public get Text4_isRequired(): boolean {
    return this.definition.Text4Visibility === 'Required';
  }

  public get Text4_label(): string {
    return !!this.definition.Text4Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Text4Label') :
      this.translate.instant('Entity_Text4');
  }

  // Agent Only

  public get TaxIdentificationNumber_isVisible(): boolean {
    return !!this.definition.TaxIdentificationNumberVisibility;
  }

  public get TaxIdentificationNumber_isRequired(): boolean {
    return this.definition.TaxIdentificationNumberVisibility === 'Required';
  }

  public get ExternalReference_label(): boolean {
    return !!this.definition.ExternalReferenceLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'ExternalReferenceLabel') :
      this.translate.instant('Agent_ExternalReference');
  }

  public get ExternalReference_isVisible(): boolean {
    return !!this.definition.ExternalReferenceVisibility;
  }

  public get ExternalReference_isRequired(): boolean {
    return this.definition.ExternalReferenceVisibility === 'Required';
  }

  public get BankAccountNumber_isVisible(): boolean {
    return !!this.definition.BankAccountNumberVisibility;
  }

  public get BankAccountNumber_isRequired(): boolean {
    return this.definition.BankAccountNumberVisibility === 'Required';
  }

  public get DateOfBirth_isVisible(): boolean {
    return !!this.definition.DateOfBirthVisibility;
  }

  public get DateOfBirth_isRequired(): boolean {
    return this.definition.DateOfBirthVisibility === 'Required';
  }

  public get ContactEmail_isVisible(): boolean {
    return !!this.definition.ContactEmailVisibility;
  }

  public get ContactEmail_isRequired(): boolean {
    return this.definition.ContactEmailVisibility === 'Required';
  }

  public get ContactMobile_isVisible(): boolean {
    return !!this.definition.ContactMobileVisibility;
  }

  public get ContactMobile_isRequired(): boolean {
    return this.definition.ContactMobileVisibility === 'Required';
  }

  public get ContactAddress_isVisible(): boolean {
    return !!this.definition.ContactAddressVisibility;
  }

  public get ContactAddress_isRequired(): boolean {
    return this.definition.ContactAddressVisibility === 'Required';
  }

  public get Agent1_isVisible(): boolean {
    return !!this.definition.Agent1Visibility;
  }

  public get Agent1_isRequired(): boolean {
    return this.definition.Agent1Visibility === 'Required';
  }

  public get Agent1_label(): string {
    return !!this.definition.Agent1Label ?
      this.ws.getMultilingualValueImmediate(this.definition, 'Agent1Label') :
      this.translate.instant('Entity_Agent1');
  }

  public get Agent1_DefinitionIds(): number[] {
    if (!!this.definition.Agent1DefinitionId) {
      return [this.definition.Agent1DefinitionId];
    } else {
      return [];
    }
  }

  public showTabs(isEdit: boolean, model: Agent): boolean {
    return this.Users_isVisible || this.Location_isVisible || this.Attachments_isVisible
      || (this.reports.length > 0 && this.showReports(isEdit, model));
  }

  public get User_isVisible(): boolean {
    return !!this.definition.UserCardinality;
  }

  public get Users_isVisible(): boolean {
    return this.definition.UserCardinality === 'Multiple';
  }

  public Users_count(model: AgentForSave): number {
    return !!model && !!model.Users ? model.Users.length : 0;
  }

  public Users_showError(model: AgentForSave): boolean {
    return !!model && !!model.Users && model.Users.some(e => !!e.serverErrors);
  }

  // Attachments

  public get Attachments_isVisible(): boolean {
    return !!this.definition.HasAttachments;
  }

  public Attachments_count(model: AgentForSave): number {
    return !!model && !!model.Attachments ? model.Attachments.length : 0;
  }

  public Attachments_showError(model: AgentForSave): boolean {
    return !!model && !!model.Users && model.Users.some(e => !!e.serverErrors);
  }

  // // Location + Map stuff

  public get Location_isVisible(): boolean {
    return !!this.definition.LocationVisibility;
  }

  public Map_showError(model: AgentForSave): boolean {
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

  // Attachments


  // Embedded Reports

  public showReports(isEdit: boolean, model: Agent) {
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
    const stateKey = `agents_details_${this.definitionId}_${e.ReportDefinitionId}`;

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
    return `agents_details_${this.definitionId}_activeTab`;
  }

  public get activeTab(): string {
    const key = this.activeTabKey;
    const miscState = this.ws.miscState;
    if (!miscState[key]) {
      if (this.Users_isVisible) {
        miscState[key] = 'users';
      } else if (this.Attachments_isVisible) {
        miscState[key] = 'attachments';
      } else if (this.Location_isVisible) {
        miscState[key] = 'location';
      } else if (this.reports.length > 0) {
        miscState[key] = this.reports[0].ReportDefinitionId;
      }
    }

    return miscState[key];
  }

  public set activeTab(v: string) {
    this.ws.miscState[this.activeTabKey] = v;
  }

  public onExpandReport(reportId: number, model: Agent) {
    this.router.navigate(['../../../report', reportId, { id: model.Id }], { relativeTo: this.route });
  }

  /////////////// Attachments - START

  private _pristineModel: string;

  public showAttachmentsErrors(model: AgentForSave) {
    return !!model && !!model.Attachments &&
      model.Attachments.some(att => !!att.serverErrors);
  }

  private _attachmentsAttachments: AgentAttachmentForSave[];
  private _attachmentsResult: AttachmentWrapper[];

  public attachmentWrappers(model: AgentForSave) {
    if (!model || !model.Attachments) {
      return [];
    }

    if (this._attachmentsAttachments !== model.Attachments) {
      this._attachmentsAttachments = model.Attachments;

      this._attachmentsResult = model.Attachments.map(attachment => ({ attachment }));
    }

    return this._attachmentsResult;
  }

  public onFileSelected(input: HTMLInputElement, model: AgentForSave) {

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

  public onDeleteAttachment(model: AgentForSave, index: number) {
    this.attachmentWrappers(model).splice(index, 1);
    model.Attachments.splice(index, 1);
  }

  public onDownloadAttachment(model: AgentForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.downloading = true; // show a little spinner
      this.agentsApi.getAttachment(docId, wrapper.attachment.Id).pipe(
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

  public onPreviewAttachment(model: AgentForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.previewing = true; // show a little spinner
      this.agentsApi.getAttachment(docId, wrapper.attachment.Id).pipe(
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

  public registerPristineFunc = (pristineModel: AgentForSave) => {
    this._pristineModel = JSON.stringify(pristineModel);
  }

  public isDirtyFunc = (model: AgentForSave) => {
    if (!!model && !!model.Attachments && model.Attachments.some(e => !!e.File)) {
      return true; // Optimization so as not to JSON.stringify large files sized in the megabytes every change detector cycle
    }

    return this._pristineModel !== JSON.stringify(model);
  }

  /////////////// Attachments - END

  public savePreprocessing = (entity: AgentForSave) => {
    // Server validation on hidden properties will be confusing to the user
    if (this.definition.UserCardinality !== 'Multiple') {
      entity.Users = [];
    }
  }

  // Message

  private _messageTemplatesDefinitions: DefinitionsForClient;
  private _messageTemplatesDefinitionId: number;
  private _messageTemplatesResult: MessageTemplateForClient[];

  public get messageTemplates(): MessageTemplateForClient[] {
    const defs = this.workspace.currentTenant.definitions;
    const defId = this.definitionId;
    if (this._messageTemplatesDefinitions !== defs ||
      this._messageTemplatesDefinitionId !== defId) {
      this._messageTemplatesDefinitions = defs;
      this._messageTemplatesDefinitionId = defId;

      this._messageTemplatesResult = Object.values(defs.MessageTemplates || {})
        .filter(e => e.Collection === 'Agent' && e.DefinitionId === defId && (e.Usage === 'FromDetails' || e.Usage === 'FromSearchAndDetails'));
    }

    return this._messageTemplatesResult;
  }

  private _messageCommandPreviewId: string | number;
  private _messageCommandPreview: (t: MessageTemplateForClient) => Observable<MessageCommandPreview>;

  public messageCommandPreviewFactory(id: number) {
    if (!id) {
      delete this._messageCommandPreviewId;
      delete this._messageCommandPreview;
    } else if (this._messageCommandPreviewId !== id) {
      this._messageCommandPreviewId = id;
      this._messageCommandPreview = (template: MessageTemplateForClient) => {
        return template.Usage === 'FromSearchAndDetails' ?
          this.agentsApi.messageCommandPreviewEntities(template.MessageTemplateId, { i: [id] }) :
          this.agentsApi.messageCommandPreviewEntity(id, template.MessageTemplateId, {});
      };
    }

    return this._messageCommandPreview;
  }

  private _sendMessageId: string | number;
  private _sendMessage: (t: MessageTemplateForClient, v?: string) => Observable<IdResult>;

  public sendMessageFactory(id: number) {
    if (!id) {
      delete this._sendMessageId;
      delete this._sendMessage;
    } else if (this._sendMessageId !== id) {
      this._sendMessageId = id;

      this._sendMessage = (template: MessageTemplateForClient, version?: string) => {
        return template.Usage === 'FromSearchAndDetails' ?
          this.agentsApi.messageEntities(template.MessageTemplateId, { i: [id] }, version) :
          this.agentsApi.messageEntity(id, template.MessageTemplateId, {}, version);
      };
    }

    return this._sendMessage;
  }
}

interface AttachmentWrapper {
  attachment: AgentAttachment;
  file?: File;
  downloading?: boolean;
  previewing?: boolean;
}
