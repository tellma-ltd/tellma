// tslint:disable:member-ordering
import { Component, Input, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService, TenantWorkspace, MasterDetailsStore } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Params } from '@angular/router';
import { DocumentForSave, Document, formatSerial, DocumentClearance, metadata_Document, DocumentState } from '~/app/data/entities/document';
import {
  DocumentDefinitionForClient, ResourceDefinitionForClient,
  LineDefinitionColumnForClient, LineDefinitionEntryForClient
} from '~/app/data/dto/definitions-for-client';
import { LineForSave, Line, LineState, LineFlags } from '~/app/data/entities/line';
import { Entry, EntryForSave } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import {
  addToWorkspace, getDataURL, downloadBlob,
  fileSizeDisplay, mergeEntitiesInWorkspace,
  toLocalDateISOString, FriendlyError
} from '~/app/data/util';
import { tap, catchError, finalize, takeUntil, skip } from 'rxjs/operators';
import { NgbModal, Placement } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError, Observable, Subscription } from 'rxjs';
import { AccountForSave } from '~/app/data/entities/account';
import { Resource } from '~/app/data/entities/resource';
import { Currency } from '~/app/data/entities/currency';
import { metadata_Agent } from '~/app/data/entities/agent';
import { AccountType } from '~/app/data/entities/account-type';
import { Attachment } from '~/app/data/entities/attachment';
import { Unit } from '~/app/data/entities/unit';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';
import { RequiredSignature } from '~/app/data/entities/required-signature';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ActionArguments } from '~/app/data/action-arguments';
import { EntitiesResponse } from '~/app/data/dto/get-response';
import { getChoices, ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { DocumentStateChange } from '~/app/data/entities/document-state-change';
import { formatDate } from '@angular/common';

type DocumentDetailsView = 'Managerial' | 'Accounting';
interface LineEntryPair {
  entry: EntryForSave;
  line: LineForSave;
  subscription?: Subscription; // cancels API calls specific to this line
  direction?: 1 | -1; // tracks whether the API call result will be debit or credit
  PH?: boolean;
}

interface DocumentDetailsState {
  tab: string;
  view: DocumentDetailsView;
}

interface ColumnTemplates {
  [index: string]: {
    headerTemplate?: TemplateRef<any>,
    rowTemplate: TemplateRef<any>,
    weight?: number,
    argument?: number
  };
}

/**
 * Hashes one dimension of an aggregate result for the pivot table
 */
interface HashTable {
  values?: { [value: string]: HashTable };
  undefined?: HashTable;

  lineIds?: number[];
  signatureIds?: number[];
}

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

interface DocumentStateChangeEvent extends DocumentEventBase {
  type: 'state';
  fromState: DocumentState;
  toState: DocumentState;
}

type DocumentEvent = DocumentReassignmentEvent | DocumentCreationEvent | DocumentStateChangeEvent;

@Component({
  selector: 't-documents-details',
  templateUrl: './documents-details.component.html',
  styles: []
})
export class DocumentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;
  private _maxAttachmentSize = 20 * 1024 * 1024;
  private _pristineDocJson: string;
  private localState = new MasterDetailsStore();  // Used in popup mode

  // These are bound from UI
  public assigneeId: number;
  public comment: string;
  public picSize = 36;

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

  @ViewChild('confirmModal', { static: true })
  confirmModal: TemplateRef<any>;

  @ViewChild('negativeSignatureModal', { static: true })
  negativeSignatureModal: TemplateRef<any>;

  public confirmationMessage: string;
  public signatureForNegativeModal: RequiredSignature;
  public reasonChoicesForNegativeModal: SelectorChoice[];
  public reasonDetails: string;
  public reasonId: number;

  public expand = 'CreatedBy,ModifiedBy,Assignee,' +
    // Entry Account
    ['Currency', /* 'Resource/Currency', */ 'Resource/Units', 'Agent',
      'EntryType', 'AccountType', 'Center']
      .map(prop => `Lines/Entries/Account/${prop}`).join(',') + ',' +

    // Entry
    ['Currency', 'Resource/Currency', 'Resource/Units', 'Agent',
      'EntryType', 'NotedAgent', 'Center', 'Unit']
      .map(prop => `Lines/Entries/${prop}`).join(',') + ',' +

    // Attachments
    ['CreatedBy', 'ModifiedBy']
      .map(prop => `Attachments/${prop}`).join(',') + ',' +

    // Attachments
    ['ModifiedBy']
      .map(prop => `StatesHistory/${prop}`).join(',') + ',' +

    // Assignment history
    ['Assignee', 'CreatedBy']
      .map(prop => `AssignmentsHistory/${prop}`).join(',');

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
    super();
  }

  ngOnInit() {

    const handleFreshStateFromUrl = (params: ParamMap) => {

      if (this.isScreenMode) {
        // Definitoin Id, must be set before retrieving the state
        this.definitionId = params.get('definitionId') || '';
        const s = this.state.detailsState as DocumentDetailsState;

        // When set to true, it means the url is out of step with the state
        let triggerUrlStateChange = false;

        // Active tab
        const urlTab = params.get('tab');
        if (!!urlTab) {
          s.tab = urlTab;
        } else if (!!s.tab) { // Prevents infinite loop
          triggerUrlStateChange = true;
        }

        // The URL is out of step with the state => sync the two
        // This happens when we navigate to the screen again 2nd time
        if (triggerUrlStateChange && !!this.details) {
          // We must be careful here to avoid an infinite loop
          this.details.urlStateChange();
        }
      }
    };

    this._subscriptions.add(this.route.paramMap.pipe(skip(1)).subscribe(handleFreshStateFromUrl)); // future changes
    handleFreshStateFromUrl(this.route.snapshot.paramMap); // right now
  }

  /**
   * Encodes any custom screen state in the url params
   */
  public encodeCustomStateFunc: (params: Params) => void = (params: Params) => {
    const s = this.state.detailsState as DocumentDetailsState;
    if (!!s.tab) {
      params.tab = s.tab;
    }
  }

  public setActiveTab(newTab: string) {
    (this.state.detailsState as DocumentDetailsState).tab = newTab;
    this.details.urlStateChange();
  }

  public getActiveTab(model: Document): string {
    // Special tabs
    const s = this.state.detailsState as DocumentDetailsState;
    if (s.tab === '_Attachments' || (!this.isJV && s.tab === '_Entries')) {
      return s.tab;
    }

    // Make sure the selected tab is a visible one
    const visibleTabs = this.visibleTabs(model);
    if (visibleTabs.some(e => e === s.tab)) {
      return s.tab;
    } else {
      // Get the first visible tab
      return visibleTabs[0];
    }
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.isPopupMode) {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else {

      // screen mode on the other hand use the global state
      return this.globalState;
    }
  }

  private get globalState(): MasterDetailsStore {
    const key = 'documents/' + this.definitionId;
    if (!this.workspace.current.mdState[key]) {
      this.workspace.current.mdState[key] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[key];
  }

  get view(): string {
    return `documents/${this.definitionId}`;
  }

  // UI Binding

  public get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result: DocumentForSave = {
      // PostingDate: toLocalDateISOString(new Date()),
      Clearance: 0,
      Lines: [],
      Attachments: []
    };

    if (this.definitionId === 'manual-journal-vouchers') {

      // Posting Date
      result.PostingDate = toLocalDateISOString(new Date());

      // Is Common
      result.MemoIsCommon = true;
      result.DebitAgentIsCommon = false;
      result.CreditAgentIsCommon = false;
      result.NotedAgentIsCommon = false;
      result.InvestmentCenterIsCommon = false;
      result.Time1IsCommon = false;
      result.Time2IsCommon = false;
      result.QuantityIsCommon = false;
      result.UnitIsCommon = false;
      result.CurrencyIsCommon = false;
    } else {
      const def = this.definition;

      // Posting Date
      if (!def.HasWorkflow) {
        result.PostingDate = toLocalDateISOString(new Date());
      }

      // Is Common
      result.MemoIsCommon = !!def.MemoIsCommonVisibility;
      result.DebitAgentIsCommon = !!def.DebitAgentVisibility;
      result.CreditAgentIsCommon = !!def.CreditAgentVisibility;
      result.NotedAgentIsCommon = !!def.NotedAgentVisibility;
      result.InvestmentCenterIsCommon = !!def.InvestmentCenterVisibility && this.ws.settings.IsMultiCenter;
      result.Time1IsCommon = !!def.Time1Visibility;
      result.Time2IsCommon = !!def.Time2Visibility;
      result.QuantityIsCommon = !!def.QuantityVisibility;
      result.UnitIsCommon = !!def.UnitVisibility;
      result.CurrencyIsCommon = !!def.CurrencyVisibility;
    }

    return result;
  }

  clone: (item: Document) => Document = (item: Document) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Document;

      // Standard
      clone.Id = null;
      delete clone.EntityMetadata;
      delete clone.serverErrors;

      // Non duplicable
      delete clone.SerialNumber;
      clone.Attachments = [];

      // Non savable
      delete clone.State;
      delete clone.StateAt;
      delete clone.Comment;
      delete clone.AssigneeId;
      delete clone.AssignedAt;
      delete clone.AssignedById;
      delete clone.OpenedAt;
      delete clone.CreatedAt;
      delete clone.CreatedById;
      delete clone.ModifiedAt;
      delete clone.ModifiedById;
      clone.AssignmentsHistory = [];
      clone.StatesHistory = [];

      if (!!clone.Lines) {
        clone.Lines.forEach(line => {
          // Standard
          line.Id = null;
          delete line.EntityMetadata;
          delete line.serverErrors;

          // Non savable
          delete line.DocumentId;
          delete line.State;
          delete line.CreatedAt;
          delete line.CreatedById;
          delete line.ModifiedAt;
          delete line.ModifiedById;

          if (!!line.Entries) {
            line.Entries.forEach(entry => {
              // Standard
              entry.Id = null;
              delete entry.EntityMetadata;
              delete entry.serverErrors;

              // Non savable
              delete entry.LineId;
              delete entry.CreatedAt;
              delete entry.CreatedById;
              delete entry.ModifiedAt;
              delete entry.ModifiedById;
            });
          }
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public formatSerial(serial: number) {
    if (!serial) {
      return `(${this.translate.instant('New')})`;
    }
    const def = this.definition;
    return formatSerial(serial, def.Prefix, def.CodeWidth || 4);
  }

  public get serialPrefix(): string {
    return this.definition.Prefix;
  }

  public get codeWidth(): number {
    return this.definition.CodeWidth;
  }

  public get showSidebar(): boolean {
    return this.isScreenMode && this.route.snapshot.paramMap.get('id') !== 'new';
  }

  isInactive(model: Document) {
    if (!model) {
      return '';
    }

    if (model.State === 1) {
      return 'Error_UnpostDocumentBeforeEdit';
    }

    if (model.State === -1) {
      return 'Error_UncancelDocumentBeforeEdit';
    }

    return null;
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

  public get functional_decimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functional_format(): string {
    const decimals = this.functional_decimals;
    return `1.${decimals}-${decimals}`;
  }

  public get functionalPostfix(): string {
    return ' (' + this.functionalName + ')';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  private _sortChronologicallyDoc: Document;
  private _sortChronologicallyResult: { date: string, events: DocumentEvent[] }[] = [];

  public sortChronologically(doc: Document): { date: string, events: DocumentEvent[] }[] {
    if (!doc) {
      return null;
    }

    if (doc !== this._sortChronologicallyDoc) {
      this._sortChronologicallyDoc = doc;
      const assignmentsHistory: DocumentAssignment[] = doc.AssignmentsHistory || [];
      const statesHistory: DocumentStateChange[] = doc.StatesHistory || [];

      const mappedAssignmentsHistory: DocumentEvent[] = assignmentsHistory
        .map(e =>
          ({
            type: 'reassignment',
            time: e.CreatedAt,
            userId: e.CreatedById,
            assigneeId: e.AssigneeId,
            comment: e.Comment,
          }));

      const mappedStatesHistory: DocumentEvent[] = statesHistory
        .map(e =>
          ({
            type: 'state',
            userId: e.ModifiedById,
            time: e.ModifiedAt,
            fromState: e.FromState,
            toState: e.ToState
          }));

      const mappedHistory = mappedAssignmentsHistory.concat(mappedStatesHistory);
      if (!!doc.CreatedById) {
        mappedHistory.push({ type: 'creation', userId: doc.CreatedById, time: doc.CreatedAt });
      }

      const sortedHistory: DocumentEvent[] = mappedHistory.sort((a, b) => {
        return a.time < b.time ? 1 :
          a.time > b.time ? -1 : 0;
      });

      const result: { [date: string]: DocumentEvent[] } = {};
      for (const entry of sortedHistory) {
        const date = toLocalDateISOString(new Date(entry.time));
        if (!result[date]) {
          result[date] = [];
        }

        result[date].push(entry);
      }

      this._sortChronologicallyResult = Object.keys(result).map(date => ({ date, events: result[date] }));
    }

    return this._sortChronologicallyResult;
  }

  public reassignment(event: DocumentEvent): DocumentReassignmentEvent {
    return event as DocumentReassignmentEvent;
  }

  public showAssignDocument(doc: Document) {
    // return true;
    return !!doc && !!doc.AssigneeId; // === this.ws.userSettings.UserId;
  }

  public stateUpdateDisplay(event: DocumentStateChangeEvent) {
    if (event.toState === 1) {
      return 'PostedThisDocument';
    } else if (event.toState === -1) {
      return 'CanceledThisDocument';
    } else {
      if (event.fromState === 1) {
        return 'UnpostedThisDocument';
      } else {
        return 'UncanceledThisDocument';
      }
    }
  }

  public onAssign(doc: Document): void {
    if (!!this.assigneeId) {
      this.documentsApi.assign([doc.Id], {
        returnEntities: true,
        expand: this.expand,
        assigneeId: this.assigneeId,
        comment: this.comment
      }, { includeRequiredSignatures: true }).pipe(
        tap(res => {
          addToWorkspace(res, this.workspace);
          this.details.state.extras = res.Extras;
          this.handleFreshExtras(res.Extras);

          // Clear the selection
          this.assigneeId = null;
          this.comment = null;
        })
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

  public get attachmentsClass(): string {
    return this.workspace.ws.isRtl ? 'mr-md-auto' : 'ml-md-auto';
  }

  ////////////// Properties of the document

  public showClearance(_: DocumentForSave) {
    return !!this.definition.ClearanceVisibility;
  }

  public clearanceDisplay(clearance: DocumentClearance) {
    if (clearance === null || clearance === undefined) {
      return '';
    }

    const desc = metadata_Document(this.workspace, this.translate, this.definitionId).properties.Clearance as ChoicePropDescriptor;
    return desc.format(clearance);
  }

  public get clearanceChoices(): SelectorChoice[] {
    const desc = metadata_Document(this.workspace, this.translate, this.definitionId).properties.Clearance as ChoicePropDescriptor;
    return getChoices(desc);
  }

  // Document Memo

  public showDocumentMemo(_: DocumentForSave): boolean {
    return !!this.definition.MemoVisibility;
  }

  public showDocumentMemoIsCommon(_: DocumentForSave): boolean {
    return this.definition.MemoIsCommonVisibility;
  }

  public requireDocumentMemo(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this.definition.MemoVisibility === 'Required' || this._requireDocumentMemo;
  }

  public readonlyDocumentMemo(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentMemo;
  }

  public labelDocumentMemo(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'MemoLabel') || this.translate.instant('Memo');
  }

  // DebitAgent

  public showDocumentDebitAgent(_: DocumentForSave): boolean {
    return this.definition.DebitAgentVisibility;
  }

  public requireDocumentDebitAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDebitAgent;
  }

  public readonlyDocumentDebitAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDebitAgent;
  }

  public labelDocumentDebitAgent(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'DebitAgentLabel');
    if (!label) {
      const agentDefId = this.definition.DebitAgentDefinitionId;
      const agentDef = this.ws.definitions.Agents[agentDefId];
      if (!!agentDef) {
        label = this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Agent');
      }
    }

    return label;
  }

  public documentDebitAgentDefinitionIds(_: DocumentForSave): string[] {
    return [this.definition.DebitAgentDefinitionId];
  }

  // CreditAgent

  public showDocumentCreditAgent(_: DocumentForSave): boolean {
    return this.definition.CreditAgentVisibility;
  }

  public requireDocumentCreditAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireCreditAgent;
  }

  public readonlyDocumentCreditAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyCreditAgent;
  }

  public labelDocumentCreditAgent(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'CreditAgentLabel');
    if (!label) {
      const agentDefId = this.definition.CreditAgentDefinitionId;
      const agentDef = this.ws.definitions.Agents[agentDefId];
      if (!!agentDef) {
        label = this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Agent');
      }
    }

    return label;
  }

  public documentCreditAgentDefinitionIds(_: DocumentForSave): string[] {
    return [this.definition.CreditAgentDefinitionId];
  }

  // NotedAgent

  public showDocumentNotedAgent(_: DocumentForSave): boolean {
    return this.definition.NotedAgentVisibility;
  }

  public requireDocumentNotedAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireNotedAgent;
  }

  public readonlyDocumentNotedAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyNotedAgent;
  }

  public labelDocumentNotedAgent(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'NotedAgentLabel');
    if (!label) {
      const agentDefId = this.definition.NotedAgentDefinitionId;
      const agentDef = this.ws.definitions.Agents[agentDefId];
      if (!!agentDef) {
        label = this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Agent');
      }
    }

    return label;
  }

  public documentNotedAgentDefinitionIds(_: DocumentForSave): string[] {
    return [this.definition.NotedAgentDefinitionId];
  }

  // Investment Center

  public showDocumentInvestmentCenter(_: DocumentForSave) {
    return this.definition.InvestmentCenterVisibility && this.ws.settings.IsMultiCenter;
  }

  public requireDocumentInvestmentCenter(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireInvestmentCenter;
  }

  public readonlyDocumentInvestmentCenter(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyInvestmentCenter;
  }

  public labelDocumentInvestmentCenter(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'InvestmentCenterLabel') ||
      this.translate.instant('Document_InvestmentCenter');
  }

  public showDocumentTime1(_: DocumentForSave) {
    return this.definition.Time1Visibility;
  }

  public requireDocumentTime1(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentTime1;
  }

  public readonlyDocumentTime1(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentTime1;
  }

  public labelDocumentTime1(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'Time1Label') ||
      this.translate.instant('Document_Time1');
  }

  // Time2

  public showDocumentTime2(_: DocumentForSave) {
    return this.definition.Time2Visibility;
  }

  public requireDocumentTime2(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentTime2;
  }

  public readonlyDocumentTime2(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentTime2;
  }

  public labelDocumentTime2(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'Time2Label') ||
      this.translate.instant('Document_Time2');
  }

  // Quantity

  public showDocumentQuantity(_: DocumentForSave) {
    return this.definition.QuantityVisibility;
  }

  public requireDocumentQuantity(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentQuantity;
  }

  public readonlyDocumentQuantity(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentQuantity;
  }

  public labelDocumentQuantity(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'QuantityLabel') ||
      this.translate.instant('Document_Quantity');
  }

  // Unit

  public showDocumentUnit(_: DocumentForSave) {
    return this.definition.UnitVisibility;
  }

  public requireDocumentUnit(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentUnit;
  }

  public readonlyDocumentUnit(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentUnit;
  }

  public labelDocumentUnit(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'UnitLabel') ||
      this.translate.instant('Document_Unit');
  }

  // Currency

  public showDocumentCurrency(_: DocumentForSave) {
    return this.definition.CurrencyVisibility;
  }

  public requireDocumentCurrency(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentCurrency;
  }

  public readonlyDocumentCurrency(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentCurrency;
  }

  public labelDocumentCurrency(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'CurrencyLabel') ||
      this.translate.instant('Document_Currency');
  }

  // Time1
  private _computeDocumentSettingsDoc: Document;
  private _computeDocumentSettingsDef: DocumentDefinitionForClient;
  private _requireDocumentMemo: boolean;
  private _readonlyDocumentMemo: boolean;
  private _requireDebitAgent: boolean;
  private _readonlyDebitAgent: boolean;
  private _requireCreditAgent: boolean;
  private _readonlyCreditAgent: boolean;
  private _requireNotedAgent: boolean;
  private _readonlyNotedAgent: boolean;
  private _requireInvestmentCenter: boolean;
  private _readonlyInvestmentCenter: boolean;
  private _requireDocumentTime1: boolean;
  private _readonlyDocumentTime1: boolean;
  private _requireDocumentTime2: boolean;
  private _readonlyDocumentTime2: boolean;
  private _requireDocumentQuantity: boolean;
  private _readonlyDocumentQuantity: boolean;
  private _requireDocumentUnit: boolean;
  private _readonlyDocumentUnit: boolean;
  private _requireDocumentCurrency: boolean;
  private _readonlyDocumentCurrency: boolean;
  private computeDocumentSettings(doc: Document): void {
    if (!doc || !doc.Lines) {
      this._requireDocumentMemo = false;
      this._readonlyDocumentMemo = false;
      this._requireDebitAgent = false;
      this._readonlyDebitAgent = false;
      this._requireCreditAgent = false;
      this._readonlyCreditAgent = false;
      this._requireNotedAgent = false;
      this._readonlyNotedAgent = false;
      this._requireInvestmentCenter = false;
      this._readonlyInvestmentCenter = false;
      this._requireDocumentTime1 = false;
      this._readonlyDocumentTime1 = false;
      this._requireDocumentTime2 = false;
      this._readonlyDocumentTime2 = false;
      this._requireDocumentQuantity = false;
      this._readonlyDocumentQuantity = false;
      this._requireDocumentUnit = false;
      this._readonlyDocumentUnit = false;
      this._requireDocumentCurrency = false;
      this._readonlyDocumentCurrency = false;

      return;
    }

    const def = this.definition;
    if (this._computeDocumentSettingsDoc !== doc ||
      this._computeDocumentSettingsDef !== def) {
      this._computeDocumentSettingsDoc = doc;
      this._computeDocumentSettingsDef = def;

      this._requireDocumentMemo = def.MemoRequiredState === 0;
      this._readonlyDocumentMemo = def.MemoReadOnlyState === 0;
      this._requireDebitAgent = def.DebitAgentRequiredState === 0;
      this._readonlyDebitAgent = def.DebitAgentReadOnlyState === 0;
      this._requireCreditAgent = def.CreditAgentRequiredState === 0;
      this._readonlyCreditAgent = def.CreditAgentReadOnlyState === 0;
      this._requireNotedAgent = def.NotedAgentRequiredState === 0;
      this._readonlyNotedAgent = def.NotedAgentReadOnlyState === 0;
      this._requireInvestmentCenter = def.InvestmentCenterRequiredState === 0;
      this._readonlyInvestmentCenter = def.InvestmentCenterReadOnlyState === 0;
      this._requireDocumentTime1 = def.Time1RequiredState === 0;
      this._readonlyDocumentTime1 = def.Time1ReadOnlyState === 0;
      this._requireDocumentTime2 = def.Time2RequiredState === 0;
      this._readonlyDocumentTime2 = def.Time2ReadOnlyState === 0;
      this._requireDocumentQuantity = def.QuantityRequiredState === 0;
      this._readonlyDocumentQuantity = def.QuantityReadOnlyState === 0;
      this._requireDocumentUnit = def.UnitRequiredState === 0;
      this._readonlyDocumentUnit = def.UnitReadOnlyState === 0;
      this._requireDocumentCurrency = def.CurrencyRequiredState === 0;
      this._readonlyDocumentCurrency = def.CurrencyReadOnlyState === 0;

      for (const lineDefId of def.LineDefinitions.map(e => e.LineDefinitionId)) {
        const lineDef = this.lineDefinition(lineDefId);
        for (const colDef of lineDef.Columns.filter(c => c.InheritsFromHeader)) {

          switch (colDef.ColumnName) {
            case 'Memo':
              if (!this._requireDocumentMemo &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentMemo = true;
              }
              if (!this._readonlyDocumentMemo &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentMemo = true;
              }
              break;
            case 'AgentId':
              if (!this._requireDebitAgent && lineDef.Entries[colDef.EntryIndex].Direction === 1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDebitAgent = true;
              }
              if (!this._requireCreditAgent && lineDef.Entries[colDef.EntryIndex].Direction === -1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireCreditAgent = true;
              }

              if (!this._readonlyDebitAgent && lineDef.Entries[colDef.EntryIndex].Direction === 1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDebitAgent = true;
              }
              if (!this._readonlyCreditAgent && lineDef.Entries[colDef.EntryIndex].Direction === -1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyCreditAgent = true;
              }
              break;

            case 'NotedAgentId':
              if (!this._requireNotedAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireNotedAgent = true;
              }
              if (!this._readonlyNotedAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyNotedAgent = true;
              }
              break;

            case 'CenterId':
              if (!this._requireInvestmentCenter &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireInvestmentCenter = true;
              }
              if (!this._readonlyInvestmentCenter &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyInvestmentCenter = true;
              }
              break;

            case 'Time1':
              if (!this._requireDocumentTime1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentTime1 = true;
              }
              if (!this._readonlyDocumentTime1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentTime1 = true;
              }
              break;

            case 'Time2':
              if (!this._requireDocumentTime2 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentTime2 = true;
              }
              if (!this._readonlyDocumentTime2 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentTime2 = true;
              }
              break;

            case 'Quantity':
              if (!this._requireDocumentQuantity &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentQuantity = true;
              }
              if (!this._readonlyDocumentQuantity &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentQuantity = true;
              }
              break;

            case 'UnitId':
              if (!this._requireDocumentUnit &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentUnit = true;
              }
              if (!this._readonlyDocumentUnit &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentUnit = true;
              }
              break;

            case 'CurrencyId':
              if (!this._requireDocumentCurrency &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentCurrency = true;
              }
              if (!this._readonlyDocumentCurrency &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentCurrency = true;
              }
              break;
          }
        }
      }
    }
  }

  /////// Properties of the lines

  public account(entry: Entry): AccountForSave {
    return this.ws.get('Account', entry.AccountId) as AccountForSave;
  }

  public resource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const resourceId = accountResourceId || entry.ResourceId;
    return this.ws.get('Resource', resourceId) as Resource;
  }

  private resourceDefinition(entry: Entry): ResourceDefinitionForClient {
    const resource = this.resource(entry);
    const defId = !!resource ? resource.DefinitionId : null;
    const resourceDefinition = !!defId ? this.ws.definitions.Resources[defId] : null;
    return resourceDefinition;
  }

  // AgentId

  public showAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account && !!account.AgentDefinitionId;
  }

  public readonlyAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.AgentId;
  }

  public readonlyValueAgentId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.AgentId : null;
  }

  public labelAgent(entry: Entry): string {
    const account = this.account(entry);
   // const agentDefinitionId = !!account ? account.AgentDefinitionId : null;

    return ''; // TODO: metadata_Agent(this.workspace, this.translate, agentDefinitionId).titleSingular();
  }

  // ResourceId

  public showResource(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account && account.HasResource;
  }

  public readonlyResource(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ResourceId;
  }

  public readonlyValueResourceId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.ResourceId : null;
  }

  public filterResource(entry: Entry): string {
    // For manual JV
    const account = this.account(entry);
    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;

    if (!!accountType.IsResourceClassification) {
      return `AccountType/Node descof ${account.AccountTypeId}`;
    } else {
      return null;
    }
  }

  public labelResource(_: Entry): string {
    return this.translate.instant('Resource');
  }

  // Quantity + Unit

  public showQuantityAndUnit(entry: Entry): boolean {
    const account = this.account(entry);
    const resource = this.resource(entry);
    return !!account && !!resource && !!resource.Units && resource.Units.length > 0;
  }

  public readonlyUnit(entry: Entry): boolean {
    const resource = this.resource(entry);
    return !!resource && !!resource.Units && resource.Units.length === 1;
  }

  public readonlyValueUnitId(entry: Entry): number {
    const resource = this.resource(entry);
    return !!resource && !!resource.Units && !!resource.Units[0] ? resource.Units[0].UnitId : null;
  }

  public filterUnitId(entry: Entry): string {
    const resource = this.resource(entry);
    if (!!resource && !!resource.Units) {
      return resource.Units.map(e => `Id eq ${e.UnitId}`).join(' or ');
    }

    return null;
  }

  private unit(entry: Entry): Unit {
    const unitId = this.readonlyUnit(entry) ? this.readonlyValueUnitId(entry) : entry.UnitId;
    return this.ws.get('Unit', unitId) as Unit;
  }

  // DueDate

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

  // Currency

  private getAccountResourceCurrencyId(entry: Entry): string {
    // returns the currency Id (if any) that will eventually be copied to the Entry in the bll
    if (!entry) {
      return null;
    }

    const account = this.account(entry);
    const resource = this.ws.get('Resource', entry.ResourceId) as Resource;

    const accountCurrencyId = !!account ? account.CurrencyId : null;
    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId;
  }

  public showCurrency(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !this.getAccountResourceCurrencyId(entry);
  }

  public readonlyValueCurrencyId(entry: Entry): string {
    // returns the currency Id if any
    if (!entry) {
      return null;
    }

    const accountResourceCurrencyId = this.getAccountResourceCurrencyId(entry);
    const entryCurrencyId = entry.CurrencyId;

    return accountResourceCurrencyId || entryCurrencyId;
  }

  public get functionalId(): string {
    return this.ws.settings.FunctionalCurrencyId;
  }

  // MonetaryValue

  public showMonetaryValue(entry: Entry): boolean {
    const account = this.account(entry);
    const currencyId = this.readonlyValueCurrencyId(entry);
    return !!account && !!currencyId && currencyId !== this.functionalId;
  }

  public MonetaryValue_decimals(entry: Entry): number {
    const currencyId = this.readonlyValueCurrencyId(entry);
    const currency = this.ws.get('Currency', currencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  public MonetaryValue_format(entry: Entry): string {
    const decimals = this.MonetaryValue_decimals(entry);
    return `1.${decimals}-${decimals}`;
  }

  // Entry Classification

  public showEntryType(entry: Entry): boolean {
    const account = this.account(entry);
    if (!account || !account.AccountTypeId) {
      return false;
    }

    // Show entry type when the account's type has an entry type parent Id
    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    return !!accountType.EntryTypeParentId;
  }

  public readonlyEntryType(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.EntryTypeId;
  }

  public readonlyValueEntryTypeId(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.EntryTypeId : null;
  }

  public filterEntryType(entry: Entry): string {
    const account = this.account(entry);
    if (!account || !account.AccountTypeId) {
      return null;
    }

    const accountType = this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    return `IsAssignable eq true and IsActive eq true and Node descof ${accountType.EntryTypeParentId}`;
  }

  // External Reference

  public showExternalReference(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasExternalReference : false;
  }

  // Additional Reference

  public showAdditionalReference(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasAdditionalReference : false;
  }

  // Noted Agent Id

  public showNotedAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasNotedAgentId : false;
  }

  // Noted Agent Name

  public showNotedAgentName(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasNotedAgentName : false;
  }

  // Noted Amount
  public showNotedAmount(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasNotedAmount : false;
  }

  // Noted Date
  public showNotedDate(entry: Entry): boolean {
    const account = this.account(entry);
    return false; // TODO: !!account ? account.HasNotedDate : false;
  }

  public onFileSelected(input: any, model: DocumentForSave) {
    const files = input.files as FileList;
    if (!files) {
      return;
    }

    // Convert the FileList to an array
    const filesArray: File[] = [];
    // tslint:disable-next-line:prefer-for-of
    for (let i = 0; i < files.length; i++) {
      filesArray.push(files[i]);
    }

    // Clear the input field
    input.value = '';

    // Calculate total size of files
    const totalSize = filesArray
      .map(e => e.size || 0)
      .reduce((total, v) => total + v, 0);

    // Make sure total size of selected files doesn't exceed maximum size
    if (totalSize > this._maxAttachmentSize) {
      this.details.displayModalError(this.translate.instant('Error_FileSizeExceedsMaximumSizeOf0',
        { size: fileSizeDisplay(this._maxAttachmentSize) }));

      return;
    }

    // Make sure pending attachments don't exceed maximum size
    model.Attachments = model.Attachments || [];
    const sumOfAttachmentSizesPendingSave = model.Attachments
      .map(a => !!a.file ? a.file.size : 0)
      .reduce((total, v) => total + v, 0);

    if (sumOfAttachmentSizesPendingSave + totalSize > this._maxAttachmentSize) {
      this.details.displayModalError(this.translate.instant('Error_PendingFilesExceedMaximumSizeOf0',
        { size: fileSizeDisplay(this._maxAttachmentSize) }));
      return;
    }

    for (const file of filesArray) {

      getDataURL(file).pipe(
        takeUntil(this.notifyDestruct$),
        tap(dataUrl => {

          // Get the base64 value from the data URL
          const commaIndex = dataUrl.indexOf(',');
          const fileBytes = dataUrl.substr(commaIndex + 1);
          const fileNamePieces = file.name.split('.');
          const extension = fileNamePieces.length > 1 ? fileNamePieces.pop() : null;
          const fileName = fileNamePieces.join('.') || '???';
          model.Attachments.push({
            Id: 0,
            File: fileBytes,
            FileName: fileName,
            FileExtension: extension,
            file,

            toJSON() {
              return {
                Id: this.Id,
                File: this.File,
                FileName: this.FileName,
                FileExtension: this.FileExtension
              };
            }
          });
        }),
        catchError(err => {
          console.error(err);
          return throwError(err);
        })
      ).subscribe();
    }
  }

  public onDeleteAttachment(model: DocumentForSave, index: number) {
    model.Attachments.splice(index, 1);
  }

  public onDownloadAttachment(model: DocumentForSave, index: number) {
    const docId = model.Id;
    const att = model.Attachments[index];

    if (!!att.Id) {
      att.downloading = true; // show a little spinner
      this.documentsApi.getAttachment(docId, att.Id).pipe(
        tap(blob => {
          delete att.downloading;
          downloadBlob(blob, this.fileName(att));
        }),
        catchError(friendlyError => {
          delete att.downloading;
          this.details.handleActionError(friendlyError);
          return of(null);
        }),
        finalize(() => {
          delete att.downloading;
        })
      ).subscribe();

    } else if (!!att.file) {
      downloadBlob(att.file, this.fileName(att));
    }
  }

  private fileName(att: Attachment) {
    return !!att.FileName && !!att.FileExtension ? `${att.FileName}.${att.FileExtension}` :
      (att.FileName || (!!att.file ? att.file.name : 'Attachment'));
  }

  public size(att: Attachment): string {
    return fileSizeDisplay(att.Size || (!!att.file ? att.file.size : null));
  }

  public colorFromExtension(extension: string): string {
    const icon = this.iconFromExtension(extension);
    switch (icon) {
      case 'file-pdf': return '#CA342B';
      case 'file-word': return '#345692';
      case 'file-excel': return '#316F3E';
      case 'file-powerpoint': return '#BD4D2D';
      case 'file-archive': return '#E5BE36';
      case 'file-image': return '#3E7A7E';
      case 'file-video': return '#A12F5E'; // CC5747
      case 'file-audio': return '#BA7D27';

      case 'file-alt': // text files
      case 'file': return '#6c757d';
    }

    return null;
  }

  public iconFromExtension(extension: string): string {
    if (!extension) {
      return 'file';
    } else {
      extension = extension.toLowerCase();
      switch (extension) {
        case 'pdf':
          return 'file-pdf';

        case 'doc':
        case 'docx':
          return 'file-word';

        case 'xls':
        case 'xlsx':
          return 'file-excel';

        case 'ppt':
        case 'pptx':
          return 'file-powerpoint';

        case 'txt':
        case 'rtf':
          return 'file-alt';

        case 'zip':
        case 'rar':
        case '7z':
        case 'tar':
          return 'file-archive';

        case 'jpg':
        case 'jpeg':
        case 'jpe':
        case 'jif':
        case 'jfif':
        case 'jfi':
        case 'png':
        case 'ico':
        case 'gif':
        case 'webp':
        case 'tiff':
        case 'tif':
        case 'psd':
        case 'raw':
        case 'arw':
        case 'cr2':
        case 'nrw':
        case 'k25':
        case 'bmp':
        case 'dib':
        case 'heif':
        case 'heic':
        case 'ind':
        case 'indd':
        case 'indt':
        case 'jp2':
        case 'j2k':
        case 'jpf':
        case 'jpx':
        case 'jpm':
        case 'mj2':
        case 'svg':
        case 'svgz':
        case 'ai':
        case 'eps':
          return 'file-image';

        case 'mpg':
        case 'mp2':
        case 'mpeg':
        case 'mpe':
        case 'mpv':
        case 'ogg':
        case 'mp4':
        case 'm4p':
        case 'm4v':
        case 'avi':
        case 'wmv':
        case 'mov':
        case 'qt':
        case 'flv':
        case 'swf':
          return 'file-video';

        case 'mp3':
        case 'aac':
        case 'wma':
        case 'flac':
        case 'alac':
        case 'wav':
        case 'aiff':
          return 'file-audio';

        default:
          return 'file';
      }
    }
  }

  public registerPristineFunc = (pristineDoc: DocumentForSave) => {
    const tracked = this.removeUntrackedProperties(pristineDoc);
    this._pristineDocJson = JSON.stringify(tracked);
  }

  public isDirtyFunc = (model: DocumentForSave) => {
    return this._pristineDocJson !== JSON.stringify(this.removeUntrackedProperties(model));
  }

  private removeUntrackedProperties(doc: Document): Document {
    if (!doc) {
      return doc;
    }

    // These properties and collections are not edited directly
    // and therefore need not be compared for dirty checking
    const copy = { ...doc } as Document;
    delete copy.AssignmentsHistory;
    if (!!doc.Attachments) {
      copy.Attachments = doc.Attachments.map(att => {
        const attCopy = { ...att } as Attachment;
        delete attCopy.file;
        delete attCopy.File;
        delete attCopy.downloading;
        return attCopy;
      });
    }

    return copy;
  }

  public extraParams = { includeRequiredSignatures: true };

  public handleFreshExtras(extras: { [key: string]: any }) {
    if (!!extras) {
      const relatedEntities = extras.RequiredSignaturesRelatedEntities as ({ [key: string]: EntityWithKey[] });
      if (!!relatedEntities) {
        mergeEntitiesInWorkspace(relatedEntities, this.workspace);
      }
    }
  }

  private _requiredSignaturesForLineDefModel: Document;
  private _requiredSignaturesForLineDefLineDefId: string;
  private _requiredSignaturesForLineDefLineIds: number[];

  public requiredSignaturesForLineDef(
    model: Document, lineDefId: string, extras: { [key: string]: any }): RequiredSignature[] {
    if (this._requiredSignaturesForLineDefModel !== model ||
      this._requiredSignaturesForLineDefLineDefId !== lineDefId) {
      this._requiredSignaturesForLineDefModel = model;
      this._requiredSignaturesForLineDefLineDefId = lineDefId;
      this._requiredSignaturesForLineDefLineIds = model.Lines
        .filter(l => !!l.Id && l.DefinitionId === lineDefId)
        .map(l => l.Id as number);
    }

    return this.requiredSignaturesSummaryInner(this._requiredSignaturesForLineDefLineIds, extras);
  }

  private _requiredSignaturesDetailed: RequiredSignature[];
  private _requiredSignaturesLineIds: number[];
  private _requiredSignaturesSummary: RequiredSignature[];
  private _requiredSignaturesLineIdsHash: HashTable;
  private _requiredSignatureProps = [
    'ToState', 'RuleType', 'RoleId', 'AgentId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
    'LastUnsignedState', 'LastNegativeState', 'CanSign', 'ProxyRoleId', 'CanSignOnBehalf',
    'ReasonId', 'ReasonDetails'];

  public requiredSignaturesSummaryInner(
    lineIds: number[],
    extras: { [key: string]: any }
  ): RequiredSignature[] {

    if (!lineIds || !extras || !extras.RequiredSignatures) {
      return [];
    }

    // This function implements a "group by" algorithm of required signatures
    const requiredSignaturesDetailed = extras.RequiredSignatures as RequiredSignature[];
    if (requiredSignaturesDetailed !== this._requiredSignaturesDetailed || lineIds !== this._requiredSignaturesLineIds) {
      this._requiredSignaturesDetailed = requiredSignaturesDetailed;
      this._requiredSignaturesLineIds = lineIds;

      // Put all included line Ids in a hash table for quick lookup
      const includedLineIds: { [id: number]: true } = {};
      for (const lineId of lineIds) {
        includedLineIds[lineId] = true;
      }

      const lineIdsHash: HashTable = {};
      const result: RequiredSignature[] = [];

      // Filter away signatures that don't belong to the provided line Ids,
      // And also filter away pending signatures of already negative lines
      const filteredSignatures = requiredSignaturesDetailed
        .filter(e => (!e.LastNegativeState || !!e.SignedById) && includedLineIds[e.LineId]);

      for (const signature of filteredSignatures) {
        let currentHash = lineIdsHash;
        let newGroup = false;
        for (const prop of this._requiredSignatureProps) {
          const value = signature[prop];

          if (value === null || value === undefined) {
            // for null or undefined values, use the "undefined" property
            if (!currentHash.undefined) {
              currentHash.undefined = {};
              newGroup = true;
            }

            currentHash = currentHash.undefined;
          } else {
            // for defined values, use the "values" property
            if (!currentHash.values) {
              currentHash.values = {};
            }

            if (currentHash.values[value] === undefined) {
              currentHash.values[value] = {};
              newGroup = true;
            }

            currentHash = currentHash.values[value];
          }
        }

        if (!currentHash.lineIds) {
          currentHash.lineIds = [];
        }

        if (!currentHash.signatureIds) {
          currentHash.signatureIds = [];
        }

        currentHash.lineIds.push(signature.LineId);
        if (!!signature.LineSignatureId) {
          currentHash.signatureIds.push(signature.LineSignatureId);
        }

        if (newGroup) {
          // The signature clone will represent this group
          const clone = { ...signature } as RequiredSignature;
          delete clone.LineId;
          result.push(clone);
        }
      }

      this._requiredSignaturesLineIdsHash = lineIdsHash;
      this._requiredSignaturesSummary = result.sort((a, b) => {
        // Order by to state
        let diff = Math.abs(a.ToState) - Math.abs(b.ToState);

        // Then by rule type
        if (diff === 0) {
          const aRT = a.RuleType || '0';
          const bRT = b.RuleType || '0';
          diff = aRT > bRT ? 1 : aRT < bRT ? -1 : 0;
        }

        // Then by role ID
        if (diff === 0) {
          diff = (a.RoleId || 0) - (b.RoleId || 0);
        }

        // Then by user ID
        if (diff === 0) {
          diff = (a.UserId || 0) - (b.UserId || 0);
        }

        // Return result
        return diff;
      });
    }

    return this._requiredSignaturesSummary;
  }

  public lineIds(requiredSignature: RequiredSignature): number[] {
    if (!requiredSignature) {
      return [];
    }

    return this.getHash(requiredSignature).lineIds;
  }

  public signatureIds(requiredSignature: RequiredSignature): number[] {
    if (!requiredSignature) {
      return [];
    }

    return this.getHash(requiredSignature).signatureIds;
  }

  private getHash(requiredSignature: RequiredSignature): HashTable {
    if (!requiredSignature) {
      return null;
    }

    let currentHash: HashTable = this._requiredSignaturesLineIdsHash;
    for (const prop of this._requiredSignatureProps) {
      const value = requiredSignature[prop];
      if (value === null || value === undefined) {
        currentHash = currentHash.undefined;
      } else {
        currentHash = currentHash.values[value];
      }
    }

    return currentHash;
  }

  public onSignYes(signature: RequiredSignature): void {
    this.onSign(signature, true);
  }

  public onSignNo(lineDefId: string, signature: RequiredSignature): void {
    // Remember the required signature that the user is signing
    this.signatureForNegativeModal = signature;

    // Remember the reason choices for the current line Definition and state
    const lineDef = this.lineDefinition(lineDefId);
    const reasons = !!lineDef ? lineDef.StateReasons || [] : [];
    this.reasonChoicesForNegativeModal = reasons
      .filter(e => Math.abs(e.State) === Math.abs(signature.ToState))
      .map(e => ({ name: () => this.ws.getMultilingualValueImmediate(e, 'Name'), value: e.Id }));

    // Launch the modal that asks the user for the reason behind the negative signature
    const modalRef = this.modalService.open(this.negativeSignatureModal);
    modalRef.result.then(
      (confirmed: boolean) => {
        if (confirmed) {
          this.onSign(signature, false);
        }
      },
      _ => { }
    );
  }

  private onSign(signature: RequiredSignature, yes: boolean): void {
    const lineIds = this.lineIds(signature);
    this.documentsApi.sign(lineIds, {
      returnEntities: true,
      expand: this.expand,
      onBehalfOfUserId: signature.OnBehalfOfUserId,
      toState: yes ? Math.abs(signature.ToState) : -Math.abs(signature.ToState),
      roleId: signature.RoleId,
      ruleType: signature.RuleType,
      reasonDetails: yes ? null : this.reasonDetails,
      reasonId: yes ? null : this.reasonId,
      signedAt: null,
    }, { includeRequiredSignatures: true }).pipe(
      tap(res => {
        addToWorkspace(res, this.workspace);
        this.details.state.extras = res.Extras;
        this.handleFreshExtras(res.Extras);
      }),
    ).subscribe({ error: this.details.handleActionError });
  }

  public onUnsign(signature: RequiredSignature) {
    this.confirmationMessage = this.translate.instant('AreYouSureYouWantToDeleteYourSignature');
    const modalRef = this.modalService.open(this.confirmModal);
    modalRef.result.then(
      (confirmed: boolean) => {
        if (confirmed) {
          this.documentsApi.unsign(this.signatureIds(signature), {
            returnEntities: true,
            expand: this.expand,
            select: undefined
          }, { includeRequiredSignatures: true }).pipe(
            tap(res => {
              addToWorkspace(res, this.workspace);
              this.details.state.extras = res.Extras;
              this.handleFreshExtras(res.Extras);
            }),
          ).subscribe({ error: this.details.handleActionError });
        }
      },
      _ => { }
    );
  }

  public canUnsign(signature: RequiredSignature) {
    return !!signature.SignedById && signature.SignedById === this.ws.userSettings.UserId;
  }

  public disableUnsign(_: RequiredSignature, model: Document) {
    return !!model ? !!model.State : true;
  }

  public unsignTooltip(_: RequiredSignature, model: Document) {
    if (!model) {
      return null;
    } else if (model.State === 1) {
      return this.translate.instant('Error_UnpostDocumentBeforeEdit');
    } else if (model.State === -1) {
      return this.translate.instant('Error_UncancelDocumentBeforeEdit');
    } else {
      return null;
    }
  }

  public positiveActionDisplay(toState: number): string {
    // Used for button
    return this.actionDisplay(Math.abs(toState));
  }

  public positiveActionIcon(toState: number): string {
    // Used for button
    return this.actionIcon(Math.abs(toState));
  }

  public negativeActionDisplay(toState: number): string {
    // Used for button
    return this.actionDisplay(-Math.abs(toState));
  }

  public negativeActionIcon(toState: number): string {
    // Used for button
    return this.actionIcon(-Math.abs(toState));
  }

  private actionIcon(toState: number): string {
    // Used for stamp
    switch (toState) {
      case 1: return this.workspace.ws.isRtl ? 'arrow-left' : 'arrow-right';
      case 2: return 'thumbs-up';
      case 3: return 'check';
      case 4: return 'check';

      case -1: return 'times';
      case -2: return 'thumbs-down';
      case -3: return 'times';
      case -4: return 'times';
      default: return '';
    }
  }

  public get assignIcon(): string {
    return this.workspace.ws.isRtl ? 'angle-left' : 'angle-right';
  }

  public actionDisplay(toState: number): string {
    if (toState >= 0) {
      return this.translate.instant('Line_State_' + toState);
    } else {
      return this.translate.instant('Line_State_minus_' + (-toState));
    }
  }

  public requiredSignatureDisplay(signature: RequiredSignature) {
    // Used for the footer of the stamp in all rule types except 'Public'
    return this.translate.instant('RequiredSignature_' + Math.abs(signature.ToState));
  }

  public requiredSignatoryDisplay(signature: RequiredSignature) {
    // Used for the footer of the stamp for rule type 'Public'
    return this.translate.instant('RequiredSigner_' + Math.abs(signature.ToState));
  }

  private isTooEarlyForThisSignature(signature: RequiredSignature): boolean {
    return signature.LastUnsignedState < signature.ToState;
  }

  private areNegativeLines(signature: RequiredSignature): boolean {
    return !!signature.LastNegativeState;
  }

  public disableSign(signature: RequiredSignature, lineDefId: string, model: Document): boolean {
    if (!model) {
      return false;
    }

    return model.State === -1 ||
      model.State === 1 ||
      this.isTooEarlyForThisSignature(signature) ||
      this.areNegativeLines(signature);
  }

  public signTooltip(signature: RequiredSignature, lineDefId: string, model: Document) {
    if (!model) {
      return null;
    } else if (model.State === 1) {
      return this.translate.instant('Error_UnpostDocumentBeforeEdit');
    } else if (model.State === -1) {
      return this.translate.instant('Error_UncancelDocumentBeforeEdit');
    } else if (this.areNegativeLines(signature)) {
      // These lines are already in a negative state, it's pointless to sign them again
      const stateDisplay = this.actionDisplay(signature.LastNegativeState);
      const lines = this.lineIds(signature) || [];
      return this.translate.instant(lines.length === 1 ? 'LineAlreadyInState0' : 'LinesAlreadyInState0', { 0: stateDisplay });

    } else if (this.isTooEarlyForThisSignature(signature)) {
      // There is a preceding positive state that hasn't been reached yet, so not yet the time to sign for this state
      const stateDisplay = this.actionDisplay(signature.LastUnsignedState);
      const lines = this.lineIds(signature) || [];
      return this.translate.instant(lines.length === 1 ? 'LineIsNotYetInState0' : 'LinesAreNotYetInState0', { 0: stateDisplay });

    } else {
      return null;
    }
  }

  /////////// Lines and Tabs

  public onInsertSmartLine(line: LineForSave, model: Document): void {
    model.Lines.push(line);
    this._computeEntriesModel = null; // Force refresh the entries view
  }

  public onDeleteSmartLine(line: LineForSave, model: Document): void {
    const index = model.Lines.indexOf(line);
    if (index > -1) {
      model.Lines.splice(index, 1);
    }
    this._computeEntriesModel = null; // Force refresh the entries view
  }

  private _computeTabsLines: LineForSave[];
  private _computeTabsDefinitions: DocumentDefinitionForClient;
  private _visibleTabs: string[];
  private _invisibleTabs: string[];

  private computeTabs(model: Document): void {
    if (!model) {
      this._computeTabsLines = null;
      this._visibleTabs = [];
      this._invisibleTabs = [];
      return;
    }

    if (this._computeTabsLines !== model.Lines || this._computeTabsDefinitions !== this.definition) {
      this._computeTabsLines = model.Lines;
      this._computeTabsDefinitions = this.definition;

      // This tracks the line definition Ids from the document lines
      const linesTracker: { [key: string]: true } = {};

      // Add all definition IDs from the document lines to the lines tracker
      if (!!model.Lines) {
        for (const line of model.Lines) {
          linesTracker[line.DefinitionId] = true;
        }
      }

      // This tracks the line definition Ids from the definition
      const visibleTabs: string[] = [];
      const invisibleTabs: string[] = [];
      const invisibleTracker: { [key: string]: true } = {};
      const definitionTracker: { [key: string]: true } = {};
      const lineDefinitions = this.definition.LineDefinitions;

      // Add the visible line def Ids from definitions to the def tracker
      const visibleLineDefIdsFromDefinitions = lineDefinitions
        .filter(e => e.IsVisibleByDefault)
        .map(e => e.LineDefinitionId);

      for (const lineDefId of visibleLineDefIdsFromDefinitions) {
        if (!definitionTracker[lineDefId]) {
          definitionTracker[lineDefId] = true;
          visibleTabs.push(lineDefId);
        }
      }

      // Add the invisible line def Ids that have lines in the document to the def tracker
      const invisibleLineDefIdsFromDefinitions = lineDefinitions
        .filter(e => !e.IsVisibleByDefault)
        .map(e => e.LineDefinitionId);

      for (const lineDefId of invisibleLineDefIdsFromDefinitions) {
        if (!!linesTracker[lineDefId]) {
          if (!definitionTracker[lineDefId]) {
            definitionTracker[lineDefId] = true;
            visibleTabs.push(lineDefId);
          }
        } else {
          if (!invisibleTracker[lineDefId]) {
            invisibleTracker[lineDefId] = true;
            invisibleTabs.push(lineDefId);
          }
        }
      }

      // Add def Ids that are presenet in the lines but not the definition (anomalies)
      for (const lineDefId of Object.keys(linesTracker)) {
        if (!definitionTracker[lineDefId]) {
          definitionTracker[lineDefId] = true;
          visibleTabs.push(lineDefId);
        }
      }

      this._visibleTabs = visibleTabs;
      this._invisibleTabs = invisibleTabs;
    }
  }

  public visibleTabs(model: Document): string[] {

    this.computeTabs(model);
    return this._visibleTabs;
  }

  public invisibleTabs(model: Document): string[] {

    this.computeTabs(model);
    return this._invisibleTabs;
  }

  public onOtherTab(lineDefId: string): void {
    this._visibleTabs.push(lineDefId);
    this._visibleTabs = this._visibleTabs.slice();
    this._invisibleTabs = this._invisibleTabs.filter(e => e !== lineDefId);

    this.setActiveTab(lineDefId);
  }

  public tabTitle(lineDefId: string, model: DocumentForSave): string {
    if (lineDefId === 'ManualLine' && this.definitionId === 'manual-journal-vouchers') {
      return this.translate.instant('Entries');
    }

    const def = this.lineDefinition(lineDefId);
    const isForm = this.showAsForm(lineDefId, model);
    return !!def ? this.ws.getMultilingualValueImmediate(def, isForm ? 'TitleSingular' : 'TitlePlural') : lineDefId;
  }

  private _lines: { [key: string]: LineForSave[] };
  private _linesModel: DocumentForSave;

  public lines(lineDefId: string, model: Document): Line[] {
    if (!model) {
      return [];
    }

    if (this._linesModel !== model) {
      this._linesModel = model;
      this._lines = {};

      if (!!model.Lines) {
        for (const line of model.Lines) {
          if (!this._lines[line.DefinitionId]) {
            this._lines[line.DefinitionId] = [];
          }

          this._lines[line.DefinitionId].push(line);
        }
      }
    }

    if (!this._lines[lineDefId]) {
      this._lines[lineDefId] = [];
    }

    return this._lines[lineDefId];
  }

  public onInsertManualEntry(pair: LineEntryPair, model: Document): void {
    // Called when the user inserts a new entry
    model.Lines.push(pair.line);
  }

  public onDeleteManualEntry(pair: LineEntryPair, model: Document): void {

    this.cancelAutofill(pair); // Good tidying up

    // Called when the user deletes an entry
    const index = model.Lines.indexOf(pair.line);
    if (index > -1) {
      model.Lines.splice(index, 1);
    }
  }

  public onNewManualEntry = (pair: LineEntryPair) => {
    // Called when a new entry is created, including placeholder entry

    // Set the entry
    pair.entry = {
      Direction: 1
    };

    // Set the line
    pair.line = {
      DefinitionId: 'ManualLine',
      Entries: [pair.entry],
      _flags: { isModified: true }
    };

    return pair;
  }

  private _smartEntries: LineEntryPair[];
  private _manualEntries: LineEntryPair[];
  private _computeEntriesModel: DocumentForSave;

  private computeEntries(model: Document): void {
    if (!model) {
      return;
    }

    if (this._computeEntriesModel !== model) {
      this._computeEntriesModel = model;
      this._smartEntries = [];
      this._manualEntries = [];

      if (!!model.Lines) {
        model.Lines.forEach(line => {
          if (!!line.Entries) {
            line.Entries.forEach(entry => {
              if (line.DefinitionId === 'ManualLine') {
                this._manualEntries.push({ entry, line });
              } else if ((line.State || 0) >= 0) {
                this._smartEntries.push({ entry, line });
              }
            });
          }
        });
      }
    }
  }

  public smartEntries(model: Document): LineEntryPair[] {
    this.computeEntries(model);
    return this._smartEntries;
  }

  public manualEntries(model: Document): LineEntryPair[] {
    this.computeEntries(model);
    return this._manualEntries;
  }

  public showLineErrors(lineDefId: string, model: Document) {
    return !!model && !!model.Lines &&
      model.Lines.some(line => lineDefId === line.DefinitionId && (!!line.serverErrors ||
        (!!line.Entries && line.Entries.some(entry => !!entry.serverErrors))));
  }

  public showAttachmentsErrors(model: Document) {
    return !!model && !!model.Attachments &&
      model.Attachments.some(att => !!att.serverErrors);
  }

  public manualColumnPaths(model: DocumentForSave, smart = false): string[] {
    const paths = ['AccountId', 'Debit', 'Credit'];

    if (this.ws.settings.IsMultiCenter) {
      paths.splice(1, 0, 'Center');
    }

    if (!model.MemoIsCommon) {
      paths.push('Memo');
    }

    if (smart) {
      paths.push('ModifiedWarning');
    }

    paths.push('Commands');

    return paths;
  }

  public smartColumnPaths(lineDefId: string, doc: Document, isForm: boolean): string[] {
    // All line definitions other than 'ManualLine'
    const lineDef = this.lineDefinition(lineDefId);
    const isMultiRS = this.ws.settings.IsMultiCenter;
    const result = !!lineDef && !!lineDef.Columns ? lineDef.Columns
      .map((column, index) => ({ column, index })) // Capture the index first thing
      .filter(e => {
        const col = e.column;
        // Below are the conditions that make the column visible
        return !(!isMultiRS && col.ColumnName === 'CenterId') // It's not a CenterId in a single center db
          // AND it doesn't inherit from a document property marked IsCommon = true
          && (!col.InheritsFromHeader ||
            !(
              (doc.MemoIsCommon && col.ColumnName === 'Memo') ||
              (doc.DebitAgentIsCommon && col.ColumnName === 'AgentId' && lineDef.Entries[col.EntryIndex].Direction === 1) ||
              (doc.CreditAgentIsCommon && col.ColumnName === 'AgentId' && lineDef.Entries[col.EntryIndex].Direction === -1) ||
              (doc.NotedAgentIsCommon && col.ColumnName === 'NotedAgentId') ||
              (doc.InvestmentCenterIsCommon && col.ColumnName === 'CenterId') ||
              (doc.Time1IsCommon && col.ColumnName === 'Time1') ||
              (doc.Time2IsCommon && col.ColumnName === 'Time2') ||
              (doc.QuantityIsCommon && col.ColumnName === 'Quantity') ||
              (doc.UnitIsCommon && col.ColumnName === 'UnitId') ||
              (doc.CurrencyIsCommon && col.ColumnName === 'CurrencyId')
            )
          );
      })
      .map(e => e.index + '') : [];

    if (!isForm) {
      result.push('Commands');
    }

    return result;
  }

  private _columnTemplatesLineDefId: string;
  private _columnTemplatesDef: DocumentDefinitionForClient;
  private _columnTemplatesResult: ColumnTemplates;

  public columnTemplates(
    lineDefId: string,
    header: TemplateRef<any>,
    row: TemplateRef<any>,
    commandsTemplate: TemplateRef<any>): ColumnTemplates {

    const def = this.definition;
    if (this._columnTemplatesLineDefId !== lineDefId ||
      this._columnTemplatesDef !== def) {
      this._columnTemplatesLineDefId = lineDefId;
      this._columnTemplatesDef = def;
      this._columnTemplatesResult = null;
    }

    if (!this._columnTemplatesResult) {
      const templates: ColumnTemplates = {};

      // Add as many templates as there are columns
      const lineDef = this.lineDefinition(lineDefId);
      const columns = !!lineDef ? lineDef.Columns : [];
      const columnCount = columns.length;
      for (let colIndex = 0; colIndex < columnCount; colIndex++) {
        templates[colIndex + ''] = {
          headerTemplate: header,
          rowTemplate: row,
          weight: 1,
          argument: colIndex
        };
      }

      // Add the commands template
      templates.Commands = {
        rowTemplate: commandsTemplate,
      };

      this._columnTemplatesResult = templates;
    }

    return this._columnTemplatesResult;
  }

  private lineDefinition(lineDefId: string) {
    return !!lineDefId ? this.ws.definitions.Lines[lineDefId] : null;
  }

  private columnDefinition(lineDefId: string, columnIndex: number): LineDefinitionColumnForClient {
    const lineDef = this.lineDefinition(lineDefId);
    return !!lineDef ? lineDef.Columns[columnIndex] : null;
  }

  private entryDefinition(lineDefId: string, columnIndex: number): LineDefinitionEntryForClient {
    const lineDef = this.lineDefinition(lineDefId);
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return !!lineDef && !!lineDef.Entries && !!colDef ? lineDef.Entries[colDef.EntryIndex] : null;
  }

  public columnName(lineDefId: string, columnIndex: number): string {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return !!colDef ? colDef.ColumnName : null;
  }

  public columnLabel(lineDefId: string, columnIndex: number): string {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return this.ws.getMultilingualValueImmediate(colDef, 'Label');
  }

  /**
   * Returns either the line or one of its entries depending on the column definition
   */
  private entity(def: LineDefinitionColumnForClient, line: LineForSave): LineForSave | EntryForSave {
    let entity: LineForSave | EntryForSave;
    if (!!def) {
      if (def.TableName === 'Lines') {
        entity = line;
      } else if (def.TableName === 'Entries') {
        entity = !!line.Entries ? line.Entries[def.EntryIndex] : null;
      }
    }

    return entity;
  }

  public entry(lineDefId: string, columnIndex: number, line: LineForSave): EntryForSave {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    if (!!colDef && colDef.TableName === 'Entries') {
      return !!line.Entries ? line.Entries[colDef.EntryIndex] : null;
    }

    return null;
  }

  public agentDefinitionIds(lineDefId: string, columnIndex: number): string[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.AgentDefinitionId ? [entryDef.AgentDefinitionId] : [];
  }

  public notedAgentDefinitionIds(lineDefId: string, columnIndex: number): string[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.NotedAgentDefinitionId ? [entryDef.NotedAgentDefinitionId] : [];
  }

  public resourcesFilter(lineDefId: string, columnIndex: number): string {
    // Filter for smart line
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.AccountTypeParentIsResourceClassification &&
      !!entryDef.AccountTypeParentId ? `AccountType/Node descof ${entryDef.AccountTypeParentId}` : null;
  }

  public entryTypeFilter(lineDefId: string, columnIndex: number): string {
    // Filter for smart line
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.EntryTypeParentId ? `Node descof ${entryDef.EntryTypeParentId}` : null;
  }

  public serverErrors(lineDefId: string, columnIndex: number, line: LineForSave) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity && !!entity.serverErrors ? entity.serverErrors[colDef.ColumnName] : null;
  }

  public entityMetadata(lineDefId: string, columnIndex: number, line: LineForSave) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity && !!entity.EntityMetadata ? entity.EntityMetadata[colDef.ColumnName] : null;
  }

  public getFieldValue(lineDefId: string, columnIndex: number, line: LineForSave): any {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity ? entity[colDef.ColumnName] : null;
  }

  public setFieldValue(lineDefId: string, columnIndex: number, line: LineForSave, value: any): void {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    if (!!entity) {
      entity[colDef.ColumnName] = value;
    }
  }

  public isReadOnly(lineDefId: string, columnIndex: number, line: Line) {
    // return false;
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const state = line.State || 0;
    return state < 0 || state >= colDef.ReadOnlyState;
  }

  public isRequired(lineDefId: string, columnIndex: number, line: Line) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return (line.State || 0) >= colDef.RequiredState;
  }

  private _onNewLineFactoryLineDefId: string;
  private _onNewLineFactoryResult: (item: LineForSave) => LineForSave;

  public onNewSmartLineFactory(lineDefId: string): (item: LineForSave) => LineForSave {
    if (this._onNewLineFactoryLineDefId !== lineDefId) {
      this._onNewLineFactoryLineDefId = lineDefId;
      this._onNewLineFactoryResult = (line) => {
        // set the definition Id
        line.DefinitionId = lineDefId;
        line._flags = { isModified: true };
        // Add the specified number of entries
        line.Entries = [];
        const lineDef = this.lineDefinition(lineDefId);
        if (!!lineDef) {
          if (lineDef.Entries) {
            for (let i = 0; i < lineDef.Entries.length; i++) {
              const entryDef = lineDef.Entries[i];
              line.Entries[i] = { Direction: entryDef.Direction, Value: 0 };
            }
          } else {
            console.error(`Line definition ${lineDefId} is missing its Entries`);
          }
        } else {
          console.error(`Missing line definition ${lineDefId}`);
        }

        return line;
      };
    }

    return this._onNewLineFactoryResult;
  }

  public columnLabelAlignment(lineDefId: string, columnIndex: number): string {
    if (this.workspace.ws.isRtl) {
      return null;
    }

    const colName = this.columnName(lineDefId, columnIndex);
    switch (colName) {
      case 'MonetaryValue':
      case 'Quantity':
      case 'Value':
      case 'NotedAmount':
        return 'right';
      default:
        return null;
    }
  }

  public showAsForm(lineDefId: string, model: DocumentForSave) {
    const count = this.lines(lineDefId, model).length;
    if (count > 1) {
      return false;
    } else {
      const lineDef = this.lineDefinition(lineDefId);
      return !!lineDef ? lineDef.ViewDefaultsToForm : false;
    }
  }

  public dummyUpdate = () => { };

  public onCreateForm(lineDefId: string, model: DocumentForSave) {
    if (!model) {
      return;
    }

    let newLine: LineForSave = {};
    newLine = this.onNewSmartLineFactory(lineDefId)(newLine);
    this.lines(lineDefId, model).push(newLine);
    this.onInsertSmartLine(newLine, model);
  }

  public onDeleteForm(lineDefId: string, model: DocumentForSave) {
    if (!model) {
      return;
    }

    const lines = this.lines(lineDefId, model);
    if (lines.length === 1) {
      const line = lines.pop();
      this.onDeleteSmartLine(line, model);
    }
  }

  public get actionsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get modifiedTooltipPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public get commandsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  // Serial Number
  public get isOriginalDocument(): boolean {
    return this.definition.IsOriginalDocument;
  }

  // To work around a bug in Angular compiler
  public isNegativeStateActive = (state: LineState, model: Document) =>
    this.isStateActive(-state as LineState, model)

  // To work around a bug in Angular compiler
  public isNegativeDocumentStateActive = (state: DocumentState, model: Document) =>
    this.isDocumentStateActive(-state as DocumentState, model)

  // The state chart
  public isStateActive(state: LineState, model: Document): boolean {
    if (!model) {
      return false;
    }

    const def = this.definition;
    if (!def) {
      return false;
    }

    return !model.State && def.HasWorkflow && this.getDocState(model) === state;
  }

  public isDocumentStateActive(state: DocumentState, model: Document): boolean {
    if (!model) {
      return false;
    }

    const def = this.definition;
    if (!def) {
      return false;
    }

    if (state === 0) { // Current
      return !model.State && !def.HasWorkflow;
    } else { // Posted + Canceled
      return model.State === state;
    }
  }

  public isStateVisible(state: LineState, model: Document): boolean {
    // Returns if a positive state is visible on the wide screen flow chart
    if (!!model && (model.State < 0 || this.getDocState(model) < 0)) { // <-- Review
      return false;
    }

    const def = this.definition;
    if (state === 0 || state === 4) {
      return !!def && def.HasWorkflow;
    } else { // 1 + 2 + 3
      return this.isStateActive(state, model) ||
        (!!def && def.HasWorkflow && def['CanReachState' + state]);
    }
  }

  public isDocumentStateVisible(state: DocumentState, _: Document): boolean {
    // Returns if a positive state is visible on the wide screen flow chart
    const def = this.definition;
    if (state === 0) { // Current
      return !def || !def.HasWorkflow;
    } else { // Posted
      return true;
    }
  }

  public isPositiveState(model: Document): boolean {
    const states: LineState[] = [0, 1, 2, 3, 4];
    const documentStates: DocumentState[] = [0, 1];

    return states.some(state => this.isStateActive(state, model)) ||
      documentStates.some(state => this.isDocumentStateActive(state, model));
  }

  ////////////// Posting State

  public onDocumentState(
    doc: Document,
    fn: (ids: (number | string)[], args: ActionArguments, extras?: { [key: string]: any }) => Observable<EntitiesResponse<Document>>) {
    fn([doc.Id], {
      returnEntities: true,
      expand: this.expand
    }, { includeRequiredSignatures: true }).pipe(
      tap(res => {
        addToWorkspace(res, this.workspace);
        this.details.state.extras = res.Extras;
        this.handleFreshExtras(res.Extras);
      })
    ).subscribe({ error: this.details.handleActionError });
  }

  public onPost(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.post);
  }

  public onUnpost(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.unpost);
  }

  public onCancel(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.cancel);
  }

  public onUncancel(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.uncancel);
  }

  public hasPermissionToUpdateState(doc: Document): boolean {
    return this.ws.canDo(this.view, 'State', !!doc ? doc.CreatedById : null);
  }

  private missingSignatures(_: Document, requiredSignatures: RequiredSignature[]): boolean {
    return !!requiredSignatures && requiredSignatures.length > 0 &&
      requiredSignatures.some(e => !e.SignedById);
  }

  private someWorkflowLinesAreNotNegative(_: Document, requiredSignatures: RequiredSignature[]): boolean {
    // There are pending lines either if some of the lines have state > 0 OR if some signatures are still pending
    return (!!requiredSignatures && requiredSignatures.length > 0 &&
      requiredSignatures.some(e => !e.LastNegativeState));
  }

  // Post

  public showPost(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !doc.State;
  }

  public disablePost(doc: Document, requiredSignatures: RequiredSignature[]): boolean {

    return !doc || !doc.Id || // Missing document
      // Missing posting date
      !doc.PostingDate ||
      // OR missing permissions
      !this.hasPermissionToUpdateState(doc) ||
      // OR missing signatures
      this.missingSignatures(doc, requiredSignatures);
  }

  public postTooltip(doc: Document, requiredSignatures: RequiredSignature[]): string {

    if (!this.hasPermissionToUpdateState(doc)) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    } else if (this.missingSignatures(doc, requiredSignatures)) {
      return this.translate.instant('Error_LineIsMissingSignatures');
    } else if (!!doc && !doc.PostingDate) {
      return this.translate.instant('Error_ThePostingDateIsRequiredForPosting');
    }

    return null;
  }

  // Cancel

  public showCancel(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !doc.State;
  }

  public disableCancel(doc: Document, requiredSignatures: RequiredSignature[]): boolean {

    return !doc || !doc.Id || // Missing document
      // OR missing permissions
      !this.hasPermissionToUpdateState(doc) ||
      // OR some lines are still pending
      this.someWorkflowLinesAreNotNegative(doc, requiredSignatures);
  }

  public cancelTooltip(doc: Document, requiredSignatures: RequiredSignature[]): string {

    if (!this.hasPermissionToUpdateState(doc)) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    } else if (this.someWorkflowLinesAreNotNegative(doc, requiredSignatures)) {
      return this.translate.instant('Error_AllLinesMustBeInNegativeState');
    }

    return null;
  }

  // Unpost & Uncancel

  public showUnpost(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !!doc.Id && doc.State === 1;
  }

  public showUncancel(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !!doc.Id && doc.State === -1;
  }

  public updateStateTooltip(doc: Document): string {
    return this.hasPermissionToUpdateState(doc) ? null : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public entriesCount(doc: DocumentForSave) {
    return this.smartEntries(doc).length + this.manualEntries(doc).length;
  }

  public get hasManualLines(): boolean {
    return this.definition.LineDefinitions
      .some(e => e.LineDefinitionId === 'ManualLine');
  }

  private _flagsModel: DocumentForSave;
  private _flags: { [id: number]: LineFlags } = {};

  private flags(line: LineForSave, doc: DocumentForSave, setUndefined = false): LineFlags {
    if (this._flagsModel !== doc) {
      this._flagsModel = doc;
      this._flags = {};
    }
    if (!!line.Id) {
      let result = this._flags[line.Id];
      if (setUndefined && !result) {
        result = (this._flags[line.Id] = {});
      }

      return result;
    } else {
      let result = line._flags;
      if (setUndefined && !result) {
        result = (line._flags = {});
      }

      return result;
    }
  }

  public getIsModified(line: LineForSave, doc: DocumentForSave): boolean {
    const flags = this.flags(line, doc);
    return !!flags ? flags.isModified : false;
  }

  public setModified(line: LineForSave, doc: DocumentForSave) {
    this.flags(line, doc, true).isModified = true;
  }

  public get isJV(): boolean {
    return this.definitionId === 'manual-journal-vouchers';
  }

  public onSmartLineUpdated(update: (item: LineForSave) => void, line: LineForSave, doc: DocumentForSave) {
    this.setModified(line, doc); // Flags the line as modified
    update(line);
  }

  private _highlightPairFactoryModel: DocumentForSave;
  private _highlightPairFactoryResult: (pair: LineEntryPair) => boolean;

  public highlightPairFactory(doc: DocumentForSave) {
    if (this._highlightPairFactoryModel !== doc) {
      this._highlightPairFactoryModel = doc;
      this._highlightPairFactoryResult = null;
    }

    if (!this._highlightPairFactoryResult) {
      this._highlightPairFactoryResult = (pair: LineEntryPair) => {
        const flags = this.flags(pair.line, doc);
        return !!flags ? flags.isHighlighted : false;
      };
    }

    return this._highlightPairFactoryResult;
  }

  private _highlightLineFactoryModel: DocumentForSave;
  private _highlightLineFactoryResult: (line: LineForSave) => boolean;

  public highlightLineFactory(doc: DocumentForSave) {
    if (this._highlightLineFactoryModel !== doc) {
      this._highlightLineFactoryModel = doc;
      this._highlightLineFactoryResult = null;
    }

    if (!this._highlightLineFactoryResult) {
      this._highlightLineFactoryResult = (line: LineForSave) => {
        const flags = this.flags(line, doc);
        return !!flags ? flags.isHighlighted : false;
      };
    }

    return this._highlightLineFactoryResult;
  }

  public toggleHighlight(line: LineForSave, doc: DocumentForSave) {
    const flags = this.flags(line, doc, true);
    flags.isHighlighted = !flags.isHighlighted;

    // To trigger OnPush change detection of t-table
    this._highlightLineFactoryResult = null;
    this._highlightPairFactoryResult = null;
  }

  public isHighlighted(line: LineForSave, doc: DocumentForSave): boolean {
    const isHighlightedLine = this.highlightLineFactory(doc);
    return isHighlightedLine(line);
  }

  public highlightSmartTab(lineDefId: string, doc: DocumentForSave): boolean {
    const isHighlightedLine = this.highlightLineFactory(doc);
    return this.lines(lineDefId, doc).some(isHighlightedLine);
  }

  public smartTabColor(lineDefId: string, doc: DocumentForSave): string {
    return this.highlightSmartTab(lineDefId, doc) ? '#eeff44' : null;
  }
  public highlightBookkeepingTab(doc: Document): boolean {
    const isHighlightedLine = this.highlightLineFactory(doc);
    return !!doc && !!doc.Lines && doc.Lines.some(e => (e.State || 0) >= 0 && isHighlightedLine(e));
  }

  public bookkeepingTabColor(doc: DocumentForSave): string {
    return this.highlightBookkeepingTab(doc) ? '#eeff44' : null;
  }

  public formColor(lineDefId: string, doc: DocumentForSave): string {
    const isHighlightedLine = this.highlightLineFactory(doc);
    const lines = this.lines(lineDefId, doc);
    return lines.length === 1 && isHighlightedLine(lines[0]) ? '#eeff44' : null;
  }

  private _documentStateDoc: Document;
  private _documentStateResult: LineState = null;

  /**
   * Returns a summary of the states of all the lines
   */
  public getDocState(doc: Document): LineState {
    if (this._documentStateDoc !== doc) {
      this._documentStateDoc = doc;
      this._documentStateResult = null;
    }

    if (this._documentStateResult === null) {
      if (!doc || !doc.Lines) {
        this._documentStateResult = 0;
      } else {
        const allLineStates = doc.Lines.map(line => line.State || 0);
        const positiveStates = allLineStates.filter(state => state >= 0);
        if (positiveStates.length > 0) {
          // Result is the smallest positive state
          this._documentStateResult = Math.min(...positiveStates) as LineState;
        } else if (allLineStates.length > 0) {
          // Result is the smallest (negative) state
          const negativeState = allLineStates.filter(state => state < 0);
          this._documentStateResult = Math.min(...negativeState) as LineState;
        } else {
          // Result is Draft
          this._documentStateResult = 0;
        }
      }
    }

    return this._documentStateResult;
  }

  public onAutofillFromExchangeRate(pair: LineEntryPair, doc: DocumentForSave, direction: 1 | -1) {

    this.cancelAutofill(pair); // cancel any previous calls

    const entry = pair.entry;
    const amount = entry.MonetaryValue;
    if (!amount) {
      entry.Value = 0;
      return;
    } else {
      const date = doc.PostingDate || toLocalDateISOString(new Date());
      const currencyId = this.readonlyValueCurrencyId(pair.entry);
      if (!!currencyId) {
        pair.direction = direction;
        pair.subscription = this.api.exchangeRatesApi(this.notifyDestruct$)
          .convertToFunctional(date, currencyId, amount)
          .pipe(
            tap(result => {
              this.cancelAutofill(pair); // remove the rotator

              // Don't update the value if the arguments have changed during the call
              const currentAmount = entry.MonetaryValue;
              const currentCurrencyId = this.readonlyValueCurrencyId(pair.entry);
              const currentDate = doc.PostingDate || date;

              if (this.details.isEdit &&
                currentAmount === amount &&
                currentCurrencyId === currencyId &&
                currentDate === date) {
                // Set the values from the server
                entry.Value = result;
                entry.Direction = direction;
              }
            }),
            catchError((error: FriendlyError) => {
              this.cancelAutofill(pair); // remove the rotator

              // Show a suitable error message
              if (error.status === 404) {
                const message = this.translate.instant('Error_NoExRateFoundForCurrency0Date1', {
                  0: this.ws.getMultilingualValue('Currency', currencyId, 'Name'),
                  1: formatDate(date, 'yyyy-MM-dd', 'en-GB')
                });
                this.details.displayModalError(message);
              } else {
                this.details.displayModalError(error.error);
              }

              return of(null);
            }),
            finalize(() => this.cancelAutofill(pair)) // removes the rotator
          ).subscribe();
      }
    }
  }

  /**
   * Cancels any pending server calls for auto-fill from exchange rate
   */
  public cancelAutofill(pair: LineEntryPair): void {
    if (!!pair.subscription) {
      pair.subscription.unsubscribe();
      delete pair.subscription;
      delete pair.direction;
    }
  }

  public onCloneManualLine(pair: LineEntryPair, doc: Document) {
    const clone = JSON.parse(JSON.stringify(pair.line)) as Line;
    delete clone.Id;
    delete clone.serverErrors;

    // TODO
  }

  public onAutoBalance(pair: LineEntryPair, doc: Document, update: (pair: LineEntryPair) => void): void {
    if (!doc || !doc.Lines) {
      return;
    }

    const currentEntry = pair.entry;
    const currentEntryValue = (currentEntry.Value || 0) * (currentEntry.Direction || 1);
    const restOfTheDocumentDifference = doc.Lines
      .filter(line => (line.State || 0) >= 0)
      .map(line => {
        return !!line.Entries ? line.Entries
          .map(entry => (entry.Value || 0) * (entry.Direction || 1))
          .reduce((total, v) => total + v, 0) : 0;
      })
      .reduce((total, v) => total + v, 0) - currentEntryValue;

    if (restOfTheDocumentDifference > 0) {
      currentEntry.Direction = -1;
      currentEntry.Value = Math.abs(restOfTheDocumentDifference);
      update(pair);
    } else if (restOfTheDocumentDifference < 0) {
      currentEntry.Direction = 1;
      currentEntry.Value = Math.abs(restOfTheDocumentDifference);
      update(pair);
    } else if (!pair.PH) {
      currentEntry.Direction = 1;
      delete currentEntry.Value;
      update(pair);
    }
  }

  public total(doc: Document, direction: number) {
    direction = direction as 1 | -1; // To avoid an Angular template binding bug

    if (!doc || !doc.Lines) {
      return null;
    }

    return direction * doc.Lines
      .filter(line => (line.State || 0) >= 0)
      .map(line => {
        return !!line.Entries ? line.Entries
          .filter(entry => entry.Direction === direction)
          .map(entry => (entry.Value || 0) * (entry.Direction || 1))
          .reduce((total, v) => total + v, 0) : 0;
      })
      .reduce((total, v) => total + v, 0);
  }

  public get functionalName() {
    return this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName');
  }
}

/* Rules for showing and hiding chart states

  -------- IsActive
  [-4] !State and !!HasWorkflow and State === -4
  [-3] !State and !!HasWorkflow and State === -3
  [-2] !State and !!HasWorkflow and State === -2
  [-1] !State and !!HasWorkflow and State === -1
  [0] !State and !!HasWorkflow and State === 0
  [1] !State and !!HasWorkflow and State === 1
  [2] !State and !!HasWorkflow and State === 2
  [3] !State and !!HasWorkflow and State === 3
  [4] !State and !!HasWorkflow and State === 4
  [Current] !State and !HasWorkflow
  [Posted] State === 1
  [Canceled] State === -1

  --------- IsVisible (In +ve state and wide screen)
  [0] !!HasWorkflow
  [1] (!!HasWorkflow && CanReachState1) || isActive(1)
  [2] (!!HasWorkflow && CanReachState2) || isActive(2)
  [3] (!!HasWorkflow && CanReachState3) || isActive(3)
  [4] !!HasWorkflow || isActive(4)
  [Current] !HasWorkflow
  [Posted] Always

  --------- IsVisible (In -ve state or narrow screen)
  IsVisible = IsActive

*/
