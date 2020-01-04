import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { DocumentForSave, Document, serialNumber } from '~/app/data/entities/document';
import { DocumentDefinitionForClient, ResourceDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { LineForSave } from '~/app/data/entities/line';
import { EntryForSave, Entry } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import { addToWorkspace } from '~/app/data/util';
import { tap, catchError } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of } from 'rxjs';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { AccountForSave } from '~/app/data/entities/account';
import { Resource, metadata_Resource, ResourceForSave } from '~/app/data/entities/resource';
import { Currency } from '~/app/data/entities/currency';
import { metadata_Agent } from '~/app/data/entities/agent';
import { ResourceClassification } from '~/app/data/entities/resource-classification';

interface DocumentEventBase {
  time: string;
  userId: number;
}

interface DocumentReassignmentEvent extends DocumentEventBase {
  type: 'reassignment';
  assigneeId: number;
  comment?: string;
}

interface DocumentCreationEvent extends DocumentEventBase {
  type: 'creation';
}

type DocumentEvent = DocumentReassignmentEvent | DocumentCreationEvent;

@Component({
  selector: 'b-documents-details',
  templateUrl: './documents-details.component.html',
  styles: []
})
export class DocumentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;
  private _currentDoc: Document;
  private _sortedHistory: { date: string, events: DocumentEvent[] }[] = [];
  private _stateChoices: SelectorChoice[];

  // These are bound from UI
  public assigneeId: number;
  public comment: string;
  public picSize = 36;

  public toState: number;
  public reasonId: number;
  public reasonDetails: string;
  public onBehalfOfUserId: number;
  public roleId: number;
  public signedAt: string = null;


  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this.documentsApi = this.api.documentsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  @ViewChild('signModal', { static: true })
  signModal: TemplateRef<any>;

  public expand2 = `CreatedBy,ModifiedBy` +
    `Lines/Entries/Account/Currency,Lines/Entries/Account/Resource/Currency,
  Lines/Entries/Account/Agent,Lines/Entries/Account/ResourceClassification,Lines/Entries/Resource/Currency,
  Signatures/OnBehalfOfUser,Signatures/Role,Signatures/CreatedBy,AssignmentsHistory/Assignee,AssignmentsHistory/CreatedBy`;

  public expand = 'CreatedBy,ModifiedBy,' +
    // Entry Account
    ['Currency', 'Resource/Currency', 'Resource/CountUnit', 'Resource/MassUnit',
      'Resource/VolumeUnit', 'Resource/TimeUnit', 'Agent', 'ResourceClassification']
      .map(prop => `Lines/Entries/Account/${prop}`).join(',') + ',' +

    // Entry
    ['Currency', 'Resource/Currency', 'Resource/CountUnit', 'Resource/MassUnit',
      'Resource/VolumeUnit', 'Resource/TimeUnit', 'Agent']
      .map(prop => `Lines/Entries/${prop}`).join(',') + ',' +

    // Signatures
    ['OnBehalfOfUser', 'Role', 'CreatedBy']
      .map(prop => `Signatures/${prop}`).join(',') + ',' +

    // Assignment history
    ['Assignee', 'CreatedBy']
      .map(prop => `AssignmentsHistory/${prop}`).join(',');

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
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
    return `documents/${this.definitionId}`;
  }

  // UI Binding

  private get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.workspace.current.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result = new DocumentForSave();
    result.Memo = this.initialText;
    result.DocumentDate = new Date().toISOString().split('T')[0];
    result.MemoIsCommon = true;

    result.Lines = [];

    // const defs = this.definition;
    // TODO: Set defaults

    return result;
  }

  clone: (item: Document) => Document = (item: Document) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Document;
      clone.Id = null;

      if (!!clone.Lines) {
        clone.Lines.forEach(line => {
          line.Id = null;
          if (!!line.Entries) {
            line.Entries.forEach(entry => {
              entry.Id = null;
            });
          }
        });
      }

      clone.AssignmentsHistory = [];

      delete clone.AssigneeId;
      delete clone.CreatedById;
      delete clone.ModifiedById;
      delete clone.SerialNumber;
      delete clone.State;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  public get ws() {
    return this.workspace.current;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public formatSerial(serial: number) {
    if (!serial) {
      return `(${this.translate.instant('New')})`;
    }
    const def = this.definition;
    return serialNumber(serial, def.Prefix, 4);
  }

  // TODO
  isInactive(_: DocumentForSave) {
    return null;
  }

  showLineErrors(_: Document) {
    return false;
  }

  onNewLine(item: LineForSave) {
    item.Entries = [new EntryForSave()];
    item.DefinitionId = 'ManualLine';
    return item;
  }

  getDebit(entry: Entry): number {
    return this.isDebit(entry) ? entry.Value : null;
  }

  setDebit(entry: Entry, v: number): void {
    entry.Value = v;
    entry.Direction = 1;
  }

  getCredit(entry: Entry): number {
    return this.isCredit(entry) ? entry.Value : null;
  }

  setCredit(entry: Entry, v: number): void {
    entry.Value = v;
    entry.Direction = -1;
  }

  isCredit(entry: Entry) {
    return entry.Direction === -1;
  }

  isDebit(entry: Entry) {
    return entry.Direction === 1;
  }

  public columnPaths(model: DocumentForSave): string[] {
    const paths = ['AccountId', 'Debit', 'Credit', 'Dynamic'];

    if (this.ws.settings.IsMultiResponsibilityCenter) {
      paths.splice(1, 0, 'ResponsibilityCenter');
    }

    if (!model.MemoIsCommon) {
      paths.unshift('Memo');
    }

    return paths;
  }

  public get functionalPostfix(): string {
    return ' (' + this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName') + ')';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public sortChronologically(model: Document): { date: string, events: DocumentEvent[] }[] {
    if (!model) {
      return null;
    }

    if (model !== this._currentDoc) {
      this._currentDoc = model;
      const history = model.AssignmentsHistory || [];

      const filteredHistory: DocumentAssignment[] = history; // .filter(e => e.CreatedById !== e.AssigneeId);

      const mappedHistory: DocumentEvent[] = filteredHistory.map(e =>
        ({
          type: 'reassignment',
          time: e.CreatedAt,
          userId: e.CreatedById,
          assigneeId: e.AssigneeId,
          comment: e.Comment,
        }));

      if (!!model.CreatedById) {
        mappedHistory.push({ type: 'creation', userId: model.CreatedById, time: model.CreatedAt });
      }

      const sortedHistory: DocumentEvent[] = mappedHistory.sort((a, b) => {
        return a.time < b.time ? 1 :
          a.time > b.time ? -1 : 0;
      });

      const result: { [date: string]: DocumentEvent[] } = {};
      for (const entry of sortedHistory) {
        const date = entry.time.split('T')[0];
        if (!result[date]) {
          result[date] = [];
        }

        result[date].push(entry);
      }

      this._sortedHistory = Object.keys(result).map(date => ({ date, events: result[date] }));
    }

    return this._sortedHistory;
  }

  public reassignment(event: DocumentEvent): DocumentReassignmentEvent {
    return event as DocumentReassignmentEvent;
  }

  public showAssignDocument(doc: Document) {
    // return true;
    return !!doc && !!doc.AssigneeId; // === this.ws.userSettings.UserId;
  }

  public onAssign(doc: Document): void {
    if (!!this.assigneeId) {
      this.documentsApi.assign([doc.Id], {
        returnEntities: true,
        expand: this.expand,
        assigneeId: this.assigneeId,
        comment: this.comment
      }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });

    }
  }

  public canAssign(_: Document, isEdit: boolean) {
    return !!this.assigneeId && !isEdit;
  }

  // Used by signatures and assignments
  public get marginLeft(): number {
    return this.workspace.ws.isRtl ? 0 : 36;
  }

  public get marginRight(): number {
    return this.workspace.ws.isRtl ? 36 : 0;
  }

  public onSign = (model: Document): void => {
    if (!!model && !!model.Id) {
      this.modalService.open(this.signModal)
        .result.then(
          () => this.onConfirmSign(model),
          (_: any) => { }
        );
    }
  }

  public onConfirmSign = (model: Document): void => {
    const lineIds = model.Lines.map(l => l.Id);
    this.documentsApi.sign(lineIds, {
      returnEntities: true,
      expand: this.expand,
      onBehalfOfUserId: this.onBehalfOfUserId,
      toState: this.toState,
      roleId: this.roleId,
      reasonDetails: this.reasonDetails,
      reasonId: this.reasonId,
      signedAt: this.signedAt
    }).pipe(
      tap(res => addToWorkspace(res, this.workspace)),
      catchError(friendlyError => {
        this.details.handleActionError(friendlyError); return of(null);
      })
    ).subscribe();
  }

  public get canConfirmSign() {
    return !!this.roleId && this.toState;
  }

  get stateChoices(): SelectorChoice[] {

    if (!this._stateChoices) {
      /*
Document_State_Draft
Document_State_Void
Document_State_Requested
Document_State_Rejected
Document_State_Authorized
Document_State_Failed
Document_State_Completed
Document_State_Invalid
Document_State_Reviewed
Document_State_Closed
      */

      this._stateChoices = [
        { value: 0, name: () => this.translate.instant('Document_State_Draft') },
        { value: -1, name: () => this.translate.instant('Document_State_Void') },
        { value: 1, name: () => this.translate.instant('Document_State_Requested') },
        { value: -2, name: () => this.translate.instant('Document_State_Rejected') },
        { value: 2, name: () => this.translate.instant('Document_State_Authorized') },
        { value: -3, name: () => this.translate.instant('Document_State_Failed') },
        { value: 3, name: () => this.translate.instant('Document_State_Completed') },
        { value: -4, name: () => this.translate.instant('Document_State_Invalid') },
        { value: 4, name: () => this.translate.instant('Document_State_Reviewed') },
        //    { value: 5,  name: () => this.translate.instant('Document_State_Closed') },
      ];
    }

    return this._stateChoices;
  }

  public showSign = (model: Document) => true; // !!model && !model.IsActive;

  public canSign = (model: Document) => true; // this.ws.canDo(this.definitionId, 'IsActive', model.Id);

  public signTooltip = (model: Document) => ''; // this.canActivateDeactivateItem(model) ? '' :
  // this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public account(entry: Entry): AccountForSave {
    return this.ws.get('Account', entry.AccountId) as AccountForSave;
  }

  public resource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const resourceId = accountResourceId || entry.ResourceId;
    return this.ws.get('Resource', resourceId) as Resource;
  }

  // AgentId

  public showAgent(entry: Entry): boolean {
    const account = this.ws.get('Account', entry.AccountId) as AccountForSave;
    return !!account && !!account.AgentDefinitionId;
  }

  public agentLabel(entry: Entry): string {
    const account = this.ws.get('Account', entry.AccountId) as AccountForSave;
    const agentDefinitionId = !!account ? account.AgentDefinitionId : null;

    return metadata_Agent(this.ws, this.translate, agentDefinitionId).titleSingular();
  }

  // ResourceId

  public showResource(entry: Entry): boolean {
    const account = this.ws.get('Account', entry.AccountId) as AccountForSave;
    return !!account && !!account.ResourceClassificationId;
  }

  public resourceDefinitionIds(entry: Entry): string[] {
    const account = this.ws.get('Account', entry.AccountId) as AccountForSave;
    const resourceClassificationId = !!account ? account.ResourceClassificationId : null;
    const resourceClassification = this.ws.get('ResourceClassification', resourceClassificationId) as ResourceClassification;

    return !!resourceClassification ? [resourceClassification.ResourceDefinitionId] : [];
  }

  public resourceLabel(entry: Entry): string {
    const resourceDefinitionIds = this.resourceDefinitionIds(entry);
    const resourceDefinitionId = resourceDefinitionIds[0];

    return metadata_Resource(this.ws, this.translate, resourceDefinitionId).titleSingular();
  }

  // DueDate

  private resourceDefinition(entry: Entry): ResourceDefinitionForClient {
    const resource = this.resource(entry);
    const defId = !!resource ? resource.DefinitionId : null;
    const resourceDefinition = !!defId ? this.ws.definitions.Resources[defId] : null;
    return resourceDefinition;
  }

  public showDueDate(entry: Entry): boolean {
    const resourceDefinition = this.resourceDefinition(entry);
    return !!resourceDefinition && !!resourceDefinition.DueDateVisibility;
  }

  public requireDueDate(entry: Entry): boolean {
    const resourceDefinition = this.resourceDefinition(entry);
    return !!resourceDefinition && resourceDefinition.DueDateVisibility === 'Required';
  }

  public labelDueDate(entry: Entry): string {
    const resourceDefinition = this.resourceDefinition(entry);
    return this.ws.getMultilingualValueImmediate(resourceDefinition, 'DueDateLabel');
  }

  // MonetaryValue + CurrencyId

  public showMonetaryValue(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && (!account.CurrencyId || account.CurrencyId !== this.ws.settings.FunctionalCurrencyId);
  }

  public getAccountCurrencyId(entry: Entry): string {
    // returns the currency Id if any
    if (!entry) {
      return null;
    }

    const account = this.ws.get('Account', entry.AccountId) as AccountForSave;
    const resource = (!!account ? this.ws.get('Resource', account.ResourceId) : null) as Resource;

    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;
    const accountCurrencyId = !!account ? account.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId;
  }

  public getCurrencyId(entry: Entry): string {
    // returns the currency Id if any
    if (!entry) {
      return null;
    }

    const accountCurrencyId = this.getAccountCurrencyId(entry);
    const entryCurrencyId = entry.CurrencyId;

    return accountCurrencyId || entryCurrencyId;
  }

  public MonetaryValue_decimals(currencyId: string): number {
    const currency = this.ws.get('Currency', currencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  // Entry Classification

  public showEntryClassification(entry: Entry) {

  }
}
