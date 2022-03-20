// tslint:disable:member-ordering
import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DefinitionsForClient, DocumentDefinitionForClient, EmailTemplateForClient, MessageTemplateForClient } from '~/app/data/dto/definitions-for-client';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { Observable } from 'rxjs';
import { Document } from '~/app/data/entities/document';
import { EmailCommandVersions } from '~/app/data/dto/email-command-preview';
import { MasterComponent } from '~/app/shared/master/master.component';

@Component({
  selector: 't-documents-master',
  templateUrl: './documents-master.component.html',
  styles: []
})
export class DocumentsMasterComponent extends MasterBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  // caching
  private _selectDefaultDefinition: DocumentDefinitionForClient;
  private _selectDefaultResult: string;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.documentsApi = this.api.documentsApi(t, this.notifyDestruct$);
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
        this.definitionId = +params.get('definitionId');
      }
    });
  }

  get view(): string {
    return `documents/${this.definitionId}`;
  }

  public get c() {
    return this.ws.Document;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !this.definitionId || !!this.definition;
  }

  public get masterCrumb(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural') :
      this.translate.instant('Documents');
  }

  // State

  public onClose = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.close(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onCancel = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.cancel(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onOpen = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.open(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onUncancel = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.uncancel(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showClose = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 0;
    });
  }

  public showCancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 0;
    });
  }

  public showOpen = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 1;
    });
  }

  public showUncancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === -1;
    });
  }

  public hasStatePermission = (_: (number | string)[]) => this.ws.canDo(this.view, 'State', null);

  public stateTooltip = (ids: (number | string)[]) => this.hasStatePermission(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get selectDefault(): string {
    const def = this.definition;
    if (this._selectDefaultDefinition !== def) {
      this._selectDefaultDefinition = def;
      let result = 'State,Assignee,AssignedAt';
      if (!!def && !!def.PostingDateVisibility) {
        result = 'PostingDate,' + result;
      }
      if (!!def && !!def.MemoVisibility) {
        result += ',Memo';
      }

      this._selectDefaultResult = result;
    }

    return this._selectDefaultResult;
  }

  public get showDocumentPostingDate(): boolean {
    const def = this.definition;
    return !def || !!def.PostingDateVisibility;
  }

  public get showDocumentMemo(): boolean {
    const def = this.definition;
    return !def || !!def.MemoVisibility;
  }

  // Emails

  private _emailTemplatesDefinitions: DefinitionsForClient;
  private _emailTemplatesDefinitionId: number;
  private _emailTemplatesResult: EmailTemplateForClient[];

  public get emailTemplates(): EmailTemplateForClient[] {
    const ws = this.ws;
    const collection = 'Document';
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
    return this.documentsApi.emailCommandPreviewEntities(template.EmailTemplateId, { i: ids });
  }

  public emailPreview = (template: EmailTemplateForClient, index: number, version?: string) => {
    const ids = this.masterContainer.checkedIds;
    return this.documentsApi.emailPreviewEntities(template.EmailTemplateId, index, { i: ids, version });
  }

  public sendEmail = (template: EmailTemplateForClient, versions?: EmailCommandVersions) => {
    const ids = this.masterContainer.checkedIds;
    return this.documentsApi.emailEntities(template.EmailTemplateId, { i: ids }, versions);
  }

  @ViewChild(MasterComponent)
  public masterContainer: any;

  // Messages

  get showSendMessage(): boolean {
    const templates = this.messageTemplates;
    return !!this.masterContainer && !!templates && templates.length > 0;
  }

  private _messageTemplatesDefinitions: DefinitionsForClient;
  private _messageTemplatesDefinitionId: number;
  private _messageTemplatesResult: MessageTemplateForClient[];

  public get messageTemplates(): MessageTemplateForClient[] {
    const collection = 'Document';
    const defs = this.workspace.currentTenant.definitions;
    const defId = this.definitionId;
    if (this._messageTemplatesDefinitions !== defs ||
      this._messageTemplatesDefinitionId !== defId) {
      this._messageTemplatesDefinitions = defs;
      this._messageTemplatesDefinitionId = defId;

      this._messageTemplatesResult = Object.values(defs.MessageTemplates || {})
        .filter(e => e.Collection === collection && e.DefinitionId === defId && e.Usage === 'FromSearchAndDetails');
    }

    return this._messageTemplatesResult;
  }

  public messageCommandPreview = (template: MessageTemplateForClient) => {
    const ids = this.masterContainer.checkedIds;
    return this.documentsApi.messageCommandPreviewEntities(template.MessageTemplateId, { i: ids });
  }

  public sendMessage = (template: MessageTemplateForClient, version: string) => {
    const ids = this.masterContainer.checkedIds;
    return this.documentsApi.messageEntities(template.MessageTemplateId, { i: ids }, version)
      .pipe(tap(_ => this.masterContainer.checked = {}));
  }
}
