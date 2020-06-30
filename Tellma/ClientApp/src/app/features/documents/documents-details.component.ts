// tslint:disable:member-ordering
import { Component, Input, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService, TenantWorkspace, MasterDetailsStore } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Params } from '@angular/router';
import { DocumentForSave, Document, formatSerial, DocumentClearance, metadata_Document, DocumentState } from '~/app/data/entities/document';
import {
  DocumentDefinitionForClient,
  LineDefinitionColumnForClient, LineDefinitionEntryForClient, DefinitionsForClient, LineDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { LineForSave, Line, LineState, LineFlags } from '~/app/data/entities/line';
import { Entry, EntryForSave } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import {
  addToWorkspace, getDataURL, downloadBlob,
  fileSizeDisplay, mergeEntitiesInWorkspace,
  toLocalDateISOString, FriendlyError, printBlob
} from '~/app/data/util';
import { tap, catchError, finalize, takeUntil, skip } from 'rxjs/operators';
import { NgbModal, Placement } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError, Observable, Subscription } from 'rxjs';
import { AccountForSave, metadata_Account } from '~/app/data/entities/account';
import { Resource, metadata_Resource } from '~/app/data/entities/resource';
import { Currency } from '~/app/data/entities/currency';
import { metadata_Contract, Contract } from '~/app/data/entities/contract';
import { AccountType } from '~/app/data/entities/account-type';
import { Attachment } from '~/app/data/entities/attachment';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';
import { RequiredSignature } from '~/app/data/entities/required-signature';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ActionArguments } from '~/app/data/dto/action-arguments';
import { EntitiesResponse } from '~/app/data/dto/entities-response';
import { getChoices, ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { DocumentStateChange } from '~/app/data/entities/document-state-change';
import { formatDate } from '@angular/common';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';

type DocumentDetailsView = 'Managerial' | 'Accounting';
interface LineEntryPair {
  entryIndex: number;
  entry: EntryForSave;
  line: LineForSave;
  subscription?: Subscription; // cancels API calls specific to this line
  direction?: 1 | -1; // tracks whether the API call result will be debit or credit
  PH?: boolean;
}

interface DocumentDetailsState {
  tab: number;
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

  private documentsApi = this.api.documentsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;
  private _maxAttachmentSize = 20 * 1024 * 1024;
  private _pristineDocJson: string;
  private localState = new MasterDetailsStore();  // Used in popup mode

  // These are bound from UI
  public assigneeId: number;
  public comment: string;
  public picSize = 36;

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

  @ViewChild('confirmModal', { static: true })
  confirmModal: TemplateRef<any>;

  @ViewChild('negativeSignatureModal', { static: true })
  negativeSignatureModal: TemplateRef<any>;

  public confirmationMessage: string;
  public signatureForNegativeModal: RequiredSignature;
  public reasonChoicesForNegativeModal: SelectorChoice[];
  public reasonDetails: string;
  public reasonId: number;

  public select = '$Details'; // The server understands this keyword, no need to list all hundreds of select paths
  public additionalSelectAccount = '$DocumentDetails';
  public additionalSelectResource = '$DocumentDetails';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
    super();
  }

  ngOnInit() {

    const handleFreshStateFromUrl = (params: ParamMap) => {

      if (this.isScreenMode) {
        // Definitoin Id, must be set before retrieving the state
        this.definitionId = +params.get('definitionId') || null;
        const s = this.state.detailsState as DocumentDetailsState;

        // When set to true, it means the url is out of step with the state
        let triggerUrlStateChange = false;

        // Active tab
        const urlTab = +params.get('tab');
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

  public setActiveTab(newTab: number) {
    (this.state.detailsState as DocumentDetailsState).tab = newTab;
    setTimeout(() => {
      // Otherwise details may be null
      this.details.urlStateChange();
    });
  }

  public getActiveTab(model: Document): number {
    // Special tabs
    const s = this.state.detailsState as DocumentDetailsState;
    // -20 = Attachments
    // -10 = Bookkeeping
    if (s.tab === -20 || (!this.isJV && s.tab === -10)) {
      return s.tab;
    }

    // Make sure the selected tab is a visible one
    const visibleTabs = this.visibleTabs(model);
    if (visibleTabs.some(e => e === s.tab)) {
      return s.tab;
    } else {
      // Get the first visible tab
      return visibleTabs[0] || -10;
    }
  }

  /**
   * Built-in hardcoded definition
   */
  public get isJV(): boolean {
    return this.definitionId === this.ws.definitions.ManualJournalVouchersDefinitionId;
  }

  /**
   * Built-in hardcoded definition
   */
  public isManualLine(defId: number): boolean {
    return defId === this.ws.definitions.ManualLinesDefinitionId;
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

    if (this.isJV) {

      // Posting Date
      result.PostingDate = toLocalDateISOString(new Date());

      // Is Common
      result.PostingDateIsCommon = true;
      result.MemoIsCommon = true;
      result.DebitContractIsCommon = false;
      result.CreditContractIsCommon = false;
      result.NotedContractIsCommon = false;
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

      result.PostingDateIsCommon = !!def.PostingDateVisibility;
      result.MemoIsCommon = !!def.MemoIsCommonVisibility;
      result.DebitContractIsCommon = !!def.DebitContractVisibility;
      result.CreditContractIsCommon = !!def.CreditContractVisibility;
      result.NotedContractIsCommon = !!def.NotedContractVisibility;
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
      return 'Error_OpenDocumentBeforeEdit';
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
      return 'ClosedThisDocument';
    } else if (event.toState === -1) {
      return 'CanceledThisDocument';
    } else {
      if (event.fromState === 1) {
        return 'OpenedThisDocument';
      } else {
        return 'UncanceledThisDocument';
      }
    }
  }

  public onAssign(doc: Document): void {
    if (!!this.assigneeId) {
      this.documentsApi.assign([doc.Id], {
        returnEntities: true,
        select: this.select,
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
    return this.definition.MemoIsCommonVisibility && !this.isJV;
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

  // Posting Date

  public showDocumentPostingDate(_: DocumentForSave) {
    return this.definition.PostingDateVisibility || this.isJV;
  }

  public showDocumentPostingDateIsCommon(_: Document): boolean {
    return !this.isJV;
  }

  public requireDocumentPostingDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentPostingDate || this.isJV;
  }

  public readonlyDocumentPostingDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentPostingDate && !this.isJV;
  }

  public labelDocumentPostingDate(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'PostingDateLabel') ||
      this.translate.instant('Document_PostingDate');
  }

  // DebitContract

  public showDocumentDebitContract(_: DocumentForSave): boolean {
    return this.definition.DebitContractVisibility;
  }

  public requireDocumentDebitContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDebitContract;
  }

  public readonlyDocumentDebitContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDebitContract;
  }

  public labelDocumentDebitContract(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'DebitContractLabel');
    if (!label) {
      const contractDefId = this.definition.DebitContractDefinitionId;
      const contractDef = this.ws.definitions.Contracts[contractDefId];
      if (!!contractDef) {
        label = this.ws.getMultilingualValueImmediate(contractDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Contract');
      }
    }

    return label;
  }

  public documentDebitContractDefinitionIds(_: DocumentForSave): number[] {
    return [this.definition.DebitContractDefinitionId];
  }

  // CreditContract

  public showDocumentCreditContract(_: DocumentForSave): boolean {
    return this.definition.CreditContractVisibility;
  }

  public requireDocumentCreditContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireCreditContract;
  }

  public readonlyDocumentCreditContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyCreditContract;
  }

  public labelDocumentCreditContract(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'CreditContractLabel');
    if (!label) {
      const contractDefId = this.definition.CreditContractDefinitionId;
      const contractDef = this.ws.definitions.Contracts[contractDefId];
      if (!!contractDef) {
        label = this.ws.getMultilingualValueImmediate(contractDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Contract');
      }
    }

    return label;
  }

  public documentCreditContractDefinitionIds(_: DocumentForSave): number[] {
    return [this.definition.CreditContractDefinitionId];
  }

  // NotedContract

  public showDocumentNotedContract(_: DocumentForSave): boolean {
    return this.definition.NotedContractVisibility;
  }

  public requireDocumentNotedContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireNotedContract;
  }

  public readonlyDocumentNotedContract(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyNotedContract;
  }

  public labelDocumentNotedContract(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'NotedContractLabel');
    if (!label) {
      const contractDefId = this.definition.NotedContractDefinitionId;
      const contractDef = this.ws.definitions.Contracts[contractDefId];
      if (!!contractDef) {
        label = this.ws.getMultilingualValueImmediate(contractDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Contract');
      }
    }

    return label;
  }

  public documentNotedContractDefinitionIds(_: DocumentForSave): number[] {
    return [this.definition.NotedContractDefinitionId];
  }

  // Segment

  public showDocumentSegment(_: DocumentForSave) {
    return this.ws.settings.IsMultiSegment;
  }

  public labelDocumentSegment(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'SegmentLabel') ||
      this.translate.instant('Document_Segment');
  }

  public readonlyDocumentSegment(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return false; // TODO
  }

  // Time 1

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
  private _requireDocumentPostingDate: boolean;
  private _readonlyDocumentPostingDate: boolean;
  private _requireDebitContract: boolean;
  private _readonlyDebitContract: boolean;
  private _requireCreditContract: boolean;
  private _readonlyCreditContract: boolean;
  private _requireNotedContract: boolean;
  private _readonlyNotedContract: boolean;
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
      this._requireDocumentPostingDate = false;
      this._readonlyDocumentPostingDate = false;
      this._requireDebitContract = false;
      this._readonlyDebitContract = false;
      this._requireCreditContract = false;
      this._readonlyCreditContract = false;
      this._requireNotedContract = false;
      this._readonlyNotedContract = false;
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
      this._requireDocumentPostingDate = def.PostingDateRequiredState === 0;
      this._readonlyDocumentPostingDate = def.PostingDateReadOnlyState === 0;
      this._requireDebitContract = def.DebitContractRequiredState === 0;
      this._readonlyDebitContract = def.DebitContractReadOnlyState === 0;
      this._requireCreditContract = def.CreditContractRequiredState === 0;
      this._readonlyCreditContract = def.CreditContractReadOnlyState === 0;
      this._requireNotedContract = def.NotedContractRequiredState === 0;
      this._readonlyNotedContract = def.NotedContractReadOnlyState === 0;
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
            case 'PostingDate':
              if (!this._requireDocumentPostingDate &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentPostingDate = true;
              }
              if (!this._readonlyDocumentPostingDate &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentPostingDate = true;
              }
              break;
            case 'ContractId':
              if (!this._requireDebitContract && lineDef.Entries[colDef.EntryIndex].Direction === 1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDebitContract = true;
              }
              if (!this._requireCreditContract && lineDef.Entries[colDef.EntryIndex].Direction === -1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireCreditContract = true;
              }

              if (!this._readonlyDebitContract && lineDef.Entries[colDef.EntryIndex].Direction === 1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDebitContract = true;
              }
              if (!this._readonlyCreditContract && lineDef.Entries[colDef.EntryIndex].Direction === -1 &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyCreditContract = true;
              }
              break;

            case 'NotedContractId':
              if (!this._requireNotedContract &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireNotedContract = true;
              }
              if (!this._readonlyNotedContract &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyNotedContract = true;
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

  public accountType(entry: Entry): AccountType {
    const account = this.account(entry);
    if (!!account && account.AccountTypeId) {
      return this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    }

    return null;
  }

  public resource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const resourceId = accountResourceId || entry.ResourceId;
    return this.ws.get('Resource', resourceId) as Resource;
  }

  // private resourceDefinition(entry: Entry): ResourceDefinitionForClient {
  //   const resource = this.resource(entry);
  //   const defId = !!resource ? resource.DefinitionId : null;
  //   const resourceDefinition = !!defId ? this.ws.definitions.Resources[defId] : null;
  //   return resourceDefinition;
  // }

  public accountDisplay(accountId: number) {
    const account = this.ws.get('Account', accountId);
    if (!!account) {
      const desc = metadata_Account(this.workspace, this.translate);
      return desc.format(account);
    }

    return '';
  }

  public filterAccount(pair: LineEntryPair, bookkeeping: boolean): string {
    if (!bookkeeping) {
      return null;
    }

    // Deconstruct the pair object
    const line = pair.line;
    const entry = pair.entry;
    const entryIndex = pair.entryIndex;

    const lineDefId = line.DefinitionId;
    const lineDef = this.lineDefinition(lineDefId);
    if (!!lineDef && !!lineDef.Entries) {
      const entryDef = lineDef.Entries[entryIndex];
      if (!!entryDef && !!entryDef.AccountTypeId) {
        // Account Type Id
        let filter = `AccountType/Node descof ${entryDef.AccountTypeId}`;

        // CurrencyId
        const currencyId = entry.CurrencyId; // this.readonlyValueCurrencyId(entry) || entry.CurrencyId;
        if (!!currencyId) {
          filter = filter + ` and (CurrencyId eq null or CurrencyId eq '${currencyId.replace(`'`, `''`)}')`;
        }

        // CenterId
        const centerId = entry.CenterId; // this.readonlyValueCenterId_Manual(entry) || entry.centerId;
        if (!!centerId) {
          filter = filter + ` and (CenterId eq null or CenterId eq ${centerId})`;
        }

        // ResourceDefinitionId
        const resource = this.ws.get('Resource', entry.ResourceId) as Resource;
        const resourceDefId = !!resource ? resource.DefinitionId : null;
        if (!!resourceDefId) {
          filter = filter + ` and ResourceDefinitionId eq ${resourceDefId}`;
        } else {
          filter = filter + ` and ResourceDefinitionId eq null`;
        }

        // ContractDefinitionId
        const contract = this.ws.get('Contract', entry.ContractId) as Contract;
        const contractDefId = !!contract ? contract.DefinitionId : null;
        if (!!contractDefId) {
          filter = filter + ` and ContractDefinitionId eq ${contractDefId}`;
        } else {
          filter = filter + ` and ContractDefinitionId eq null`;
        }

        // ResourceId
        const resourceId = entry.ResourceId;
        if (!!resourceId) {
          filter = filter + ` and (ResourceId eq null or ResourceId eq ${resourceId})`;
        }

        // ContractId
        const contractId = entry.ContractId;
        if (!!contractId) {
          filter = filter + ` and (ContractId eq null or ContractId eq ${contractId})`;
        }

        // EntryTypeId
        const entryTypeId = entry.EntryTypeId;
        if (!!entryTypeId) {
          filter = filter + ` and (EntryTypeId eq null or EntryTypeId eq ${entryTypeId})`;
        }

        return filter;
      }
    }

    return null;
  }

  // Center

  public readonlyCenter_Manual(entry: Entry): boolean {
    const at = this.account(entry);
    return !!at && !!at.CenterId;
  }

  public readonlyValueCenterId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.CenterId : null;
  }

  // ContractId

  public showContract_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ContractDefinitionId;
  }

  public readonlyContract_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ContractId;
  }

  public readonlyValueContractId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.ContractId : null;
  }

  public labelContract_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.ContractDefinitionId : null;

    return metadata_Contract(this.workspace, this.translate, defId).titleSingular();
  }

  public definitionIdsContract_Manual(entry: Entry): number[] {
    const account = this.account(entry);
    return [account.ContractDefinitionId];
    // return !!account && !!account.ContractDefinitions ? account.ContractDefinitions.map(e => e.ContractDefinitionId) : [];
  }

  public contract(entry: Entry): Contract {
    const account = this.account(entry);
    const accountContractId = !!account ? account.ContractId : null;
    const contractId = accountContractId || entry.ContractId;
    return this.ws.get('Contract', contractId) as Contract;
  }

  // Noted Contract Id

  public showNotedContract_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.NotedContractDefinitionId;
  }

  public labelNotedContract_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.NotedContractDefinitionId : null;

    return metadata_Contract(this.workspace, this.translate, defId).titleSingular();
  }

  public definitionIdsNotedContract_Manual(entry: Entry): number[] {
    const account = this.account(entry);
    return [account.NotedContractDefinitionId];
    // return !!at && !!at.NotedContractDefinitions ? at.NotedContractDefinitions.map(e => e.NotedContractDefinitionId) : [];
  }

  // ResourceId

  public showResource_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ResourceDefinitionId;
  }

  public readonlyResource_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.ResourceId;
  }

  public readonlyValueResourceId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.ResourceId : null;
  }

  public labelResource_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.ResourceDefinitionId : null;

    return metadata_Resource(this.workspace, this.translate, defId).titleSingular();
  }

  public definitionIdsResource_Manual(entry: Entry): number[] {
    const account = this.account(entry);
    return [account.ResourceDefinitionId];
    // return !!at && !!at.ResourceDefinitions ? at.ResourceDefinitions.map(e => e.ResourceDefinitionId) : [];
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

  // DueDate

  public showDueDate_Manual(entry: Entry): boolean {
    // const resourceDefinition = this.resourceDefinition(entry);
    // return !!resourceDefinition && !!resourceDefinition.DueDateVisibility;
    const at = this.accountType(entry);
    return !!at && !!at.DueDateLabel;
  }

  public labelDueDate_Manual(entry: Entry): string {
    // const rd = this.resourceDefinition(entry);
    // const at = this.accountType(entry);
    // return !!rd.DueDateLabel ? this.ws.getMultilingualValueImmediate(rd, 'DueDateLabel') :
    //   !!at.DueDateLabel ? this.ws.getMultilingualValueImmediate(at, 'DueDateLabel') :
    //     this.translate.instant('Entry_DueDate');

    const at = this.accountType(entry);
    return this.ws.getMultilingualValueImmediate(at, 'DueDateLabel');
  }

  // Time1

  public showTime1_Manual(entry: Entry): boolean {
    const at = this.accountType(entry);
    return !!at && !!at.Time1Label;
  }

  public labelTime1_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return this.ws.getMultilingualValueImmediate(at, 'Time1Label');
  }

  // Time2

  public showTime2_Manual(entry: Entry): boolean {
    const at = this.accountType(entry);
    return !!at && !!at.Time2Label;
  }

  public labelTime2_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return this.ws.getMultilingualValueImmediate(at, 'Time2Label');
  }

  // Currency

  public showCurrency(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !this.getAccountResourceCurrencyId(entry);
  }

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

  // Entry Type

  public showEntryType_Manual(entry: Entry): boolean {
    // Show entry type when the account's type has an entry type parent Id
    const at = this.accountType(entry);
    if (!!at) {
      const entryTypeParent = this.ws.get('EntryType', at.EntryTypeParentId);
      return !!entryTypeParent && entryTypeParent.IsActive;
    }

    return false;
  }

  public readonlyEntryType_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.EntryTypeId;
  }

  public readonlyValueEntryTypeId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.EntryTypeId : null;
  }

  public filterEntryType_Manual(entry: Entry): string {
    const accountType = this.accountType(entry);
    return `IsAssignable eq true and Node descof ${accountType.EntryTypeParentId}`;
  }

  // External Reference

  public showExternalReference_Manual(entry: Entry): boolean {
    const at = this.accountType(entry);
    return !!at ? !!at.ExternalReferenceLabel : false;
  }

  public labelExternalReference_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.ExternalReferenceLabel ?
      this.ws.getMultilingualValueImmediate(at, 'ExternalReferenceLabel') :
      this.translate.instant('Entry_ExternalReference');
  }

  // Additional Reference

  public showAdditionalReference_Manual(entry: Entry): boolean {
    const account = this.accountType(entry);
    return !!account ? !!account.AdditionalReferenceLabel : false;
  }

  public labelAdditionalReference_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.AdditionalReferenceLabel ?
      this.ws.getMultilingualValueImmediate(at, 'AdditionalReferenceLabel') :
      this.translate.instant('Entry_AdditionalReference');
  }

  // Noted Agent Name

  public showNotedAgentName_Manual(entry: Entry): boolean {
    const account = this.accountType(entry);
    return !!account ? !!account.NotedAgentNameLabel : false;
  }

  public labelNotedAgentName_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.NotedAgentNameLabel ?
      this.ws.getMultilingualValueImmediate(at, 'NotedAgentNameLabel') :
      this.translate.instant('Entry_NotedAgentName');
  }

  // Noted Amount

  public showNotedAmount_Manual(entry: Entry): boolean {
    const account = this.accountType(entry);
    return !!account ? !!account.NotedAmountLabel : false;
  }

  public labelNotedAmount_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.NotedAmountLabel ?
      this.ws.getMultilingualValueImmediate(at, 'NotedAmountLabel') :
      this.translate.instant('Entry_NotedAmount');
  }

  // Noted Date

  public showNotedDate_Manual(entry: Entry): boolean {
    const account = this.accountType(entry);
    return !!account ? !!account.NotedDateLabel : false;
  }

  public labelNotedDate_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.NotedDateLabel ?
      this.ws.getMultilingualValueImmediate(at, 'NotedDateLabel') :
      this.translate.instant('Entry_NotedDate');
  }

  public onFileSelected(input: HTMLInputElement, model: DocumentForSave) {
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
  private _requiredSignaturesForLineDefLineDefId: number;
  private _requiredSignaturesForLineDefLineIds: number[];

  public requiredSignaturesForLineDef(
    model: Document, lineDefId: number, extras: { [key: string]: any }): RequiredSignature[] {
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
    'ToState', 'RuleType', 'RoleId', 'ContractId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
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

  public onSignNo(lineDefId: number, signature: RequiredSignature): void {
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
      select: this.select,
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
            select: this.select
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
      return this.translate.instant('Error_OpenDocumentBeforeEdit');
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
      return this.translate.instant('Error_OpenDocumentBeforeEdit');
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
  private _visibleTabs: number[];
  private _invisibleTabs: number[];

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
      const visibleTabs: number[] = [];
      const invisibleTabs: number[] = [];
      const invisibleTracker: { [key: number]: true } = {};
      const definitionTracker: { [key: number]: true } = {};
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
      for (const lineDefId of Object.keys(linesTracker).map(e => +e)) {
        if (!definitionTracker[lineDefId]) {
          definitionTracker[lineDefId] = true;
          visibleTabs.push(lineDefId);
        }
      }

      this._visibleTabs = visibleTabs;
      this._invisibleTabs = invisibleTabs;
    }
  }

  public visibleTabs(model: Document): number[] {

    this.computeTabs(model);
    return this._visibleTabs;
  }

  public invisibleTabs(model: Document): number[] {

    this.computeTabs(model);
    return this._invisibleTabs;
  }

  public onOtherTab(lineDefId: number, model: Document): void {
    this._visibleTabs.push(lineDefId);
    this._visibleTabs = this._visibleTabs.slice();
    this._invisibleTabs = this._invisibleTabs.filter(e => e !== lineDefId);

    this.setActiveTab(lineDefId);
  }

  public manualLineTabTitle(model: DocumentForSave): string {
    if (this.isJV) {
      return this.translate.instant('Entries');
    } else {
      return this.tabTitle(this.ws.definitions.ManualLinesDefinitionId, model);
    }
  }

  public tabTitle(lineDefId: number, model: DocumentForSave): string {
    const def = this.lineDefinition(lineDefId);
    const isForm = this.showAsForm(lineDefId, model);
    return !!def ? this.ws.getMultilingualValueImmediate(def, isForm ? 'TitleSingular' : 'TitlePlural')
      : this.translate.instant('Undefined');
  }

  private _lines: { [defId: number]: LineForSave[] };
  private _linesModel: DocumentForSave;

  public lines(lineDefId: number, model: Document): Line[] {
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

  private _manualLineModel: Document;
  private _manualLineResult: LineForSave;

  public manualLine(model: Document): LineForSave {
    // Retrieves the one and only manual line in the document
    if (this._manualLineModel !== model) {
      this._manualLineModel = model;
      this._manualLineResult = model.Lines.find(e => this.isManualLine(e.DefinitionId));
    }

    return this._manualLineResult;
  }

  public showManualLineProps(model: Document): boolean {
    // Manual Line Memo and Posting Date fields are shown in non JV documents that have a manual line
    return !this.isJV && !!this.manualLine(model);
  }

  public onInsertManualEntry(pair: LineEntryPair, model: Document): void {
    // Called when the user inserts a new entry
    // Get the one and only manual line
    let manualLine = this.manualLine(model);
    if (!manualLine) {
      manualLine = {
        PostingDate: toLocalDateISOString(new Date()),
        DefinitionId: this.ws.definitions.ManualLinesDefinitionId,
        Entries: [],
        _flags: { isModified: true }
      };

      model.Lines.push(manualLine);

      this._manualLineModel = model;
      this._manualLineResult = manualLine;
    }

    // Add the entry to it
    manualLine.Entries.push(pair.entry);
    pair.line = manualLine;
  }

  public onDeleteManualEntry(pair: LineEntryPair, model: Document): void {

    this.cancelAutofill(pair); // Good tidying up

    const entryIndex = pair.line.Entries.indexOf(pair.entry);
    if (entryIndex > -1) {
      pair.line.Entries.splice(entryIndex, 1);
    }

    // If the line is empty, remove it
    if (pair.line.Entries.length === 0) {
      const lineIndex = model.Lines.indexOf(pair.line);
      model.Lines.splice(lineIndex);

      this._manualLineModel = model;
      this._manualLineResult = null;
    }
  }

  public onNewManualEntry = (pair: LineEntryPair) => {
    // Called when a new entry is created, including placeholder entry

    // Set the entry
    pair.entry = {
      Direction: 1
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
            line.Entries.forEach((entry, entryIndex) => {
              if (this.isManualLine(line.DefinitionId)) {
                this._manualEntries.push({ entry, line, entryIndex });
              } else if ((line.State || 0) >= 0) {
                this._smartEntries.push({ entry, line, entryIndex });
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

  public showLineErrors(lineDefId: number, model: Document) {
    return !!model && !!model.Lines &&
      model.Lines.some(line => this.isManualLine(lineDefId) && (!!line.serverErrors ||
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

    // if (!model.MemoIsCommon) {
    if (smart) {
      // This only appears in the smart bookkeeping grid
      paths.push('Memo');
    }

    if (smart) {
      paths.push('ModifiedWarning');
    }

    paths.push('Commands');

    return paths;
  }

  public smartColumnPaths(lineDefId: number, doc: Document, isForm: boolean): string[] {
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
              (doc.PostingDateIsCommon && col.ColumnName === 'PostingDate') ||
              (doc.DebitContractIsCommon && col.ColumnName === 'ContractId' && lineDef.Entries[col.EntryIndex].Direction === 1) ||
              (doc.CreditContractIsCommon && col.ColumnName === 'ContractId' && lineDef.Entries[col.EntryIndex].Direction === -1) ||
              (doc.NotedContractIsCommon && col.ColumnName === 'NotedContractId') ||
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

  private _columnTemplatesLineDefId: number;
  private _columnTemplatesDef: DocumentDefinitionForClient;
  private _columnTemplatesResult: ColumnTemplates;

  public columnTemplates(
    lineDefId: number,
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

  private lineDefinition(lineDefId: number): LineDefinitionForClient {
    return !!lineDefId ? this.ws.definitions.Lines[lineDefId] : null;
  }

  private columnDefinition(lineDefId: number, columnIndex: number): LineDefinitionColumnForClient {
    const lineDef = this.lineDefinition(lineDefId);
    return !!lineDef ? lineDef.Columns[columnIndex] : null;
  }

  private entryDefinition(lineDefId: number, columnIndex: number): LineDefinitionEntryForClient {
    const lineDef = this.lineDefinition(lineDefId);
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return !!lineDef && !!lineDef.Entries && !!colDef ? lineDef.Entries[colDef.EntryIndex] : null;
  }

  public columnName(lineDefId: number, columnIndex: number): string {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return !!colDef ? colDef.ColumnName : null;
  }

  public columnLabel(lineDefId: number, columnIndex: number): string {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return this.ws.getMultilingualValueImmediate(colDef, 'Label');
  }

  /**
   * Returns either the line or one of its entries depending on the column definition
   */
  private entity(def: LineDefinitionColumnForClient, line: LineForSave): LineForSave | EntryForSave {
    let entity: LineForSave | EntryForSave;
    if (!!def) {
      if (def.ColumnName === 'Memo' || def.ColumnName === 'PostingDate') {
        entity = line;
      } else {
        entity = !!line.Entries ? line.Entries[def.EntryIndex] : null;
      }
    }

    return entity;
  }

  public entry(lineDefId: number, columnIndex: number, line: LineForSave): EntryForSave {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    if (!!colDef && colDef.ColumnName !== 'Memo' && colDef.ColumnName !== 'PostingDate') {
      return !!line.Entries ? line.Entries[colDef.EntryIndex] : null;
    }

    return null;
  }

  public definitionIdsContract_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.ContractDefinitionIds ? entryDef.ContractDefinitionIds : [];
  }

  public definitionIdsNotedContract_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.NotedContractDefinitionIds ? entryDef.NotedContractDefinitionIds : [];
  }

  public definitionIdsResource_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.ResourceDefinitionIds ? entryDef.ResourceDefinitionIds : [];
  }

  public entryTypeFilter(lineDefId: number, columnIndex: number): string {
    // Filter for smart line
    // TODO: What about EntryTypeId ??
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.EntryTypeParentId ? `Node descof ${entryDef.EntryTypeParentId}` : null;
  }

  public serverErrors(lineDefId: number, columnIndex: number, line: LineForSave): string[] {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity && !!entity.serverErrors ? entity.serverErrors[colDef.ColumnName] : null;
  }

  public entityMetadata(lineDefId: number, columnIndex: number, line: LineForSave): 0 | 1 | 2 {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity && !!entity.EntityMetadata ? entity.EntityMetadata[colDef.ColumnName] : null;
  }

  public getFieldValue(lineDefId: number, columnIndex: number, line: LineForSave): any {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity ? entity[colDef.ColumnName] : null;
  }

  public setFieldValue(lineDefId: number, columnIndex: number, line: LineForSave, value: any): void {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    if (!!entity) {
      entity[colDef.ColumnName] = value;
    }
  }

  public isReadOnly(lineDefId: number, columnIndex: number, line: Line) {
    // return false;
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const state = line.State || 0;
    return state < 0 || state >= colDef.ReadOnlyState;
  }

  public isRequired(lineDefId: number, columnIndex: number, line: Line) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return (line.State || 0) >= colDef.RequiredState;
  }

  private _onNewLineFactoryLineDefId: number;
  private _onNewLineFactoryResult: (item: LineForSave) => LineForSave;

  public onNewSmartLineFactory(lineDefId: number): (item: LineForSave) => LineForSave {
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

  public columnLabelAlignment(lineDefId: number, columnIndex: number): string {
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

  public showAsForm(lineDefId: number, model: DocumentForSave) {
    const count = this.lines(lineDefId, model).length;
    if (count > 1) {
      return false;
    } else {
      const lineDef = this.lineDefinition(lineDefId);
      return !!lineDef ? lineDef.ViewDefaultsToForm : false;
    }
  }

  public dummyUpdate = () => { };

  public onCreateForm(lineDefId: number, model: DocumentForSave) {
    if (!model) {
      return;
    }

    let newLine: LineForSave = {};
    newLine = this.onNewSmartLineFactory(lineDefId)(newLine);
    this.lines(lineDefId, model).push(newLine);
    this.onInsertSmartLine(newLine, model);
  }

  public onDeleteForm(lineDefId: number, model: DocumentForSave) {
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
    } else { // Closed + Canceled
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
    } else { // Closed
      return true;
    }
  }

  public isPositiveState(model: Document): boolean {
    const states: LineState[] = [0, 1, 2, 3, 4];
    const documentStates: DocumentState[] = [0, 1];

    return states.some(state => this.isStateActive(state, model)) ||
      documentStates.some(state => this.isDocumentStateActive(state, model));
  }

  ////////////// State

  public onDocumentState(
    doc: Document,
    fn: (ids: (number | string)[], args: ActionArguments, extras?: { [key: string]: any }) => Observable<EntitiesResponse<Document>>) {
    fn([doc.Id], {
      returnEntities: true,
      select: this.select,
    }, { includeRequiredSignatures: true }).pipe(
      tap(res => {
        addToWorkspace(res, this.workspace);
        this.details.state.extras = res.Extras;
        this.handleFreshExtras(res.Extras);
      })
    ).subscribe({ error: this.details.handleActionError });
  }

  public onClose(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.close);
  }

  public onOpen(doc: Document): void {
    this.onDocumentState(doc, this.documentsApi.open);
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

  // Close

  public showClose(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !doc.State;
  }

  public disableClose(doc: Document, requiredSignatures: RequiredSignature[]): boolean {

    return !doc || !doc.Id || // Missing document
      // Missing posting date
      // !doc.PostingDate ||
      // OR missing permissions
      !this.hasPermissionToUpdateState(doc) ||
      // OR missing signatures
      this.missingSignatures(doc, requiredSignatures);
  }

  public closeTooltip(doc: Document, requiredSignatures: RequiredSignature[]): string {

    if (!this.hasPermissionToUpdateState(doc)) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    } else if (this.missingSignatures(doc, requiredSignatures)) {
      return this.translate.instant('Error_LineIsMissingSignatures');
    }
    // else if (!!doc && !doc.PostingDate) {
    //   return this.translate.instant('Error_ThePostingDateIsRequiredForClosing');
    // }

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

  // Open & Uncancel

  public showOpen(doc: Document, _: RequiredSignature[]): boolean {
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
      .some(e => this.isManualLine(e.LineDefinitionId));
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
        // setUndefined = add an empty flags object if non exist
        result = (line._flags = {});
      }

      return result;
    }
  }

  public getIsModified(line: LineForSave, doc: DocumentForSave): boolean {
    // if (!line) {
    //   return false;
    // }
    const flags = this.flags(line, doc);
    return !!flags ? flags.isModified : false;
  }

  public setModified(line: LineForSave, doc: DocumentForSave): void {
    // if (!line) {
    //   return;
    // }
    this.flags(line, doc, true).isModified = true;
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

  public highlightSmartTab(lineDefId: number, doc: DocumentForSave): boolean {
    const isHighlightedLine = this.highlightLineFactory(doc);
    return this.lines(lineDefId, doc).some(isHighlightedLine);
  }

  public smartTabColor(lineDefId: number, doc: DocumentForSave): string {
    return this.highlightSmartTab(lineDefId, doc) ? '#eeff44' : null;
  }
  public highlightBookkeepingTab(doc: Document): boolean {
    const isHighlightedLine = this.highlightLineFactory(doc);
    return !!doc && !!doc.Lines && doc.Lines.some(e => (e.State || 0) >= 0 && isHighlightedLine(e));
  }

  public bookkeepingTabColor(doc: DocumentForSave): string {
    return this.highlightBookkeepingTab(doc) ? '#eeff44' : null;
  }

  public formColor(lineDefId: number, doc: DocumentForSave): string {
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

  public get showPrint(): boolean {
    return this.printingTemplates.length > 0;
  }

  private _printingTemplatesSettings: SettingsForClient;
  private _printingTemplatesDefinitions: DefinitionsForClient;
  private _printingTemplatesResult: PrintingTemplate[];

  public get printingTemplates(): PrintingTemplate[] {
    const ws = this.ws;
    if (this._printingTemplatesDefinitions !== ws.definitions ||
      this._printingTemplatesSettings !== ws.settings) {
      this._printingTemplatesDefinitions = ws.definitions;
      this._printingTemplatesSettings = ws.settings;
      const result: PrintingTemplate[] = [];

      const settings = ws.settings;
      const def = this.definition;
      for (const template of def.MarkupTemplates.filter(e => e.Usage === 'QueryById')) {
        const langCount = (template.SupportsPrimaryLanguage ? 1 : 0)
          + (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId ? 1 : 0)
          + (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId ? 1 : 0);

        if (template.SupportsPrimaryLanguage) {
          const postfix = langCount > 1 ? ` (${settings.PrimaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.PrimaryLanguageId
          });
        }

        if (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.SecondaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.SecondaryLanguageId
          });
        }

        if (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.TernaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.TernaryLanguageId
          });
        }
      }

      this._printingTemplatesResult = result;
    }

    return this._printingTemplatesResult;
  }

  private printingSubscription: Subscription;

  public onPrint(doc: Document, template: PrintingTemplate): void {
    if (!doc || !doc.Id || !template) {
      return;
    }

    // Cancel any existing printing query
    if (!!this.printingSubscription) {
      this.printingSubscription.unsubscribe();
    }

    // New printing query
    this.printingSubscription = this.documentsApi
      .printById(doc.Id, template.templateId, { culture: template.culture })
      .pipe(
        tap(blob => {
          this.printingSubscription = null;
          printBlob(blob);
        }),
        catchError(friendlyError => {
          this.printingSubscription = null;
          this.details.displayModalError(friendlyError.error);
          return of();
        }),
        finalize(() => {
          this.printingSubscription = null;
        })
      ).subscribe();
  }

  public get isPrinting(): boolean {
    return !!this.printingSubscription;
  }
}

export interface PrintingTemplate {
  name: () => string;
  templateId: number;
  culture: string;
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
  [Closed] State === 1
  [Canceled] State === -1

  --------- IsVisible (In +ve state and wide screen)
  [0] !!HasWorkflow
  [1] (!!HasWorkflow && CanReachState1) || isActive(1)
  [2] (!!HasWorkflow && CanReachState2) || isActive(2)
  [3] (!!HasWorkflow && CanReachState3) || isActive(3)
  [4] !!HasWorkflow || isActive(4)
  [Current] !HasWorkflow
  [Closed] Always

  --------- IsVisible (In -ve state or narrow screen)
  IsVisible = IsActive

*/
