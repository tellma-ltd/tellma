// tslint:disable:member-ordering
import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { AgentDefinitionForClient, DefinitionsForClient, EmailTemplateForClient, MessageTemplateForClient } from '~/app/data/dto/definitions-for-client';
import { MasterComponent } from '~/app/shared/master/master.component';
import { EmailCommandVersions } from '~/app/data/dto/email-command-preview';

@Component({
  selector: 't-agents-master',
  templateUrl: './agents-master.component.html'
})
export class AgentsMasterComponent extends MasterBaseComponent implements OnInit {

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

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) {
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

  public get c() {
    return this.workspace.currentTenant.Agent;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get definition(): AgentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Agents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !this.definitionId || !!this.definition;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.view, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural') :
      this.translate.instant('Agents');
  }

  public get summary(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitleSingular') :
      this.translate.instant('Agents');
  }

  public get Image_isVisible(): boolean {
    return !!this.definition.ImageVisibility;
  }

  // Emails

  private _emailTemplatesDefinitions: DefinitionsForClient;
  private _emailTemplatesDefinitionId: number;
  private _emailTemplatesResult: EmailTemplateForClient[];

  public get emailTemplates(): EmailTemplateForClient[] {
    const ws = this.ws;
    const collection = 'Agent';
    const defId = this.definitionId;
    const defs = ws.definitions;
    if (this._emailTemplatesDefinitions !== defs ||
      this._emailTemplatesDefinitionId !== defId) {
      this._emailTemplatesDefinitions = ws.definitions;
      this._emailTemplatesDefinitionId = defId;

      this._emailTemplatesResult = Object.values(defs.EmailTemplates || {})
        .filter(e => e.Collection === collection && e.DefinitionId === defId && e.Usage === 'FromSearchAndDetails');
    }

    return this._emailTemplatesResult;
  }

  get showSendEmail(): boolean {
    const templates = this.emailTemplates;
    return !!this.masterContainer && !!templates && templates.length > 0;
  }

  public emailCommandPreview = (template: EmailTemplateForClient) => {
    const ids = this.masterContainer.checkedIds;
    return this.agentsApi.emailCommandPreviewEntities(template.EmailTemplateId, { i: ids });
  }

  public emailPreview = (template: EmailTemplateForClient, index: number, version?: string) => {
    const ids = this.masterContainer.checkedIds;
    return this.agentsApi.emailPreviewEntities(template.EmailTemplateId, index, { i: ids, version });
  }

  public sendEmail = (template: EmailTemplateForClient, versions?: EmailCommandVersions) => {
    const ids = this.masterContainer.checkedIds;
    return this.agentsApi.emailEntities(template.EmailTemplateId, { i: ids }, versions);
  }

  // Messages

  @ViewChild(MasterComponent)
  public masterContainer: any;

  get showSendMessage(): boolean {
    const templates = this.messageTemplates;
    return !!this.masterContainer && !!templates && templates.length > 0;
  }

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
        .filter(e => e.Collection === 'Agent' && e.DefinitionId === defId && (e.Usage === 'FromSearchAndDetails'));
    }

    return this._messageTemplatesResult;
  }

  public messageCommandPreview = (template: MessageTemplateForClient) => {
    const ids = this.masterContainer.checkedIds;
    return this.agentsApi.messageCommandPreviewEntities(template.MessageTemplateId, { i: ids });
  }

  public sendMessage = (template: MessageTemplateForClient, version: string) => {
    const ids = this.masterContainer.checkedIds;
    return this.agentsApi.messageEntities(template.MessageTemplateId, { i: ids }, version)
      .pipe(tap(_ => this.masterContainer.checked = {}));
  }
}
