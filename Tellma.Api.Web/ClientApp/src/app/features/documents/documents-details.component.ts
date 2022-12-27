// tslint:disable:member-ordering
import { Component, Input, TemplateRef, ViewChild, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService, TenantWorkspace, MasterDetailsStore, DetailsStatus } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { DocumentForSave, Document, formatSerial, DocumentClearance, metadata_Document, DocumentState } from '~/app/data/entities/document';
import {
  DocumentDefinitionForClient, LineDefinitionColumnForClient, LineDefinitionEntryForClient,
  LineDefinitionForClient, LineDefinitionGenerateParameterForClient, EntryColumnName,
  DefinitionsForClient, ResourceDefinitionForClient, MessageTemplateForClient, EmailTemplateForClient
} from '~/app/data/dto/definitions-for-client';
import { LineForSave, Line, LineState, LineFlags } from '~/app/data/entities/line';
import { Entry, EntryForSave } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import {
  addToWorkspace, openOrDownloadBlob,
  fileSizeDisplay, mergeEntitiesInWorkspace,
  FriendlyError,
  isSpecified, colorFromExtension, iconFromExtension,
  onFileSelected, descFromControlOptions, updateOn, downloadBlob, addSingleToWorkspace
} from '~/app/data/util';
import { toLocalDateOnlyISOString, todayISOString } from '~/app/data/date-util';
import { tap, catchError, finalize, skip, takeUntil } from 'rxjs/operators';
import { NgbModal, Placement, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { of, Observable, Subscription, timer } from 'rxjs';
import { Account, metadata_Account } from '~/app/data/entities/account';
import { Resource, metadata_Resource } from '~/app/data/entities/resource';
import { Currency } from '~/app/data/entities/currency';
import { metadata_Agent, Agent } from '~/app/data/entities/agent';
import { AccountType } from '~/app/data/entities/account-type';
import { Attachment, AttachmentForSave } from '~/app/data/entities/attachment';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';
import { RequiredSignature } from '~/app/data/entities/required-signature';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ActionArguments } from '~/app/data/dto/action-arguments';
import { EntitiesResponse } from '~/app/data/dto/entities-response';
import {
  getChoices,
  ChoicePropDescriptor,
  EntityDescriptor,
  isText,
  PropVisualDescriptor,
  Collection
} from '~/app/data/entities/base/metadata';
import { DocumentStateChange } from '~/app/data/entities/document-state-change';
import { DocumentLineDefinitionEntryForSave, DocumentLineDefinitionEntry } from '~/app/data/entities/document-line-definition-entry';
import { GetArguments } from '~/app/data/dto/get-arguments';
import { AudioService } from '~/app/data/audio.service';
import { dateFormat, datetimeFormat, timeFormat } from '~/app/shared/date-format/date-time-format';
import { UpdateAssignmentArguments } from '~/app/data/dto/update-assignment-arguments';
import { EmailCommandPreview, EmailCommandVersions, EmailPreview } from '~/app/data/dto/email-command-preview';
import { MessageCommandPreview } from '~/app/data/dto/message-command-preview';
import { IdResult } from '~/app/data/dto/id-result';

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
    rowTemplate?: TemplateRef<any>,
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
  id: string | number;
  modifiedTime: string;

  // For editing
  isEdit?: boolean;
  commentForEdit?: string;
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

  @Input()
  previewDefinition: DocumentDefinitionForClient; // Used in preview mode

  @ViewChild('confirmModal', { static: true })
  confirmModal: TemplateRef<any>;

  @ViewChild('signatureModal', { static: true })
  signatureModal: TemplateRef<any>;

  @ViewChild('autoGenerateModal', { static: true })
  autoGenerateModal: TemplateRef<any>;

  public confirmationMessage: string;

  public selectBase = '$Details'; // The server understands this keyword, no need to list all hundreds of select paths
  public additionalSelectAccount = '$DocumentDetails';
  public additionalSelectAgent = '$DocumentDetails';
  public additionalSelectResource = '$DocumentDetails';
  public additionalSelectNotedAgent = '$DocumentDetails';
  public additionalSelectNotedResource = '$DocumentDetails';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute, private modalService: NgbModal, private audio: AudioService) {
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
    if ((this.showAttachmentsTab && s.tab === -20) || (this.showBookkeepingTab && s.tab === -10)) {
      return s.tab;
    }

    // Make sure the selected tab is a visible one
    const visibleTabs = this.visibleTabs(model);
    if (visibleTabs.some(e => e === s.tab)) {
      return s.tab;
    } else {
      // Get the first visible tab
      return visibleTabs[0] || (this.showBookkeepingTab ? -10 : null) || (this.showAttachmentsTab ? -20 : null);
    }
  }

  public get showBookkeepingTab() {
    return this.definition.HasBookkeeping;
  }

  public get showAttachmentsTab() {
    return !!this.definition.AttachmentVisibility;
  }

  /**
   * Built-in hardcoded definition
   */
  public get isJV(): boolean {
    return this.definition.Code === 'ManualJournalVoucher';
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
    return this.previewDefinition || (!!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null);
  }

  public get found(): boolean {
    return !!this.definition;
  }

  create = () => {
    const result: DocumentForSave = {
      // PostingDate: toLocalDateISOString(new Date()),
      Clearance: 0,
      LineDefinitionEntries: [],
      Lines: [],
      Attachments: []
    };

    if (this.isJV) {

      // Posting Date
      result.PostingDate = todayISOString();

      // Is Common
      result.PostingDateIsCommon = true;
      result.MemoIsCommon = true;
      result.CurrencyIsCommon = false;
      result.CenterIsCommon = false;

      result.AgentIsCommon = false;
      result.ResourceIsCommon = false;
      result.NotedAgentIsCommon = false;
      result.NotedResourceIsCommon = false;

      result.QuantityIsCommon = false;
      result.UnitIsCommon = false;
      result.Time1IsCommon = false;
      result.DurationIsCommon = false;
      result.DurationUnitIsCommon = false;
      result.Time2IsCommon = false;
      result.NotedDateIsCommon = false;

      result.ExternalReferenceIsCommon = false;
      result.ReferenceSourceIsCommon = false;
      result.InternalReferenceIsCommon = false;
    } else {
      const def = this.definition;

      // Posting Date
      result.PostingDate = todayISOString();

      // Is Common
      result.PostingDateIsCommon = true;
      result.MemoIsCommon = true;
      result.CurrencyIsCommon = true;
      result.CenterIsCommon = true;

      result.AgentIsCommon = true;
      result.ResourceIsCommon = true;
      result.NotedAgentIsCommon = true;
      result.NotedResourceIsCommon = true;

      result.QuantityIsCommon = true;
      result.UnitIsCommon = true;
      result.Time1IsCommon = true;
      result.DurationIsCommon = true;
      result.DurationUnitIsCommon = true;
      result.Time2IsCommon = true;
      result.NotedDateIsCommon = true;

      result.ExternalReferenceIsCommon = true;
      result.ReferenceSourceIsCommon = true;
      result.InternalReferenceIsCommon = true;
    }

    return result;
  }

  clone: (item: Document, removeIds?: boolean) => Document = (item: Document, removeIds = true) => {
    if (!!item) {
      // Cost a lot to duplicate in edit mode, so we use this trick
      const temp = item.Attachments;
      item.Attachments = [];
      const clone = JSON.parse(JSON.stringify(item)) as Document;
      item.Attachments = temp;

      // Standard
      if (removeIds) {
        delete clone.Id;
      }
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

      if (!!clone.LineDefinitionEntries) {
        clone.LineDefinitionEntries.forEach(tabEntry => {
          // Standard
          if (removeIds) {
            delete tabEntry.Id;
          }
          delete tabEntry.EntityMetadata;
          delete tabEntry.serverErrors;

          // Non savable
          delete tabEntry.DocumentId;
          delete tabEntry.CreatedAt;
          delete tabEntry.CreatedById;
          delete tabEntry.ModifiedAt;
          delete tabEntry.ModifiedById;
        });
      }

      if (!!clone.Lines) {
        clone.Lines.forEach(line => {
          // Standard
          if (removeIds) {
            delete line.Id;
          }
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
            line.Entries.forEach(e => this.processEntryClone(e, removeIds));
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

  private processEntryClone(clone: Entry, removeIds = true): Entry {

    // Standard
    if (removeIds) {
      delete clone.Id;
    }
    delete clone.EntityMetadata;
    delete clone.serverErrors;

    // Non savable
    delete clone.LineId;
    delete clone.CreatedAt;
    delete clone.CreatedById;
    delete clone.ModifiedAt;
    delete clone.ModifiedById;

    return clone;
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
          modifiedTime: e.ModifiedAt,
          userId: e.CreatedById,
          assigneeId: e.AssigneeId,
          comment: e.Comment,
          id: e.Id
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
        const date = toLocalDateOnlyISOString(new Date(entry.time));
        if (!result[date]) {
          result[date] = [];
        }

        result[date].push(entry);
      }

      this._sortChronologicallyResult = Object.keys(result).map(date => ({ date, events: result[date] }));
    }

    return this._sortChronologicallyResult;
  }

  public modifiedTime = (event: DocumentReassignmentEvent): string => {
    const modifiedDate = dateFormat(event.modifiedTime, this.workspace, this.translate);
    const date = dateFormat(event.time, this.workspace, this.translate);

    // If the date is the same, only show the time, else show both date and time
    let result: string;
    if (modifiedDate === date) {
      result = timeFormat(event.modifiedTime, this.workspace, this.translate);
    } else {
      result = datetimeFormat(event.modifiedTime, this.workspace, this.translate);
    }

    return result;
  }

  public canEditAssignment = (event: DocumentReassignmentEvent, isEdit: boolean): boolean => {
    return event.userId === this.ws.userSettings.UserId && !isEdit;
  }

  public onEditAssignment(event: DocumentReassignmentEvent): void {
    event.isEdit = true;
    event.commentForEdit = event.comment;
  }

  public onCancelEditAssignment(event: DocumentReassignmentEvent): void {
    event.isEdit = false;
    delete event.commentForEdit;
  }

  public onDoEditAssignment(event: DocumentReassignmentEvent): void {
    const args: UpdateAssignmentArguments = {
      returnEntities: true,
      select: this.select,
      id: event.id,
      comment: event.commentForEdit
    };

    this.documentsApi.updateAssignment(args).pipe(
      tap(res => {
        addSingleToWorkspace(res, this.workspace);
        this.details.state.extras = res.Extras;
        this.handleFreshExtras(res.Extras);
      }),
    ).subscribe({ error: this.details.handleActionError });
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

  public showClearance(_: DocumentForSave): boolean {
    return !!this.definition.ClearanceVisibility;
  }

  public clearanceDisplay(clearance: DocumentClearance) {
    if (clearance === null || clearance === undefined) {
      return '';
    }

    const desc = metadata_Document(this.workspace, this.translate, null).properties.Clearance as ChoicePropDescriptor;
    return desc.format(clearance);
  }

  public get clearanceChoices(): SelectorChoice[] {
    const desc = metadata_Document(this.workspace, this.translate, null).properties.Clearance as ChoicePropDescriptor;
    return getChoices(desc);
  }

  // Posting Date

  public showDocumentPostingDate(_: DocumentForSave): boolean {
    return !!this.definition.PostingDateVisibility;
  }

  public showDocumentPostingDateIsCommon(_: Document): boolean {
    return this.definition.PostingDateIsCommonVisibility;
  }

  public requireDocumentPostingDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);

    return this.definition.PostingDateVisibility === 'Required' || this._requireDocumentPostingDate;
  }

  public readonlyDocumentPostingDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);

    return this._readonlyDocumentPostingDate && !this.isJV; // JV Posting Date is never readonly
  }

  public labelDocumentPostingDate(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'PostingDateLabel') ||
      this.translate.instant('Document_PostingDate');
  }

  // Center (Business Unit)

  public showDocumentCenter(_: DocumentForSave): boolean {
    return !!this.definition.CenterVisibility && !this.ws.settings.SingleBusinessUnitId;
  }

  public showDocumentCenterIsCommon(_: Document): boolean {
    return this.definition.CenterIsCommonVisibility;
  }

  public requireDocumentCenter(doc: Document): boolean {
    this.computeDocumentSettings(doc);

    return this.definition.CenterVisibility === 'Required' || this._requireDocumentCenter;
  }

  public readonlyDocumentCenter(doc: Document): boolean {
    this.computeDocumentSettings(doc);

    return this._readonlyDocumentCenter && !this.isJV;
  }

  public labelDocumentCenter(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'CenterLabel') ||
      this.translate.instant('Document_Center');
  }

  public filterDocumentCenter(_: DocumentForSave): string {
    const s = this.ws.settings;
    let prefix: string;
    if (s.FeatureFlags && s.FeatureFlags.BusinessUnitGoneWithTheWind) {
      prefix = `IsLeaf eq true`;
    } else {
      prefix = `CenterType eq 'BusinessUnit'`;
    }

    const def = this.definition;
    if (def.CenterFilter) {
      return `${prefix} or (${def.CenterFilter})`;
    } else {
      return prefix;
    }
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

    return this._readonlyDocumentMemo && !this.isJV;
  }

  public labelDocumentMemo(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'MemoLabel') || this.translate.instant('Memo');
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
      this.translate.instant('Entry_Currency');
  }

  public filterDocumentCurrency(_: DocumentForSave): string {
    return this.definition.CurrencyFilter;
  }

  // Agent

  public showDocumentAgent(_: DocumentForSave): boolean {
    return this.definition.AgentVisibility;
  }

  public requireDocumentAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentAgent;
  }

  public readonlyDocumentAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentAgent;
  }

  public labelDocumentAgent(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'AgentLabel');
    if (!!label) {
      return label;
    }

    // Second try the agent definition
    if (this.definition.AgentDefinitionIds.length === 1) {
      const agentDefId = this.definition.AgentDefinitionIds[0];
      const agentDef = this.ws.definitions.Agents[agentDefId];
      if (!!agentDef) {
        label = this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular');
      }
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_Agent');
    }

    return label;
  }

  public documentAgentDefinitionIds(_: DocumentForSave): number[] {
    return this.definition.AgentDefinitionIds;
  }

  public filterDocumentAgent(_: DocumentForSave): string {
    return this.definition.AgentFilter;
  }

  // Resource

  public showDocumentResource(_: DocumentForSave): boolean {
    return this.definition.ResourceVisibility;
  }

  public requireDocumentResource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentResource;
  }

  public readonlyDocumentResource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentResource;
  }

  public labelDocumentResource(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'ResourceLabel');
    if (!!label) {
      return label;
    }

    // Second try the resource definition
    if (this.definition.ResourceDefinitionIds.length === 1) {
      const resourceDefId = this.definition.ResourceDefinitionIds[0];
      const resourceDef = this.ws.definitions.Resources[resourceDefId];
      if (!!resourceDef) {
        label = this.ws.getMultilingualValueImmediate(resourceDef, 'TitleSingular');
      }
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_Resource');
    }

    return label;
  }

  public documentResourceDefinitionIds(_: DocumentForSave): number[] {
    return this.definition.ResourceDefinitionIds;
  }

  public filterDocumentResource(_: DocumentForSave): string {
    return this.definition.ResourceFilter;
  }

  // NotedAgent

  public showDocumentNotedAgent(_: DocumentForSave): boolean {
    return this.definition.NotedAgentVisibility;
  }

  public requireDocumentNotedAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentNotedAgent;
  }

  public readonlyDocumentNotedAgent(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentNotedAgent;
  }

  public labelDocumentNotedAgent(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'NotedAgentLabel');
    if (!!label) {
      return label;
    }

    // Second try the notedagent definition
    if (this.definition.NotedAgentDefinitionIds.length === 1) {
      const notedagentDefId = this.definition.NotedAgentDefinitionIds[0];
      const notedagentDef = this.ws.definitions.Agents[notedagentDefId];
      if (!!notedagentDef) {
        label = this.ws.getMultilingualValueImmediate(notedagentDef, 'TitleSingular');
      }
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_NotedAgent');
    }

    return label;
  }

  public documentNotedAgentDefinitionIds(_: DocumentForSave): number[] {
    return this.definition.NotedAgentDefinitionIds;
  }

  public filterDocumentNotedAgent(_: DocumentForSave): string {
    return this.definition.NotedAgentFilter;
  }

  // NotedResource

  public showDocumentNotedResource(_: DocumentForSave): boolean {
    return this.definition.NotedResourceVisibility;
  }

  public requireDocumentNotedResource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentNotedResource;
  }

  public readonlyDocumentNotedResource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentNotedResource;
  }

  public labelDocumentNotedResource(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'NotedResourceLabel');
    if (!!label) {
      return label;
    }

    // Second try the notedresource definition
    if (this.definition.NotedResourceDefinitionIds.length === 1) {
      const notedresourceDefId = this.definition.NotedResourceDefinitionIds[0];
      const notedresourceDef = this.ws.definitions.Resources[notedresourceDefId];
      if (!!notedresourceDef) {
        label = this.ws.getMultilingualValueImmediate(notedresourceDef, 'TitleSingular');
      }
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_NotedResource');
    }

    return label;
  }

  public documentNotedResourceDefinitionIds(_: DocumentForSave): number[] {
    return this.definition.NotedResourceDefinitionIds;
  }

  public filterDocumentNotedResource(_: DocumentForSave): string {
    return this.definition.NotedResourceFilter;
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
      this.translate.instant('Entry_Quantity');
  }

  // Unit

  public showDocumentUnit(_: DocumentForSave): boolean {
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

  public labelDocumentUnit(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'UnitLabel');
    if (!!label) {
      return label;
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_Unit');
    }

    return label;
  }

  public filterDocumentUnit(_: DocumentForSave): string {
    return this.definition.UnitFilter;
  }

  // Time1

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
      this.translate.instant('Entry_Time1');
  }

  // Duration

  public showDocumentDuration(_: DocumentForSave) {
    return this.definition.DurationVisibility;
  }

  public requireDocumentDuration(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentDuration;
  }

  public readonlyDocumentDuration(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentDuration;
  }

  public labelDocumentDuration(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'DurationLabel') ||
      this.translate.instant('Entry_Duration');
  }

  // DurationUnit

  public showDocumentDurationUnit(_: DocumentForSave): boolean {
    return this.definition.DurationUnitVisibility;
  }

  public requireDocumentDurationUnit(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentDurationUnit;
  }

  public readonlyDocumentDurationUnit(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentDurationUnit;
  }

  public labelDocumentDurationUnit(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'DurationUnitLabel');
    if (!!label) {
      return label;
    }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_DurationUnit');
    }

    return label;
  }

  public filterDocumentDurationUnit(_: DocumentForSave): string {
    return this.definition.DurationUnitFilter;
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
      this.translate.instant('Entry_Time2');
  }

  // NotedDate

  public showDocumentNotedDate(_: DocumentForSave) {
    return this.definition.NotedDateVisibility;
  }

  public requireDocumentNotedDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentNotedDate;
  }

  public readonlyDocumentNotedDate(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentNotedDate;
  }

  public labelDocumentNotedDate(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'NotedDateLabel') ||
      this.translate.instant('Entry_NotedDate');
  }

  // External Reference

  public showDocumentExternalReference(_: DocumentForSave) {
    return this.definition.ExternalReferenceVisibility;
  }

  public requireDocumentExternalReference(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentExternalReference;
  }

  public readonlyDocumentExternalReference(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentExternalReference;
  }

  public labelDocumentExternalReference(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'ExternalReferenceLabel') ||
      this.translate.instant('Entry_ExternalReference');
  }

  // ReferenceSource

  public showDocumentReferenceSource(_: DocumentForSave): boolean {
    return this.definition.ReferenceSourceVisibility;
  }

  public requireDocumentReferenceSource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentReferenceSource;
  }

  public readonlyDocumentReferenceSource(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentReferenceSource;
  }

  public labelDocumentReferenceSource(_: DocumentForSave): string {
    // First try the document definition
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'ReferenceSourceLabel');
    if (!!label) {
      return label;
    }

    // // Second try the referencesource definition
    // if (this.definition.ReferenceSourceDefinitionIds.length === 1) {
    //   const referencesourceDefId = this.definition.ReferenceSourceDefinitionIds[0];
    //   const referencesourceDef = this.ws.definitions.Agents[referencesourceDefId];
    //   if (!!referencesourceDef) {
    //     label = this.ws.getMultilingualValueImmediate(referencesourceDef, 'TitleSingular');
    //   }
    // }

    // Last resort: generic label
    if (!label) {
      label = this.translate.instant('Entry_ReferenceSource');
    }

    return label;
  }

  public documentReferenceSourceDefinitionIds(_: DocumentForSave): number[] {
    return this.ws.definitions.ReferenceSourceDefinitionIds;
  }

  public filterDocumentReferenceSource(_: DocumentForSave): string {
    return this.definition.ReferenceSourceFilter;
  }

  // Internal Reference

  public showDocumentInternalReference(_: DocumentForSave) {
    return this.definition.InternalReferenceVisibility;
  }

  public requireDocumentInternalReference(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._requireDocumentInternalReference;
  }

  public readonlyDocumentInternalReference(doc: Document): boolean {
    this.computeDocumentSettings(doc);
    return this._readonlyDocumentInternalReference;
  }

  public labelDocumentInternalReference(_: Document): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'InternalReferenceLabel') ||
      this.translate.instant('Entry_InternalReference');
  }

  private _computeDocumentSettingsDoc: Document;
  private _computeDocumentSettingsDef: DocumentDefinitionForClient;

  private _requireDocumentPostingDate: boolean;
  private _readonlyDocumentPostingDate: boolean;
  private _requireDocumentMemo: boolean;
  private _readonlyDocumentMemo: boolean;

  private _requireDocumentCurrency: boolean;
  private _readonlyDocumentCurrency: boolean;
  private _requireDocumentCenter: boolean;
  private _readonlyDocumentCenter: boolean;

  private _requireDocumentAgent: boolean;
  private _readonlyDocumentAgent: boolean;
  private _requireDocumentResource: boolean;
  private _readonlyDocumentResource: boolean;
  private _requireDocumentNotedAgent: boolean;
  private _readonlyDocumentNotedAgent: boolean;
  private _requireDocumentNotedResource: boolean;
  private _readonlyDocumentNotedResource: boolean;

  private _requireDocumentQuantity: boolean;
  private _readonlyDocumentQuantity: boolean;
  private _requireDocumentUnit: boolean;
  private _readonlyDocumentUnit: boolean;
  private _requireDocumentTime1: boolean;
  private _readonlyDocumentTime1: boolean;
  private _requireDocumentDuration: boolean;
  private _readonlyDocumentDuration: boolean;
  private _requireDocumentDurationUnit: boolean;
  private _readonlyDocumentDurationUnit: boolean;
  private _requireDocumentTime2: boolean;
  private _readonlyDocumentTime2: boolean;
  private _requireDocumentNotedDate: boolean;
  private _readonlyDocumentNotedDate: boolean;

  private _requireDocumentExternalReference: boolean;
  private _readonlyDocumentExternalReference: boolean;
  private _requireDocumentReferenceSource: boolean;
  private _readonlyDocumentReferenceSource: boolean;
  private _requireDocumentInternalReference: boolean;
  private _readonlyDocumentInternalReference: boolean;

  private computeDocumentSettings(doc: Document): void {
    if (!doc || !doc.Lines) {
      this._requireDocumentPostingDate = false;
      this._readonlyDocumentPostingDate = false;
      this._requireDocumentMemo = false;
      this._readonlyDocumentMemo = false;

      this._requireDocumentCurrency = false;
      this._readonlyDocumentCurrency = false;
      this._requireDocumentCenter = false;
      this._readonlyDocumentCenter = false;

      this._requireDocumentAgent = false;
      this._readonlyDocumentAgent = false;
      this._requireDocumentResource = false;
      this._readonlyDocumentResource = false;
      this._requireDocumentNotedAgent = false;
      this._readonlyDocumentNotedAgent = false;
      this._requireDocumentNotedResource = false;
      this._readonlyDocumentNotedResource = false;

      this._requireDocumentQuantity = false;
      this._readonlyDocumentQuantity = false;
      this._requireDocumentUnit = false;
      this._readonlyDocumentUnit = false;
      this._requireDocumentTime1 = false;
      this._readonlyDocumentTime1 = false;
      this._requireDocumentDuration = false;
      this._readonlyDocumentDuration = false;
      this._requireDocumentDurationUnit = false;
      this._readonlyDocumentDurationUnit = false;
      this._requireDocumentTime2 = false;
      this._readonlyDocumentTime2 = false;
      this._requireDocumentNotedDate = false;
      this._readonlyDocumentNotedDate = false;

      this._requireDocumentExternalReference = false;
      this._readonlyDocumentExternalReference = false;
      this._requireDocumentReferenceSource = false;
      this._readonlyDocumentReferenceSource = false;
      this._requireDocumentInternalReference = false;
      this._readonlyDocumentInternalReference = false;

      return;
    }

    const def = this.definition;
    if (this._computeDocumentSettingsDoc !== doc ||
      this._computeDocumentSettingsDef !== def) {
      this._computeDocumentSettingsDoc = doc;
      this._computeDocumentSettingsDef = def;

      this._requireDocumentPostingDate = def.PostingDateRequiredState === 0;
      this._readonlyDocumentPostingDate = def.PostingDateReadOnlyState === 0;
      this._requireDocumentMemo = def.MemoRequiredState === 0;
      this._readonlyDocumentMemo = def.MemoReadOnlyState === 0;

      this._requireDocumentCurrency = def.CurrencyRequiredState === 0;
      this._readonlyDocumentCurrency = def.CurrencyReadOnlyState === 0;
      this._requireDocumentCenter = def.CenterRequiredState === 0;
      this._readonlyDocumentCenter = def.CenterReadOnlyState === 0;

      this._requireDocumentAgent = def.AgentRequiredState === 0;
      this._readonlyDocumentAgent = def.AgentReadOnlyState === 0;
      this._requireDocumentResource = def.ResourceRequiredState === 0;
      this._readonlyDocumentResource = def.ResourceReadOnlyState === 0;
      this._requireDocumentNotedAgent = def.NotedAgentRequiredState === 0;
      this._readonlyDocumentNotedAgent = def.NotedAgentReadOnlyState === 0;
      this._requireDocumentNotedResource = def.NotedResourceRequiredState === 0;
      this._readonlyDocumentNotedResource = def.NotedResourceReadOnlyState === 0;

      this._requireDocumentQuantity = def.QuantityRequiredState === 0;
      this._readonlyDocumentQuantity = def.QuantityRequiredState === 0;
      this._requireDocumentUnit = def.UnitRequiredState === 0;
      this._readonlyDocumentUnit = def.UnitRequiredState === 0;
      this._requireDocumentTime1 = def.Time1RequiredState === 0;
      this._readonlyDocumentTime1 = def.Time1RequiredState === 0;
      this._requireDocumentDuration = def.DurationRequiredState === 0;
      this._readonlyDocumentDuration = def.DurationRequiredState === 0;
      this._requireDocumentDurationUnit = def.DurationUnitRequiredState === 0;
      this._readonlyDocumentDurationUnit = def.DurationUnitRequiredState === 0;
      this._requireDocumentTime2 = def.Time2RequiredState === 0;
      this._readonlyDocumentTime2 = def.Time2RequiredState === 0;
      this._requireDocumentNotedDate = def.NotedDateRequiredState === 0;
      this._readonlyDocumentNotedDate = def.NotedDateRequiredState === 0;

      this._requireDocumentInternalReference = def.InternalReferenceRequiredState === 0;
      this._readonlyDocumentInternalReference = def.InternalReferenceReadOnlyState === 0;
      this._requireDocumentReferenceSource = def.ReferenceSourceRequiredState === 0;
      this._readonlyDocumentReferenceSource = def.ReferenceSourceRequiredState === 0;
      this._requireDocumentExternalReference = def.ExternalReferenceRequiredState === 0;
      this._readonlyDocumentExternalReference = def.ExternalReferenceReadOnlyState === 0;

      for (const lineDefId of def.LineDefinitions.map(e => e.LineDefinitionId)) {
        const lineDef = this.lineDefinition(lineDefId);
        for (const colDef of lineDef.Columns.filter(c => c.InheritsFromHeader === 2)) {

          switch (colDef.ColumnName) {
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

            case 'CenterId':
              if (!this._requireDocumentCenter &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentCenter = true;
              }
              if (!this._readonlyDocumentCenter &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentCenter = true;
              }
              break;
            case 'AgentId':
              if (!this._requireDocumentAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentAgent = true;
              }
              if (!this._readonlyDocumentAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentAgent = true;
              }
              break;
            case 'ResourceId':
              if (!this._requireDocumentResource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentResource = true;
              }
              if (!this._readonlyDocumentResource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentResource = true;
              }
              break;
            case 'NotedAgentId':
              if (!this._requireDocumentNotedAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentNotedAgent = true;
              }
              if (!this._readonlyDocumentNotedAgent &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentNotedAgent = true;
              }
              break;
            case 'NotedResourceId':
              if (!this._requireDocumentNotedResource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentNotedResource = true;
              }
              if (!this._readonlyDocumentNotedResource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentNotedResource = true;
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
            case 'Duration':
              if (!this._requireDocumentDuration &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentDuration = true;
              }
              if (!this._readonlyDocumentDuration &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentDuration = true;
              }
              break;
            case 'DurationUnitId':
              if (!this._requireDocumentDurationUnit &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentDurationUnit = true;
              }
              if (!this._readonlyDocumentDurationUnit &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentDurationUnit = true;
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
            case 'NotedDate':
              if (!this._requireDocumentNotedDate &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentNotedDate = true;
              }
              if (!this._readonlyDocumentNotedDate &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentNotedDate = true;
              }
              break;
            case 'ExternalReference':
              if (!this._requireDocumentExternalReference &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentExternalReference = true;
              }
              if (!this._readonlyDocumentExternalReference &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentExternalReference = true;
              }
              break;
            case 'ReferenceSourceId':
              if (!this._requireDocumentReferenceSource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentReferenceSource = true;
              }
              if (!this._readonlyDocumentReferenceSource &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentReferenceSource = true;
              }
              break;
            case 'InternalReference':
              if (!this._requireDocumentInternalReference &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.RequiredState)) {
                this._requireDocumentInternalReference = true;
              }
              if (!this._readonlyDocumentInternalReference &&
                this.lines(lineDefId, doc).some(line => (line.State || 0) >= colDef.ReadOnlyState || (line.State || 0) < 0)) {
                this._readonlyDocumentInternalReference = true;
              }
              break;
          }
        }
      }
    }
  }

  /////// Properties of the lines

  public account(entry: Entry): Account {
    return this.ws.get('Account', entry.AccountId) as Account;
  }

  public accountType(entry: Entry): AccountType {
    const account = this.account(entry);
    if (!!account && account.AccountTypeId) {
      return this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    }

    return null;
  }

  /**
   * The resource that will eventually be set in the entry after saving
   */
  public resource_old(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const entryResourceId = !!account && !!account.ResourceDefinitionId ? entry.ResourceId : null;
    const resourceId = accountResourceId || entryResourceId;

    return this.ws.get('Resource', resourceId) as Resource;
  }

  /**
   * The resource that will eventually be set in the entry after saving
   */
  public resource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountResourceId = !!account ? account.ResourceId : null;
    const entryResourceId = this.showResource_Manual(entry) ? entry.ResourceId : null;
    const resourceId = accountResourceId || entryResourceId;

    return this.ws.get('Resource', resourceId) as Resource;
  }

  private resourceDefinition(entry: Entry): ResourceDefinitionForClient {
    const resource = this.resource(entry);
    const defId = !!resource ? resource.DefinitionId : null;
    const resourceDefinition = !!defId ? this.ws.definitions.Resources[defId] : null;
    return resourceDefinition;
  }

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

    const s = this.ws.settings;
    const accountNullDefinitionsIncludeAll = !!s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll;

    // Deconstruct the pair object
    const line = pair.line;
    const entry = pair.entry;
    const entryIndex = pair.entryIndex;

    const lineDefId = line.DefinitionId;
    const lineDef = this.lineDefinition(lineDefId);
    if (!!lineDef && !!lineDef.Entries) {
      const entryDef = lineDef.Entries[entryIndex];
      if (!!entryDef && !!entryDef.ParentAccountTypeId) {
        // Account Type Id
        let filter = `AccountType.Id descof ${entryDef.ParentAccountTypeId}`;

        if (accountNullDefinitionsIncludeAll) {
          filter = filter + ` and IsAutoSelected = true`;
        }

        // CurrencyId
        const currencyId = entry.CurrencyId; // this.readonlyValueCurrencyId(entry) || entry.CurrencyId;
        if (!!currencyId) {
          filter = filter + ` and (CurrencyId eq null or CurrencyId eq '${currencyId.replace(`'`, `''`)}')`;
        }

        // CenterId
        const centerId = entry.CenterId; // this.readonlyValueCenterId_Manual(entry) || entry.CenterId;
        if (!!centerId) {
          filter = filter + ` and (CenterId eq null or CenterId eq ${centerId})`;
        }

        // ResourceDefinitionId
        const resource = this.ws.get('Resource', entry.ResourceId) as Resource;
        const resourceDefId = !!resource ? resource.DefinitionId : null;
        if (!!resourceDefId) {
          if (accountNullDefinitionsIncludeAll) {
            filter = filter + ` and (ResourceDefinitionId eq ${resourceDefId} or ResourceDefinitionId eq null)`;
          } else {
            filter = filter + ` and ResourceDefinitionId eq ${resourceDefId}`;
          }
        } else {
          filter = filter + ` and ResourceDefinitionId eq null`;
        }

        // AgentDefinitionId
        const agent = this.ws.get('Agent', entry.AgentId) as Agent;
        const agentDefId = !!agent ? agent.DefinitionId : null;
        if (!!agentDefId) {
          if (accountNullDefinitionsIncludeAll) {
            filter = filter + ` and (AgentDefinitionId eq ${agentDefId} or AgentDefinitionId eq null)`;
          } else {
            filter = filter + ` and AgentDefinitionId eq ${agentDefId}`;
          }
        } else {
          filter = filter + ` and AgentDefinitionId eq null`;
        }

        // NotedAgentDefinitionId
        const notedAgent = this.ws.get('Agent', entry.NotedAgentId) as Agent;
        const notedAgentDefId = !!notedAgent ? notedAgent.DefinitionId : null;
        if (!!notedAgentDefId) {
          if (accountNullDefinitionsIncludeAll) {
            filter = filter + ` and (NotedAgentDefinitionId eq ${notedAgentDefId} or NotedAgentDefinitionId eq null)`;
          } else {
            filter = filter + ` and NotedAgentDefinitionId eq ${notedAgentDefId}`;
          }
        } else {
          filter = filter + ` and NotedAgentDefinitionId eq null`;
        }

        // NotedResourceDefinitionId
        const notedResource = this.ws.get('Resource', entry.NotedResourceId) as Resource;
        const notedResourceDefId = !!notedResource ? notedResource.DefinitionId : null;
        if (!!notedResourceDefId) {
          if (accountNullDefinitionsIncludeAll) {
            filter = filter + ` and (NotedResourceDefinitionId eq ${notedResourceDefId} or NotedResourceDefinitionId eq null)`;
          } else {
            filter = filter + ` and NotedResourceDefinitionId eq ${notedResourceDefId}`;
          }
        } else {
          filter = filter + ` and NotedResourceDefinitionId eq null`;
        }

        // What about NotedAgentId?

        // ResourceId
        const resourceId = entry.ResourceId;
        if (!!resourceId) {
          filter = filter + ` and (ResourceId eq null or ResourceId eq ${resourceId})`;
        }

        // AgentId
        const agentId = entry.AgentId;
        if (!!agentId) {
          filter = filter + ` and (AgentId eq null or AgentId eq ${agentId})`;
        }

        // NotedAgentId
        const notedAgentId = entry.NotedAgentId;
        if (!!notedAgentId) {
          filter = filter + ` and (NotedAgentId eq null or NotedAgentId eq ${notedAgentId})`;
        }

        // NotedResourceId
        const notedResourceId = entry.NotedResourceId;
        if (!!notedResourceId) {
          filter = filter + ` and (NotedResourceId eq null or NotedResourceId eq ${notedResourceId})`;
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
    return !!this.getAccountAgentCenterId(entry);
  }

  public readonlyValueCenterId_Manual(entry: Entry): number {
    return this.getAccountAgentCenterId(entry);
  }

  private getAccountAgentCenterId(entry: Entry): number {
    // returns the center Id (if any) that will eventually be copied to the Entry in the bll
    if (!entry) {
      return null;
    }

    const account = this.account(entry);
    const agent = this.agent(entry);

    const accountCenterId = !!account ? account.CenterId : null;
    const agentCenterId = !!agent ? agent.CenterId : null;

    return accountCenterId || agentCenterId;
  }

  public filterCenter_Manual(entry: Entry): string {
    const s = this.ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.BusinessUnitGoneWithTheWind) {
      return 'IsLeaf eq true';
    } else {
      return null;
    }
  }

  // AgentId
  public showAgent_Manual(entry: Entry): boolean {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      let count = 0;
      const accountType = this.accountType(entry);
      if (!!accountType && !!accountType.AgentDefinitions) {
        for (const e of accountType.AgentDefinitions) {
          count += ws.definitions.Agents[e.AgentDefinitionId] ? 1 : 0;
        }
      }
      return count > 0;
    } else {
      const account = this.account(entry);
      return !!account && !!account.AgentDefinitionId; // Show the agent when account type specifies any visible agent definitions
    }
  }

  public readonlyAgent_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.AgentId;
  }

  public readonlyValueAgentId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.AgentId : null;
  }

  public labelAgent_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.AgentDefinitionId : null;

    return defId ?
      metadata_Agent(this.workspace, this.translate, defId).titleSingular() :
      this.translate.instant('Entry_Agent');
  }

  public definitionIdsAgent_Manual(entry: Entry): number[] {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      const accountType = this.accountType(entry);
      return (accountType.AgentDefinitions || []).map(e => e.AgentDefinitionId).filter(id => ws.definitions.Agents[id]);
    } else {
      const account = this.account(entry);
      return [account.AgentDefinitionId];
    }
  }

  /**
   * The agent that will eventually be set in the entry after saving
   */
  public agent(entry: Entry): Agent {
    const account = this.account(entry);
    const accountAgentId = !!account ? account.AgentId : null;
    const entryAgentId = !!account && !!account.AgentDefinitionId ? entry.AgentId : null;
    const agentId = accountAgentId || entryAgentId;

    return this.ws.get('Agent', agentId) as Agent;
  }

  // NotedAgentId
  public showNotedAgent_Manual(entry: Entry): boolean {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      let count = 0;
      const accountType = this.accountType(entry);
      if (!!accountType && !!accountType.NotedAgentDefinitions) {
        for (const e of accountType.NotedAgentDefinitions) {
          count += ws.definitions.Agents[e.NotedAgentDefinitionId] ? 1 : 0;
        }
      }
      return count > 0;
    } else {
      const account = this.account(entry);
      return !!account && !!account.NotedAgentDefinitionId;
    }
  }

  public readonlyNotedAgent_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.NotedAgentId;
  }

  public readonlyValueNotedAgentId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.NotedAgentId : null;
  }

  public labelNotedAgent_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.NotedAgentDefinitionId : null;

    return defId ?
      metadata_Agent(this.workspace, this.translate, defId).titleSingular() :
      this.translate.instant('Entry_NotedAgent');
  }

  public definitionIdsNotedAgent_Manual(entry: Entry): number[] {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      const accountType = this.accountType(entry);
      return (accountType.NotedAgentDefinitions || []).map(e => e.NotedAgentDefinitionId).filter(id => ws.definitions.Agents[id]);
    } else {
      const account = this.account(entry);
      return [account.NotedAgentDefinitionId];
    }
  }

  /**
   * The noted agent that will eventually be set in the entry after saving
   */
  public notedAgent(entry: Entry): Agent {
    const account = this.account(entry);
    const accountNotedAgentId = !!account ? account.NotedAgentId : null;
    const entryNotedAgentId = !!account && !!account.NotedAgentDefinitionId ? entry.NotedAgentId : null;
    const notedAgentId = accountNotedAgentId || entryNotedAgentId;

    return this.ws.get('Agent', notedAgentId) as Agent;
  }

  // ResourceId

  public showResource_Manual(entry: Entry): boolean {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      let count = 0;
      const accountType = this.accountType(entry);
      if (!!accountType && !!accountType.ResourceDefinitions) {
        for (const e of accountType.ResourceDefinitions) {
          count += ws.definitions.Resources[e.ResourceDefinitionId] ? 1 : 0;
        }
      }
      return count > 0;
    } else {
      const account = this.account(entry);
      return !!account && !!account.ResourceDefinitionId;
    }
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

    return defId ?
      metadata_Resource(this.workspace, this.translate, defId).titleSingular() :
      this.translate.instant('Entry_Resource');
  }

  public definitionIdsResource_Manual(entry: Entry): number[] {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      const accountType = this.accountType(entry);
      return (accountType.ResourceDefinitions || []).map(e => e.ResourceDefinitionId).filter(id => ws.definitions.Resources[id]);
    } else {
      const account = this.account(entry);
      return [account.ResourceDefinitionId];
    }
  }

  // NotedResourceId
  public showNotedResource_Manual(entry: Entry): boolean {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      let count = 0;
      const accountType = this.accountType(entry);
      if (!!accountType && !!accountType.NotedResourceDefinitions) {
        for (const e of accountType.NotedResourceDefinitions) {
          count += ws.definitions.Resources[e.NotedResourceDefinitionId] ? 1 : 0;
        }
      }
      return count > 0;
    } else {
      const account = this.account(entry);
      return !!account && !!account.NotedResourceDefinitionId;
    }
  }

  public readonlyNotedResource_Manual(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && !!account.NotedResourceId;
  }

  public readonlyValueNotedResourceId_Manual(entry: Entry): number {
    const account = this.account(entry);
    return !!account ? account.NotedResourceId : null;
  }

  public labelNotedResource_Manual(entry: Entry): string {
    const account = this.account(entry);
    const defId = !!account ? account.NotedResourceDefinitionId : null;

    return defId ?
      metadata_Resource(this.workspace, this.translate, defId).titleSingular() :
      this.translate.instant('Entry_NotedResource');
  }

  public definitionIdsNotedResource_Manual(entry: Entry): number[] {
    const ws = this.ws;
    const s = ws.settings;
    if (s.FeatureFlags && s.FeatureFlags.AccountNullDefinitionsIncludeAll) {
      const accountType = this.accountType(entry);
      return (accountType.NotedResourceDefinitions || []).map(e => e.NotedResourceDefinitionId).filter(id => ws.definitions.Resources[id]);
    } else {
      const account = this.account(entry);
      return [account.NotedResourceDefinitionId];
    }
  }

  /**
   * The noted resource that will eventually be set in the entry after saving
   */
  public notedResource(entry: Entry): Resource {
    const account = this.account(entry);
    const accountNotedResourceId = !!account ? account.NotedResourceId : null;
    const entryNotedResourceId = !!account && !!account.NotedResourceDefinitionId ? entry.NotedResourceId : null;
    const notedResourceId = accountNotedResourceId || entryNotedResourceId;

    return this.ws.get('Resource', notedResourceId) as Resource;
  }

  // Quantity + Unit

  public showQuantity(entry: Entry): boolean {
    return !!this.resource(entry);
  }

  public showUnit(entry: Entry): boolean {
    const resource = this.resource(entry);
    const resourceDef = !!resource && !!resource.DefinitionId ? this.ws.definitions.Resources[resource.DefinitionId] : null;
    return !!resourceDef && !!resourceDef.UnitCardinality;
  }

  public readonlyUnit(entry: Entry): boolean {
    const def = this.resourceDefinition(entry);
    if (!!def && def.UnitCardinality === 'Single' &&
      def.ResourceDefinitionType !== 'PropertyPlantAndEquipment' &&
      def.ResourceDefinitionType !== 'InvestmentProperty' &&
      def.ResourceDefinitionType !== 'IntangibleAssetsOtherThanGoodwill') {
      const resource = this.resource(entry);
      if (!!resource) {
        return !!resource.UnitId;
      }
    }

    return false;
  }

  public readonlyValueUnitId(entry: Entry): number {
    const resource = this.resource(entry);
    return !!resource ? resource.UnitId : null;
  }

  public filterUnitId(entry: Entry): string {
    // This method is only required when Cardinality === Multi
    const resource = this.resource(entry);
    let filter: string;
    if (!!resource) {
      if (!!resource.UnitId) {
        filter = `Id eq ${resource.UnitId}`;
      }

      if (!!resource.Units && resource.Units.length > 0) {
        const unitsFilter = resource.Units.map(e => `Id eq ${e.UnitId}`).join(' or ');
        if (!!filter) {
          filter += ` or ${unitsFilter}`;
        } else {
          filter = unitsFilter;
        }
      }
    }

    // If account type allows pure, add it to the filter
    const accountType = this.accountType(entry);
    if (!!accountType && accountType.StandardAndPure) {
      const pureFilter = `UnitType eq 'Pure'`;
      if (!!filter) {
        filter += ` or ${pureFilter}`;
      } else {
        filter = pureFilter;
      }
    }

    return filter;
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

  // Duration + DurationUnit

  public showDuration(entry: Entry): boolean {
    return false;
  }

  public showDurationUnit(entry: Entry, line: Line): boolean {
    const lineDef = !!line ? this.lineDefinition(line.DefinitionId) : null;
    return !!lineDef && (lineDef.LineType === 20 || lineDef.LineType === 60 || lineDef.LineType === 80) // Plan Template or Model Template
      && !!this.showTime1_Manual(entry) && this.showTime2_Manual(entry);
  }

  public readonlyDurationUnit(entry: Entry): boolean {
    return false;
  }

  public readonlyValueDurationUnitId(entry: Entry): number {
    return null;
  }

  public filterDurationUnitId(entry: Entry): string {
    return `UnitType eq 'Time'`;
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
    return !!account && !this.getAccountResourceAgentCurrencyId(entry);
  }

  private getAccountResourceAgentCurrencyId(entry: Entry): string {
    // returns the currency Id (if any) that will eventually be copied to the Entry in the bll
    if (!entry) {
      return null;
    }

    const account = this.account(entry);
    const resource = this.resource(entry);
    const agent = this.agent(entry);

    const accountCurrencyId = !!account ? account.CurrencyId : null;
    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;
    const agentCurrencyId = !!agent ? agent.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId || agentCurrencyId;
  }

  public readonlyValueCurrencyId(entry: Entry): string {
    // returns the currency Id if any
    if (!entry) {
      return null;
    }

    const copiedCurrencyId = this.getAccountResourceAgentCurrencyId(entry);
    const entryCurrencyId = entry.CurrencyId;

    return copiedCurrencyId || entryCurrencyId;
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
    return `IsAssignable eq true and Id descof ${accountType.EntryTypeParentId}`;
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

  // ReferenceSourceId
  public showReferenceSource_Manual(entry: Entry): boolean {
    const at = this.accountType(entry);
    return !!at ? !!at.ReferenceSourceLabel : false;
  }

  public readonlyReferenceSource_Manual(entry: Entry): boolean {
    return false;
  }

  public readonlyValueReferenceSourceId_Manual(entry: Entry): number {
    return null;
  }

  public labelReferenceSource_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.ReferenceSourceLabel ?
      this.ws.getMultilingualValueImmediate(at, 'ReferenceSourceLabel') :
      this.translate.instant('Entry_ReferenceSource');
  }

  public definitionIdsReferenceSource_Manual(entry: Entry): number[] {
    return this.ws.definitions.ReferenceSourceDefinitionIds;
  }

  // Internal Reference

  public showInternalReference_Manual(entry: Entry): boolean {
    const account = this.accountType(entry);
    return !!account ? !!account.InternalReferenceLabel : false;
  }

  public labelInternalReference_Manual(entry: Entry): string {
    const at = this.accountType(entry);
    return !!at.InternalReferenceLabel ?
      this.ws.getMultilingualValueImmediate(at, 'InternalReferenceLabel') :
      this.translate.instant('Entry_InternalReference');
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

  /////////////// Attachments - START

  public showAttachmentsErrors(model: Document) {
    return !!model && !!model.Attachments &&
      model.Attachments.some(att => !!att.serverErrors);
  }

  private _attachmentsAttachments: AttachmentForSave[];
  private _attachmentsResult: AttachmentWrapper[];

  public attachmentWrappers(model: DocumentForSave) {
    if (!model || !model.Attachments) {
      return [];
    }

    if (this._attachmentsAttachments !== model.Attachments) {
      this._attachmentsAttachments = model.Attachments;

      this._attachmentsResult = model.Attachments.map(attachment => ({ attachment }));
    }

    return this._attachmentsResult;
  }

  public onFileSelected(input: HTMLInputElement, model: DocumentForSave) {

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

  public onDeleteAttachment(model: DocumentForSave, index: number) {
    this.attachmentWrappers(model).splice(index, 1);
    model.Attachments.splice(index, 1);
  }

  public onDownloadAttachment(model: DocumentForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.downloading = true; // show a little spinner
      this.documentsApi.getAttachment(docId, wrapper.attachment.Id).pipe(
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

  public onPreviewAttachment(model: DocumentForSave, index: number) {
    const docId = model.Id;
    const wrapper = this.attachmentWrappers(model)[index];

    if (!!wrapper.attachment.Id) {
      wrapper.previewing = true; // show a little spinner
      this.documentsApi.getAttachment(docId, wrapper.attachment.Id).pipe(
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

  public registerPristineFunc = (pristineModel: DocumentForSave) => {
    this._pristineDocJson = JSON.stringify(pristineModel);
  }

  public isDirtyFunc = (model: DocumentForSave) => {
    if (!!model && !!model.Attachments && model.Attachments.some(e => !!e.File)) {
      return true; // Optimization so as not to JSON.stringify large files sized in the megabytes every change detector cycle
    }

    return this._pristineDocJson !== JSON.stringify(model);
  }

  private _attachmentSeverErrorInput: { [key: string]: string[] };
  private _attachmentSeverErrorResult: string[];

  public attachmentSeverError = (errors: { [key: string]: string[] }): string[] => {

    if (this._attachmentSeverErrorInput !== errors) {
      this._attachmentSeverErrorInput = errors;

      if (!errors) {
        this._attachmentSeverErrorResult = null;
      } else {
        const result = [];
        if (!!errors.FileName) {
          errors.FileName.forEach(errorMsg => result.push(errorMsg));
        }

        if (!!errors.File) {
          errors.File.forEach(errorMsg => result.push(errorMsg));
        }

        this._attachmentSeverErrorResult = result;
      }
    }

    return this._attachmentSeverErrorResult;
  }

  /////////////// Attachments - END

  public extraParams = { includeRequiredSignatures: true };

  public handleFreshExtras(extras: { [key: string]: any }) {
    if (!!extras) {
      const relatedEntities = extras.RequiredSignaturesRelatedEntities as ({ [key: string]: EntityWithKey[] });
      if (!!relatedEntities) {
        mergeEntitiesInWorkspace(relatedEntities, this.workspace);

        // We always call 'this.workspace.'notifyStateChanged' before this, so no need to call it again
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
    'ToState', 'RuleType', 'RoleId', 'CustodianId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
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

  // Binding for signature modal
  public isNegative: boolean;
  public reasonChoicesForNegativeModal: SelectorChoice[];
  public onBehalfOfUserAmbiguous: boolean;

  // Data collected in signature modal
  public reasonDetails: string;
  public reasonId: number;
  public onBehalfOfUserId: number;

  public onSignYes(signature: RequiredSignature): void {
    const decision = true;

    if (signature.CanSign) {
      // The user can sign herself, straightforward
      this.onSign(signature, decision, null);
    } else if (signature.CanSignOnBehalf) {
      if (!!signature.OnBehalfOfUserId) {
        // The user can sign on behalf of another uniquely-determined user, also straight-forward
        this.onSign(signature, decision, signature.OnBehalfOfUserId);
      } else {
        // The user can sign on behalf 2 or more users, launch a modal that asks which one

        // Reset the params
        this.reasonDetails = null;
        this.reasonId = null;
        this.onBehalfOfUserId = null;

        // Affect the contens of the modal
        this.reasonChoicesForNegativeModal = [];
        this.isNegative = !decision;
        this.onBehalfOfUserAmbiguous = true;

        // Launch the modal
        const modalRef = this.modalService.open(this.signatureModal);
        modalRef.result.then(
          (confirmed: boolean) => {
            if (confirmed) {
              this.onSign(signature, decision, this.onBehalfOfUserId);
            }
          },
          _ => { }
        );
      }
    } else {
      // Programmer mistake
      console.error('Should not be able to sign');
      return;
    }
  }

  public reasonDisplay(lineDefId: number, reasonId: number): string {
    if (!reasonId) {
      return '';
    }

    // No need to optimize this, it is rare anyways
    const lineDef = this.lineDefinition(lineDefId);
    if (!!lineDef.StateReasons) {
      for (const reason of lineDef.StateReasons) {
        if (reason.Id === reasonId) {
          return this.ws.getMultilingualValueImmediate(reason, 'Name');
        }
      }
    }

    return '';
  }

  public onSignNo(lineDefId: number, signature: RequiredSignature): void {
    if (!signature.CanSign && !signature.CanSignOnBehalf) {
      // Programmer mistake
      console.error('Should not be able to sign');
      return;
    }

    const decision = false;

    // Reset the params
    this.reasonDetails = null;
    this.reasonId = null;
    this.onBehalfOfUserId = !signature.CanSign && signature.CanSignOnBehalf ? signature.OnBehalfOfUserId : null;

    // Remember the reason choices for the current line Definition and state
    const lineDef = this.lineDefinition(lineDefId);
    const reasons = !!lineDef ? lineDef.StateReasons || [] : [];
    this.reasonChoicesForNegativeModal = reasons
      .filter(e => Math.abs(e.State) === Math.abs(signature.ToState))
      .map(e => ({ name: () => this.ws.getMultilingualValueImmediate(e, 'Name'), value: e.Id }));

    this.isNegative = !decision; // Affects the contents of the modal
    this.onBehalfOfUserAmbiguous = !signature.CanSign && signature.CanSignOnBehalf && !this.onBehalfOfUserId;

    // Launch the modal that asks the user for the reason behind the negative
    // signature and - if needed - whom is she signing on behalf of
    const modalRef = this.modalService.open(this.signatureModal);
    modalRef.result.then(
      (confirmed: boolean) => {
        if (confirmed) {
          this.onSign(signature, decision, this.onBehalfOfUserId);
        }
      },
      _ => { }
    );
  }

  private onSign(signature: RequiredSignature, yes: boolean, onBehalfOfUserId: number): void {
    const lineIds = this.lineIds(signature);
    this.documentsApi.sign(lineIds, {
      returnEntities: true,
      select: this.select,
      onBehalfOfUserId,
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
    return !!signature.SignedById && (signature.SignedById === this.ws.userSettings.UserId ||
      signature.OnBehalfOfUserId === this.ws.userSettings.UserId);
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
    this._linesModel = null; // Force refresh the UI grids
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

  public onOtherTab(lineDefId: number, _: Document): void {
    this._visibleTabs.push(lineDefId);
    this._visibleTabs = this._visibleTabs.slice();
    this._invisibleTabs = this._invisibleTabs.filter(e => e !== lineDefId);

    this.setActiveTab(lineDefId);
  }

  public bookkeepingManualAdjustmentsTitle(model: DocumentForSave): string {
    return this.tabTitle(this.ws.definitions.ManualLinesDefinitionId, model);
  }

  public tabTitle(lineDefId: number, model: DocumentForSave): string {
    if (this.isJV && this.isManualLine(lineDefId)) {
      return this.translate.instant('Entries');
    } else {
      const def = this.lineDefinition(lineDefId);
      const isForm = this.showAsForm(lineDefId, model);
      return !!def ? this.ws.getMultilingualValueImmediate(def, isForm ? 'TitleSingular' : 'TitlePlural')
        : this.translate.instant('Undefined');
    }
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

  private _tabEntries: { [defId: number]: LineForSave[] };
  private _tabEntriesModel: DocumentForSave;

  /**
   * Returns the array of DocumentLineDefinitionEntries indexed by EntryIndex (may contain gaps)
   */
  public tabEntries(lineDefId: number, model: Document): DocumentLineDefinitionEntry[] {
    if (!model) {
      return [];
    }

    if (this._tabEntriesModel !== model) {
      this._tabEntriesModel = model;
      this._tabEntries = {};

      if (!!model.LineDefinitionEntries) {
        for (const tabEntry of model.LineDefinitionEntries) {
          if (!this._tabEntries[tabEntry.LineDefinitionId]) {
            this._tabEntries[tabEntry.LineDefinitionId] = [];
          }

          this._tabEntries[tabEntry.LineDefinitionId][tabEntry.EntryIndex] = tabEntry;
        }
      }
    }

    if (!this._tabEntries[lineDefId]) {
      this._tabEntries[lineDefId] = [];
    }

    return this._tabEntries[lineDefId];
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
    return !!this.manualLine(model) && !(this.showDocumentPostingDate(model) && this.showDocumentMemo(model));
  }

  public onInsertManualEntry(pair: LineEntryPair, model: Document): void {
    // Called when the user inserts a new entry
    this.addManualEntry(pair.entry, model);
    pair.line = this.manualLine(model); // Must exist after calling addManualEntry
  }

  /**
   * Finds the one manual line (creates it if missing) and adds the entry to it
   */
  private addManualEntry(entry: Entry, model: Document): void {
    // Get the one and only manual line
    let manualLine = this.manualLine(model);
    if (!manualLine) {
      manualLine = {
        PostingDate: todayISOString(),
        DefinitionId: this.ws.definitions.ManualLinesDefinitionId,
        Entries: [],
        _flags: { isModified: true }
      };

      model.Lines.push(manualLine);

      // Optimization
      this._manualLineModel = model;
      this._manualLineResult = manualLine;
    }

    // Add the entry to it
    manualLine.Entries.push(entry);
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
      model.Lines.splice(lineIndex, 1);

      this._manualLineModel = model;
      this._manualLineResult = null;
    }
  }

  public onNewManualEntry = (pair: LineEntryPair) => {
    // Called when a new entry is created, including placeholder entry

    // Set the entry
    pair.entry = {
      Id: 0,
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

  public showTabErrors(lineDefId: number, model: Document) {
    // Get the relevant tab entries
    const tabEntries = this.tabEntries(lineDefId, model);
    const lines = this.lines(lineDefId, model);
    return (!!tabEntries && tabEntries.some(tabEntry => !!tabEntry.serverErrors)) ||
      (!!lines && lines.some(line => !!line.serverErrors || (!!line.Entries && line.Entries.some(entry => !!entry.serverErrors))));
  }

  public manualColumnPaths(model: DocumentForSave, bookkeeping = false): string[] {
    const paths = ['AccountId', 'Center', 'Debit', 'Credit'];


    // if (!model.MemoIsCommon) {
    if (bookkeeping) {
      // This only appears in the smart bookkeeping grid
      paths.push('Memo');
    }

    if (bookkeeping) {
      paths.push('ModifiedWarning');
    }

    paths.push('Commands');

    return paths;
  }

  private _defaultTabEntry: DocumentLineDefinitionEntryForSave = {
    PostingDateIsCommon: true,
    MemoIsCommon: true,
    CurrencyIsCommon: true,
    CenterIsCommon: true,
    AgentIsCommon: true,
    ResourceIsCommon: true,
    NotedAgentIsCommon: true,
    NotedResourceIsCommon: true,
    QuantityIsCommon: true,
    UnitIsCommon: true,
    Time1IsCommon: true,
    DurationIsCommon: true,
    DurationUnitIsCommon: true,
    Time2IsCommon: true,
    NotedDateIsCommon: true,
    ExternalReferenceIsCommon: true,
    ReferenceSourceIsCommon: true,
    InternalReferenceIsCommon: true,
  };

  private _smartTabHeaderColumnPathsIsCommonHasChanged = false;
  private _smartTabHeaderColumnPathsDefinitions: DefinitionsForClient;
  private _smartTabHeaderColumnPathsModel: Document;
  private _smartTabHeaderColumnPathsLineDefId: number;
  private _smartTabHeaderColumnPathsResult: number[];

  public smartTabHeaderColumnPaths(lineDefId: number, doc: Document): number[] {
    // It is named smartTabHeaderColumnPaths to mirror manualColumnPaths, even though the returned array is just column indices

    if (this._smartTabHeaderColumnPathsIsCommonHasChanged ||
      this._smartTabHeaderColumnPathsModel !== doc ||
      this._smartTabHeaderColumnPathsLineDefId !== lineDefId ||
      this._smartTabHeaderColumnPathsDefinitions !== this.ws.definitions) {

      this._smartTabHeaderColumnPathsIsCommonHasChanged = false;
      this._smartTabHeaderColumnPathsModel = doc;
      this._smartTabHeaderColumnPathsLineDefId = lineDefId;
      this._smartTabHeaderColumnPathsDefinitions = this.ws.definitions;

      // All line definitions other than 'ManualLine'
      const lineDef = this.lineDefinition(lineDefId);

      const result = !!lineDef && !!lineDef.Columns ? lineDef.Columns
        .map((column, index) => ({ column, index })) // Capture the index first thing
        .filter(e => {
          const col = e.column;

          if (col.InheritsFromHeader >= 2 && (
            (doc.PostingDateIsCommon && col.ColumnName === 'PostingDate') ||
            (doc.MemoIsCommon && col.ColumnName === 'Memo') ||
            (doc.CurrencyIsCommon && col.ColumnName === 'CurrencyId') ||
            (doc.CenterIsCommon && col.ColumnName === 'CenterId') ||
            (doc.AgentIsCommon && col.ColumnName === 'AgentId') ||
            (doc.ResourceIsCommon && col.ColumnName === 'ResourceId') ||
            (doc.NotedAgentIsCommon && col.ColumnName === 'NotedAgentId') ||
            (doc.NotedResourceIsCommon && col.ColumnName === 'NotedResourceId') ||
            (doc.QuantityIsCommon && col.ColumnName === 'Quantity') ||
            (doc.UnitIsCommon && col.ColumnName === 'UnitId') ||
            (doc.Time1IsCommon && col.ColumnName === 'Time1') ||
            (doc.DurationIsCommon && col.ColumnName === 'Duration') ||
            (doc.DurationUnitIsCommon && col.ColumnName === 'DurationUnitId') ||
            (doc.Time2IsCommon && col.ColumnName === 'Time2') ||
            (doc.NotedDateIsCommon && col.ColumnName === 'NotedDate') ||
            (doc.ExternalReferenceIsCommon && col.ColumnName === 'ExternalReference') ||
            (doc.ReferenceSourceIsCommon && col.ColumnName === 'ReferenceSourceId') ||
            (doc.InternalReferenceIsCommon && col.ColumnName === 'InternalReference')
          )) {
            // This column inherits from document header, hide it from the tab header
            return false;
          } else if (!lineDef.ViewDefaultsToForm && col.InheritsFromHeader >= 1) {
            switch (col.ColumnName) {
              case 'PostingDate':
              case 'Memo':
              case 'CurrencyId':
              case 'CenterId':
              case 'AgentId':
              case 'ResourceId':
              case 'NotedAgentId':
              case 'NotedResourceId':
              case 'Quantity':
              case 'UnitId':
              case 'Time1':
              case 'Duration':
              case 'DurationUnitId':
              case 'Time2':
              case 'NotedDate':
              case 'ExternalReference':
              case 'ReferenceSourceId':
              case 'InternalReference':
                return true;
            }
          }

          return false;
        })
        .map(e => e.index) : [];

      this._smartTabHeaderColumnPathsResult = result;
    }

    return this._smartTabHeaderColumnPathsResult;
  }

  private _smartColumnIndicesIsCommonHasChanged = false;
  private _smartColumnIndicesDefinitions: DefinitionsForClient;
  private _smartColumnIndicesModel: Document;
  private _smartColumnIndicesLineDefId: number;
  private _smartColumnIndicesResult: number[];

  public smartColumnIndices(lineDefId: number, doc: Document): number[] {
    // Returns the smart column indices that are visible in the table or form
    if (this._smartColumnIndicesIsCommonHasChanged ||
      this._smartColumnIndicesModel !== doc ||
      this._smartColumnIndicesLineDefId !== lineDefId ||
      this._smartColumnIndicesDefinitions !== this.ws.definitions) {

      this._smartColumnIndicesIsCommonHasChanged = false;
      this._smartColumnIndicesModel = doc;
      this._smartColumnIndicesLineDefId = lineDefId;
      this._smartColumnIndicesDefinitions = this.ws.definitions;

      // All line definitions other than 'ManualLine'
      const lineDef = this.lineDefinition(lineDefId);
      const tabEntries: DocumentLineDefinitionEntryForSave[] = [];
      if (!!doc.LineDefinitionEntries) {
        for (const tabEntry of doc.LineDefinitionEntries.filter(e => e.LineDefinitionId === lineDefId)) {
          tabEntries[tabEntry.EntryIndex] = tabEntry;
        }
      }

      const result = !!lineDef && !!lineDef.Columns ? lineDef.Columns
        .map((column, index) => ({ column, index })) // Capture the index first thing
        .filter(e => {
          const col = e.column;

          if (col.InheritsFromHeader >= 2 && (
            (doc.PostingDateIsCommon && col.ColumnName === 'PostingDate') ||
            (doc.MemoIsCommon && col.ColumnName === 'Memo') ||
            (doc.CurrencyIsCommon && col.ColumnName === 'CurrencyId') ||
            (doc.CenterIsCommon && col.ColumnName === 'CenterId') ||
            (doc.AgentIsCommon && col.ColumnName === 'AgentId') ||
            (doc.ResourceIsCommon && col.ColumnName === 'ResourceId') ||
            (doc.NotedAgentIsCommon && col.ColumnName === 'NotedAgentId') ||
            (doc.NotedResourceIsCommon && col.ColumnName === 'NotedResourceId') ||
            (doc.QuantityIsCommon && col.ColumnName === 'Quantity') ||
            (doc.UnitIsCommon && col.ColumnName === 'UnitId') ||
            (doc.Time1IsCommon && col.ColumnName === 'Time1') ||
            (doc.DurationIsCommon && col.ColumnName === 'Duration') ||
            (doc.DurationUnitIsCommon && col.ColumnName === 'DurationUnitId') ||
            (doc.Time2IsCommon && col.ColumnName === 'Time2') ||
            (doc.NotedDateIsCommon && col.ColumnName === 'NotedDate') ||
            (doc.ExternalReferenceIsCommon && col.ColumnName === 'ExternalReference') ||
            (doc.ReferenceSourceIsCommon && col.ColumnName === 'ReferenceSourceId') ||
            (doc.InternalReferenceIsCommon && col.ColumnName === 'InternalReference')
          )) {
            // This column inherits from document header, hide it from the grid
            return false;
          } else {
            const tabEntryIndex = this.tabEntryIndex(col);
            const tab = tabEntries[tabEntryIndex] || this._defaultTabEntry;
            if (!lineDef.ViewDefaultsToForm && col.InheritsFromHeader >= 1 && (
              (tab.PostingDateIsCommon && col.ColumnName === 'PostingDate') ||
              (tab.MemoIsCommon && col.ColumnName === 'Memo') ||
              (tab.CurrencyIsCommon && col.ColumnName === 'CurrencyId') ||
              (tab.CenterIsCommon && col.ColumnName === 'CenterId') ||
              (tab.AgentIsCommon && col.ColumnName === 'AgentId') ||
              (tab.ResourceIsCommon && col.ColumnName === 'ResourceId') ||
              (tab.NotedAgentIsCommon && col.ColumnName === 'NotedAgentId') ||
              (tab.NotedResourceIsCommon && col.ColumnName === 'NotedResourceId') ||
              (tab.QuantityIsCommon && col.ColumnName === 'Quantity') ||
              (tab.UnitIsCommon && col.ColumnName === 'UnitId') ||
              (tab.Time1IsCommon && col.ColumnName === 'Time1') ||
              (tab.DurationIsCommon && col.ColumnName === 'Duration') ||
              (tab.DurationUnitIsCommon && col.ColumnName === 'DurationUnitId') ||
              (tab.Time2IsCommon && col.ColumnName === 'Time2') ||
              (tab.NotedDateIsCommon && col.ColumnName === 'NotedDate') ||
              (tab.ExternalReferenceIsCommon && col.ColumnName === 'ExternalReference') ||
              (tab.ReferenceSourceIsCommon && col.ColumnName === 'ReferenceSourceId') ||
              (tab.InternalReferenceIsCommon && col.ColumnName === 'InternalReference')
            )) {
              return false;
            }
          }

          return true;
        })
        .map(e => e.index) : [];

      this._smartColumnIndicesResult = result;
    }

    return this._smartColumnIndicesResult;

  }

  private _smartColumnPathsForTableNumberPaths: number[];
  private _smartColumnPathsForTableResult: string[];

  public smartColumnPathsForTable(lineDefId: number, doc: Document): string[] {
    const numberPaths = this.smartColumnIndices(lineDefId, doc);
    if (this._smartColumnPathsForTableNumberPaths !== numberPaths) {

      this._smartColumnPathsForTableNumberPaths = numberPaths;

      const result = this.smartColumnIndices(lineDefId, doc).map(e => e + '');

      this._smartColumnPathsForTableResult = result;
    }

    return this._smartColumnPathsForTableResult;
  }

  public smartTotalsColumnIndices(lineDefId: number, model: Document, isEdit: boolean): number[] {
    const result: number[] = [];

    for (const i of this.smartColumnIndices(lineDefId, model)) {
      const colDef = this.columnDefinition(lineDefId, i);
      const lines = this.lines(lineDefId, model);

      switch (colDef.ColumnName) {
        case 'Value':
          result.push(i);
          break;

        case 'MonetaryValue':
          if (!lines || lines.length <= 1) {
            result.push(i);
          } else {
            const firstEntry = lines[0].Entries[colDef.EntryIndex];
            const firstCurrencyId = isEdit ? this.readonlyValueCurrencyId(firstEntry) : firstEntry.CurrencyId;
            if (lines.every(l => {
              // IF the currency Id of a line cannot be determined yet (e.g. new line), assume conformity and show total
              const currentEntry = l.Entries[colDef.EntryIndex];
              const currentCurrencyId = isEdit ? this.readonlyValueCurrencyId(currentEntry) : currentEntry.CurrencyId;
              return !currentCurrencyId || currentCurrencyId === firstCurrencyId;
            })) {
              result.push(i);
            }
          }
          break;

        case 'Quantity':
          if (!lines || lines.length <= 1) {
            result.push(i);
          } else {
            const firstEntry = lines[0].Entries[colDef.EntryIndex];
            const firstUnitId = isEdit && this.readonlyUnit(firstEntry) ?
              this.readonlyValueUnitId(firstEntry) : firstEntry.UnitId;

            if (lines.every(l => {
              // IF the unit Id of a line cannot be determined yet (e.g. new line), assume conformity and show total
              const currentEntry = l.Entries[colDef.EntryIndex];
              const currentUnitId = isEdit && this.readonlyUnit(currentEntry) ?
                this.readonlyValueUnitId(currentEntry) : currentEntry.UnitId;

              return !currentUnitId || currentUnitId === firstUnitId;
            })) {
              result.push(i);
            }
          }
          break;
      }
    }

    return result;
  }

  public smartTotal(lineDefId: number, columnIndex: number, model: Document) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entryIndex = colDef.EntryIndex;

    let getter: (e: Entry) => number;
    switch (colDef.ColumnName) {
      case 'Value':
        getter = (e: Entry) => e.Value;
        break;
      case 'MonetaryValue':
        getter = (e: Entry) => e.MonetaryValue;
        break;
      case 'Quantity':
        getter = (e: Entry) => e.Quantity;
        break;
    }

    return this.lines(lineDefId, model)
      .filter(line => (line.State || 0) >= 0)
      .map(line => !!line.Entries ? getter(line.Entries[entryIndex]) || 0 : 0)
      .reduce((total, v) => total + v, 0);
  }

  private _columnTemplatesLineDefId: number;
  private _columnTemplatesDef: DocumentDefinitionForClient;
  private _columnTemplatesResult: ColumnTemplates;

  public columnTemplates(lineDefId: number): ColumnTemplates {

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
          weight: 1,
          argument: colIndex
        };
      }

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

  public columnName(lineDefId: number, columnIndex: number): EntryColumnName {
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
    if (!!def) {
      switch (def.ColumnName) {
        case 'Memo':
        case 'PostingDate':
        case 'Boolean1':
        case 'Decimal1':
        case 'Decimal2':
        case 'Text1':
        case 'Text2':
          return line;
        default:
          return !!line && !!line.Entries ? line.Entries[def.EntryIndex] : null;
      }
    }
  }

  /**
   * Returns a hard-coded 0 for Memo and PostingDate and the actual entry index otherwise
   */
  private tabEntryIndex(colDef: LineDefinitionColumnForClient): number {
    switch (colDef.ColumnName) {
      case 'Memo':
      case 'PostingDate':
      case 'Boolean1':
      case 'Decimal1':
      case 'Decimal2':
      case 'Text1':
      case 'Text2':
        return 0;
      default:
        return colDef.EntryIndex;
    }
  }

  /**
   * Returns the DocumentLineDefinitionEntry that matches the lineDefId and colDef
   */
  private tabEntry(lineDefId: number, colDef: LineDefinitionColumnForClient, doc: DocumentForSave): DocumentLineDefinitionEntryForSave {
    if (!doc.LineDefinitionEntries) {
      return undefined;
    }

    const entryIndex = this.tabEntryIndex(colDef);
    return this.tabEntries(lineDefId, doc)[entryIndex];
  }

  public entry(lineDefId: number, columnIndex: number, line: LineForSave): EntryForSave {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    if (!!colDef && colDef.ColumnName !== 'Memo' &&
      colDef.ColumnName !== 'PostingDate' &&
      colDef.ColumnName !== 'Boolean1' &&
      colDef.ColumnName !== 'Decimal1' &&
      colDef.ColumnName !== 'Decimal2' &&
      colDef.ColumnName !== 'Text1' &&
      colDef.ColumnName !== 'Text2') {
      return !!line && !!line.Entries ? line.Entries[colDef.EntryIndex] : null;
    }

    return null;
  }

  public definitionIdsAgent_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.AgentDefinitionIds ? entryDef.AgentDefinitionIds : [];
  }

  public definitionIdsResource_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.ResourceDefinitionIds ? entryDef.ResourceDefinitionIds : [];
  }

  public definitionIdsNotedAgent_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.NotedAgentDefinitionIds ? entryDef.NotedAgentDefinitionIds : [];
  }

  public definitionIdsNotedResource_Smart(lineDefId: number, columnIndex: number): number[] {
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    return !!entryDef && !!entryDef.NotedResourceDefinitionIds ? entryDef.NotedResourceDefinitionIds : [];
  }

  public definitionIdsReferenceSource_Smart(lineDefId: number, columnIndex: number): number[] {
    return this.ws.definitions.ReferenceSourceDefinitionIds;
  }

  public getFilter(lineDefId: number, columnIndex: number): string {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return !!colDef ? colDef.Filter : null;
  }

  public entryTypeFilter(lineDefId: number, columnIndex: number): string {
    // Filter for smart line
    // TODO: What about EntryTypeId ??
    const entryDef = this.entryDefinition(lineDefId, columnIndex);
    const parentFilter = !!entryDef && !!entryDef.EntryTypeParentId ? `Id descof ${entryDef.EntryTypeParentId}` : null;

    const customFilter = this.getFilter(lineDefId, columnIndex);

    if (!!parentFilter && !!customFilter) {
      return `(${parentFilter}) and (${customFilter})`;
    } else if (!!parentFilter) {
      return parentFilter;
    } else if (customFilter) {
      return customFilter;
    } else {
      return null;
    }
  }

  public lineEntryServerErrors(lineDefId: number, columnIndex: number, line: LineForSave): string[] {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const entity = this.entity(colDef, line);
    return !!entity && !!entity.serverErrors ? entity.serverErrors[colDef.ColumnName] : null;
  }

  public tabHeaderServerErrors(lineDefId: number, columnIndex: number, doc: DocumentForSave) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const tabEntry = this.tabEntry(lineDefId, colDef, doc);
    return !!tabEntry && !!tabEntry.serverErrors ? tabEntry.serverErrors[colDef.ColumnName] : null;
  }

  public savePreprocessing = (doc: DocumentForSave): void => {
    // Add all missing DocumentLineDefinitionEntries (the tab entries)
    // This is so that the server is able to report errors on any
    // tab entry slot even if the tab entry was not created by that user
    // The server removes excess tab entries anyways in DocumentsController.SavePreprocessAsync
    const def = this.definition;
    const lineDefIds = def.LineDefinitions.map(e => e.LineDefinitionId);
    for (const lineDefId of lineDefIds) {
      const lineDef = this.lineDefinition(lineDefId);
      for (const colDef of lineDef.Columns) {
        const tabEntry = this.tabEntry(lineDefId, colDef, doc);
        if (!tabEntry) {
          this.addNewTabEntry(lineDefId, colDef, doc);
        }
      }
    }
  }

  public getFieldValue(lineDefId: number, columnIndex: number, line: LineForSave, doc: DocumentForSave): any {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    if (!!line) {
      // Get the value from the line or the entry
      const entity = this.entity(colDef, line);
      return !!entity ? entity[colDef.ColumnName] : null;
    } else {
      // Get the value from the tab header
      let tabEntry = this.tabEntry(lineDefId, colDef, doc);
      if (!tabEntry) {
        tabEntry = this._defaultTabEntry;
      }

      return tabEntry[colDef.ColumnName];
    }
  }

  /**
   * Creates a new DocumentLineDefinitionEntry from defaults and adds it to the document
   */
  private addNewTabEntry(lineDefId: number, colDef: LineDefinitionColumnForClient, doc: DocumentForSave): DocumentLineDefinitionEntry {
    const tabEntry = {
      EntryIndex: this.tabEntryIndex(colDef),
      LineDefinitionId: lineDefId,
      ... this._defaultTabEntry
    };

    doc.LineDefinitionEntries.push(tabEntry);
    this.tabEntries(lineDefId, doc)[tabEntry.EntryIndex] = tabEntry;

    return tabEntry;
  }

  private isCommonPropertyName(prop: EntryColumnName): string {

    switch (prop) {
      case 'PostingDate': return 'PostingDateIsCommon';
      case 'Memo': return 'MemoIsCommon';
      case 'CurrencyId': return 'CurrencyIsCommon';
      case 'CenterId': return 'CenterIsCommon';

      case 'AgentId': return 'AgentIsCommon';
      case 'ResourceId': return 'ResourceIsCommon';
      case 'NotedAgentId': return 'NotedAgentIsCommon';
      case 'NotedResourceId': return 'NotedResourceIsCommon';

      case 'Quantity': return 'QuantityIsCommon';
      case 'UnitId': return 'UnitIsCommon';
      case 'Time1': return 'Time1IsCommon';
      case 'Duration': return 'DurationIsCommon';
      case 'DurationUnitId': return 'DurationUnitIsCommon';
      case 'Time2': return 'Time2IsCommon';
      case 'NotedDate': return 'NotedDateIsCommon';
      case 'ExternalReference': return 'ExternalReferenceIsCommon';
      case 'ReferenceSourceId': return 'ReferenceSourceIsCommon';
      case 'InternalReference': return 'InternalReferenceIsCommon';
      default: {
        console.error(`Could not determine IsCommon version of column name ${prop}`);
        return '';
      }
    }
  }

  public setFieldValue(lineDefId: number, columnIndex: number, line: LineForSave, doc: DocumentForSave, value: any): void {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    if (!!line) {
      const entity = this.entity(colDef, line);
      if (!!entity) {
        entity[colDef.ColumnName] = value;
      }
    } else {
      const tabEntry = this.tabEntry(lineDefId, colDef, doc) || this.addNewTabEntry(lineDefId, colDef, doc);
      tabEntry[colDef.ColumnName] = value;
    }
  }

  public onToggleDocumentIsCommon(model: Document, prop: string) {
    if (!!model && !!prop) {
      model[prop] = !model[prop];

      this._smartTabHeaderColumnPathsIsCommonHasChanged = true;
      this._smartColumnIndicesIsCommonHasChanged = true;
    }
  }

  public onToggleTabIsCommon(lineDefId: number, columnIndex: number, doc: DocumentForSave): void {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const tabEntry = this.tabEntry(lineDefId, colDef, doc) || this.addNewTabEntry(lineDefId, colDef, doc);

    const isCommonPropName = this.isCommonPropertyName(colDef.ColumnName);

    tabEntry[isCommonPropName] = !tabEntry[isCommonPropName];

    this._smartColumnIndicesIsCommonHasChanged = true;
  }

  public tabIsCommon(lineDefId: number, columnIndex: number, doc: DocumentForSave): boolean {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const tabEntry = this.tabEntry(lineDefId, colDef, doc) || this._defaultTabEntry;

    const isCommonPropName = this.isCommonPropertyName(colDef.ColumnName);
    return tabEntry[isCommonPropName];
  }

  public isReadOnly(lineDefId: number, columnIndex: number, line: LineForSave, doc: DocumentForSave) {
    // return false;
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const lines = this.lines(lineDefId, doc);

    // inline function
    const isReadOnlyInner = (e: Line) => {
      const state = e.State || 0;
      return state < 0 || state >= colDef.ReadOnlyState;
    };

    if (!!line) {
      return isReadOnlyInner(line);
    } else {
      return lines.some(isReadOnlyInner); // One read-only line, makes the header read-only
    }
  }

  public isRequired(lineDefId: number, columnIndex: number, line: LineForSave, doc: DocumentForSave) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    const lines = this.lines(lineDefId, doc);

    // inline function
    const isRequiredInner = (e: Line) => {
      const state = e.State || 0;
      return state >= colDef.RequiredState;
    };

    if (!!line) {
      return isRequiredInner(line);
    } else {
      return lines.some(isRequiredInner); // One required line, makes the header required
    }
  }

  private _onNewLineFactoryLineDefId: number;
  private _onNewLineFactoryResult: (item: LineForSave) => LineForSave;

  public onNewSmartLineFactory(lineDefId: number): (item: LineForSave) => LineForSave {
    if (this._onNewLineFactoryLineDefId !== lineDefId) {
      this._onNewLineFactoryLineDefId = lineDefId;
      this._onNewLineFactoryResult = (line) => {
        // set the definition Id
        line.DefinitionId = lineDefId;
        line.Boolean1 = false;
        line._flags = { isModified: true };
        // Add the specified number of entries
        line.Entries = [];
        const lineDef = this.lineDefinition(lineDefId);
        if (!!lineDef) {
          if (lineDef.Entries) {
            for (let i = 0; i < lineDef.Entries.length; i++) {
              const entryDef = lineDef.Entries[i];
              line.Entries[i] = { Id: 0, Direction: entryDef.Direction, Value: 0 };
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
      case 'Duration':
        return 'right';
      default:
        return null;
    }
  }

  public defaultsToForm(lineDefId: number) {
    const lineDef = this.lineDefinition(lineDefId);
    return !!lineDef ? lineDef.ViewDefaultsToForm : false;
  }

  public showAsForm(lineDefId: number, model: DocumentForSave) {
    const count = this.lines(lineDefId, model).length;
    if (count > 1) {
      return false;
    } else {
      return this.defaultsToForm(lineDefId);
    }
  }

  public showTabHeader(lineDefId: number, doc: DocumentForSave, isEdit: boolean) {
    return this.smartTabHeaderColumnPaths(lineDefId, doc).length > 0 ||
      this.showBarcodeField(lineDefId, isEdit);
  }

  public dummyUpdate = () => { };

  public onCreateForm(lineDefId: number, model: DocumentForSave) {
    this.onAddNewLine(lineDefId, model);
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

  private onAddNewLine(lineDefId: number, model: DocumentForSave): LineForSave {
    // (1) Adds a new default line to the document (with the specified number of entries)
    // (2) Updates the UI grids
    // (3) Updates the smart entries grid
    if (!model) {
      return;
    }

    let newLine: LineForSave = {};
    newLine = this.onNewSmartLineFactory(lineDefId)(newLine);
    this.lines(lineDefId, model).push(newLine);
    this.onInsertSmartLine(newLine, model);

    return newLine;
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

    if (!this.details || this.details.state.detailsStatus !== DetailsStatus.loaded) {
      return; // Don't do anything unless the doc is loaded
    }

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
    return !!requiredSignatures &&
      // requiredSignatures.filter(e => (!e.LastNegativeState || !!e.SignedById)).some(e => !e.SignedById);
      requiredSignatures.some(e => !e.LastNegativeState && !e.SignedById);
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
    this.flags(line, doc, true).isModified = true;
  }

  public onSmartLineUpdated(update: (item: LineForSave) => void, line: LineForSave, doc: DocumentForSave) {
    if (!!line) {
      this.setModified(line, doc); // Flags the line as modified
      update(line);
    }
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
      const date = doc.PostingDate || todayISOString();
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
                  1: dateFormat(date, this.workspace, this.translate)
                });
                this.details.displayErrorModal(message);
              } else {
                this.details.displayErrorModal(error.error);
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

  public onReverse(pair: LineEntryPair, doc: Document, isEdit: boolean): void {
    if (!isEdit) {
      return;
    }

    const entry = pair.entry;
    const clone = this.processEntryClone(JSON.parse(JSON.stringify(entry)));

    // Reverse the direction
    clone.Direction = clone.Direction === 1 ? -1 : 1; // typescript complains if a simply multiply by -1

    // Insert the reversed entry in the manual line
    this.addManualEntry(clone, doc);
    this._computeEntriesModel = null; // To refresh the manual entries grid
  }

  public onDeleteAll(lineDefId: number, doc: Document, isEdit: boolean): void {
    if (!isEdit) {
      return;
    }

    doc.Lines = doc.Lines.filter(line => line.DefinitionId !== lineDefId);
    this._computeEntriesModel = null; // Refresh the bookkeping and manual lines
    this._linesModel = null; // Refresh the smart tabs
  }

  public onDeleteAllManualEntries(doc: Document, isEdit: boolean): void {
    this.onDeleteAll(this.ws.definitions.ManualLinesDefinitionId, doc, isEdit);
  }

  public onCloneManualLine(pair: LineEntryPair, doc: Document, isEdit: boolean): void {
    if (!isEdit) {
      return;
    }

    const entry = pair.entry;
    const clone = this.processEntryClone(JSON.parse(JSON.stringify(entry)));

    // Insert the reversed entry in the manual line
    this.addManualEntry(clone, doc);
    this._computeEntriesModel = null; // To refresh the manual entries grid
  }

  public showAutoGenerate(lineDefId: number, isEdit: boolean): boolean {
    if (!isEdit) {
      return false;
    }

    const lineDef = this.lineDefinition(lineDefId);
    return !!lineDef && lineDef.GenerateScript;
  }

  public autoGenerateLabel(lineDefId: number): string {
    const lineDef = this.lineDefinition(lineDefId);
    return this.ws.getMultilingualValueImmediate(lineDef, 'GenerateLabel') || this.translate.instant('AutoGenerate');
  }

  // Used by the auto-generate modal
  public autoGenerateArgs: { [key: string]: any } = {};
  public autoGenerateLineDefId: number;
  public autoGenerateDoc: Document;

  public parameterLabel(p: LineDefinitionGenerateParameterForClient) {
    let label = this.ws.getMultilingualValueImmediate(p, 'Label');
    if (p.Visibility === 'Required') {
      label = `${label} *`;
    }

    return label;
  }

  public parameterDesc(p: LineDefinitionGenerateParameterForClient): PropVisualDescriptor {
    return p.desc || (p.desc = descFromControlOptions(this.ws, p.Control, p.ControlOptions));
  }

  public updateOn(p: LineDefinitionGenerateParameterForClient): 'change' | 'blur' {
    const desc = this.parameterDesc(p);
    return updateOn(desc);
  }


  public get autoGenerateLineDef(): LineDefinitionForClient {
    return this.lineDefinition(this.autoGenerateLineDefId);
  }

  public trackByKey(p: LineDefinitionGenerateParameterForClient): string {
    return p.Key;
  }

  public onAutoGenerate(lineDefId: number, doc: Document, isEdit: boolean): void {
    if (!isEdit) {
      return;
    }

    const lineDef = this.lineDefinition(lineDefId);
    if (this.autoGenerateLineDefId !== lineDefId ||
      this.autoGenerateDoc !== doc) {
      // This IF statement to keep the args if the use is still on the same document
      this.autoGenerateDoc = doc;
      this.autoGenerateLineDefId = lineDefId;
      this.autoGenerateArgs = {}; // Reset the args
    }

    if (lineDef.GenerateParameters.length > 0) {

      // Launch the modal that takes the parameters and calls onDoAutoGenerate
      this.modalService.open(this.autoGenerateModal);

    } else {
      // No parameters needed, generate right away
      this.onDoAutoGenerate();
    }
  }

  public onDoAutoGenerate(modal?: NgbModalRef): void {
    const lineDefId = this.autoGenerateLineDefId;
    const doc = this.autoGenerateDoc;
    const clone = this.clone(doc, false); // Do not remove the Ids

    if (!this.autoGenerateRequiredParamsAreSet) {
      return; // Can't call the API unless all required params are set
    }

    // Call the API and retrieve the generated lines
    this.documentsApi.autoGenerate(lineDefId, [clone], this.autoGenerateArgs).pipe(
      tap((res: EntitiesResponse<LineForSave>) => {
        // Close the modal if one is open
        if (!!modal) {
          modal.close();
        }

        // Handle the result
        this.handleAutoGenerateResult(res, doc);
      }),
    ).subscribe({ error: this.details.handleActionError });
  }

  /**
   * Runs auto-generate for all line definitions that have no required params
   */
  public onAutoGenerateAll(doc: Document, isEdit: boolean): void {
    if (!isEdit) {
      return;
    }

    const lineDefIds = this.generatableLineDefIdsWithoutRequiredParams();

    // Call the API and retrieve the generated lines
    const clone = this.clone(doc, false); // Do not remove the Ids
    this.documentsApi.autoGenerateForMultipleDefs(lineDefIds, [clone]).pipe(
      tap((res: EntitiesResponse<LineForSave>) => this.handleAutoGenerateResult(res, doc)),
    ).subscribe({ error: this.details.handleActionError });
  }

  public showAutoGenerateAll = (): boolean => {
    return this.generatableLineDefIdsWithoutRequiredParams().length > 0;
  }

  private generatableLineDefIdsWithoutRequiredParams = (): number[] => {
    // Get the line def Ids that have a generate script and no required params.
    const def = this.definition;
    const lineDefIds = def.LineDefinitions
      .map(e => e.LineDefinitionId)
      .filter(lineDefId => {
        const lineDef = this.lineDefinition(lineDefId);
        return lineDef.GenerateScript &&
          lineDef.GenerateParameters
            .every(e => e.Visibility !== 'Required');
      });

    return lineDefIds;
  }

  private handleAutoGenerateResult = (res: EntitiesResponse<LineForSave>, doc: Document) => {
    if (res.Result.length > 0) {
      // Add related entities to workspace
      mergeEntitiesInWorkspace(res.RelatedEntities, this.workspace);
      this.workspace.notifyStateChanged();

      for (const line of res.Result) {
        line._flags = { isModified: true };
      }

      // Add the new lines to the doc and refresh the grids
      doc.Lines.push(...res.Result);
      this._computeEntriesModel = null;
      this._linesModel = null;
    } else {
      const msg = this.translate.instant('Message_AutoGenerateReturnedNothing');
      this.details.displaySuccessMessage(msg);
    }
  }

  public get autoGenerateRequiredParamsAreSet(): boolean {
    const lineDef = this.autoGenerateLineDef;
    if (!!lineDef && lineDef.GenerateParameters.length > 0) {
      for (const param of lineDef.GenerateParameters) {
        if (param.Visibility === 'Required' && !isSpecified(this.autoGenerateArgs[param.Key])) {
          return false; // At least one required parameter is missing
        }
      }
    }

    return true;
  }

  public total(doc: Document, direction: number, manualOnly = false) {
    direction = direction as 1 | -1; // To avoid an Angular template binding bug

    if (!doc || !doc.Lines) {
      return null;
    }

    let lines = doc.Lines;
    if (manualOnly) {

      const manualId = this.ws.definitions.ManualLinesDefinitionId;
      lines = lines.filter(e => e.DefinitionId === manualId);
    }

    return direction * lines
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

  ////////////////// Barcode stuff

  private barcodeColumnCollection(lineDefId: number): Collection {
    const lineDef = this.lineDefinition(lineDefId);
    const colDef = lineDef.Columns[lineDef.BarcodeColumnIndex];
    if (!!colDef) {
      switch (colDef.ColumnName) {
        case 'AgentId':
        case 'NotedAgentId':
          return 'Agent';

        case 'ResourceId':
        case 'NotedResourceId':
          return 'Resource';
      }
    } else {
      // Server validation should prevent this
      console.error(`BarcodeColumnIndex ${lineDef.BarcodeColumnIndex} is out of range`);
    }
  }

  private barcodeColumnIndex(lineDefId: number): number {
    const lineDef = this.lineDefinition(lineDefId);
    return lineDef.BarcodeColumnIndex;
  }

  private barcodeQuantityColumnIndexDefinition: LineDefinitionForClient;
  private barcodeQuantityColumnIndexResult: number;

  private barcodeQuantityColumnIndex(lineDefId: number): number {
    const lineDef = this.lineDefinition(lineDefId);
    if (this.barcodeQuantityColumnIndexDefinition !== lineDef) {
      this.barcodeQuantityColumnIndexDefinition = lineDef;
      const barcodeColumn = lineDef.Columns[lineDef.BarcodeColumnIndex];
      if (!!barcodeColumn) {
        this.barcodeQuantityColumnIndexResult = lineDef.Columns
          .findIndex(c => c.ColumnName === 'Quantity' && c.EntryIndex === barcodeColumn.EntryIndex);
      } else {
        console.error('Could not find suitable quantity column for barcode');
      }
    }

    return this.barcodeQuantityColumnIndexResult;
  }

  public showBarcodeField(lineDefId: number, isEdit: boolean): boolean {
    if (!isEdit) {
      return false;
    }

    const def = this.lineDefinition(lineDefId);
    return isSpecified(def.BarcodeColumnIndex);
  }

  public barcodeFieldPlaceholder(lineDefId: number): string {
    const columnIndex = this.barcodeColumnIndex(lineDefId);
    const columnLabel = this.columnLabel(lineDefId, columnIndex);
    return this.translate.instant('Scan0', { 0: columnLabel });
  }

  private barcodeModel: Document;
  private barcodeLineDefId: number;
  private barcode: string;

  public getBarcode(lineDefId: number, model: Document): string {
    if (this.barcodeModel !== model || this.barcodeLineDefId !== lineDefId) {
      this.barcodeModel = model;
      this.barcodeLineDefId = lineDefId;

      this.barcode = null;
    }

    return this.barcode;
  }

  public setBarcode(lineDefId: number, model: Document, barcode: string): void {
    if (this.barcodeModel !== model || this.barcodeLineDefId !== lineDefId) {
      this.barcodeModel = model;
      this.barcodeLineDefId = lineDefId;

      this.barcode = null;
    }

    if (this.barcode !== barcode) {
      this.barcode = barcode;
      this.clearBarcodeError();
    }
  }

  // Scroll to a line and make it flash a light green color

  public attentionItem: LineForSave = null;
  private drawAttentionToLine(line: LineForSave, lineIndex: number, table: TableComponent) {
    this.attentionItem = line;
    setTimeout(() => this.attentionItem = null, 250);

    if (lineIndex === null) {
      setTimeout(() => table.scrollToEnd(), 50);
    } else {
      setTimeout(() => table.scrollTo(lineIndex), 50);
    }
  }

  // Barcode error

  private onBarcodeSuccess(lineDef: LineDefinitionForClient): void {
    // The user scanning the items will be more productive if he doesn't have to look at the screen
    // We play a success beep when a scanned barcode is found and processed
    if (lineDef.BarcodeBeepsEnabled) {
      this.audio.beep(100, 920, 150); // high pitch beep to signal a successful scan
    }
  }

  private barcodeErrorsModel: Document;
  private barcodeErrorsLineDefId: number;
  private barcodeError: () => string;

  public getBarcodeError(lineDefId: number, model: Document): string {
    if (this.barcodeErrorsModel !== model || this.barcodeErrorsLineDefId !== lineDefId) {
      this.barcodeErrorsModel = model;
      this.barcodeErrorsLineDefId = lineDefId;

      this.clearBarcodeError();
    }

    if (!!this.barcodeError) {
      return this.barcodeError();
    } else {
      return null;
    }
  }

  public showBarcodeError(lineDefId: number, model: Document): boolean {
    if (this.barcodeErrorsModel !== model || this.barcodeErrorsLineDefId !== lineDefId) {
      this.barcodeErrorsModel = model;
      this.barcodeErrorsLineDefId = lineDefId;

      this.clearBarcodeError();
    }

    return !!this.barcodeError;
  }

  private onBarcodeError(lineDef: LineDefinitionForClient, errorFunc: () => string) {
    this.barcodeError = errorFunc;

    // The user scanning the items will be more productive if she doesn't have to look at the screen
    // We play a distinctive error beep when a problem occurs
    if (lineDef.BarcodeBeepsEnabled) {
      this.audio.beep(999, 150, 300); // low pitch beep to signal a failed scan
    }
  }

  public clearBarcodeError() {
    this.barcodeError = null;
  }

  // These functions handle when users scan the same barcode multiple times before the first scan has loaded

  private _barcodeScanCountModel: Document;
  private _barcodeScanCount: { [key: string]: number } = {};

  private getBarcodeScanCount(lineDefId: number, model: Document, barcode: string): number {
    if (this._barcodeScanCountModel !== model) {
      this._barcodeScanCountModel = model;
      this._barcodeScanCount = {};
    }

    const key = `${lineDefId}/${barcode}`;
    return this._barcodeScanCount[key] || 0;
  }

  private incrementBarcodeScanCount(lineDefId: number, model: Document, barcode: string): void {
    if (this._barcodeScanCountModel !== model) {
      this._barcodeScanCountModel = model;
      this._barcodeScanCount = {};
    }

    // To handle subsequent scans of the same barcode
    const key = `${lineDefId}/${barcode}`;
    if (!this._barcodeScanCount[key]) {
      this._barcodeScanCount[key] = 1;
    } else {
      this._barcodeScanCount[key] = this._barcodeScanCount[key] + 1;
    }
  }

  private clearBarcodeScanCount(lineDefId: number, model: Document, barcode: string): number {
    if (this._barcodeScanCountModel !== model) {
      this._barcodeScanCountModel = model;
      this._barcodeScanCount = {};
    }

    const key = `${lineDefId}/${barcode}`;
    const n = this._barcodeScanCount[key];
    delete this._barcodeScanCount[key];
    return n;
  }

  // The barcode loading spinner
  private _isBarcodeLoadingModel: Document;
  public _isBarcodeLoading: { [lineDefId: number]: number } = {};

  public isBarcodeLoading(lineDefId: number, model: Document): boolean {
    if (this._isBarcodeLoadingModel !== model) {
      this._isBarcodeLoadingModel = model;
      this._isBarcodeLoading = {};
    }

    return this._isBarcodeLoading[lineDefId] > 0;
  }

  private onBarcodeStartLoading(lineDefId: number, model: Document, barcode: string) {
    if (this._isBarcodeLoadingModel !== model) {
      this._isBarcodeLoadingModel = model;
      this._isBarcodeLoading = {};
    }

    // To show the spinner
    if (!this._isBarcodeLoading[lineDefId]) {
      this._isBarcodeLoading[lineDefId] = 1;
    } else {
      this._isBarcodeLoading[lineDefId] = this._isBarcodeLoading[lineDefId] + 1;
    }

    this.incrementBarcodeScanCount(lineDefId, model, barcode);
  }

  private onBarcodeFinishedLoading(lineDefId: number, model: Document, barcode: string): number {
    if (this._isBarcodeLoadingModel !== model) {
      this._isBarcodeLoadingModel = model;
      this._isBarcodeLoading = {};
    }

    // To hide the spinner
    if (!!this._isBarcodeLoading[lineDefId]) {
      this._isBarcodeLoading[lineDefId] = Math.max(0, this._isBarcodeLoading[lineDefId] - 1);
    }

    return this.clearBarcodeScanCount(lineDefId, model, barcode);
  }

  public onBarcodeEnter(lineDefId: number, model: Document, table: TableComponent): void {
    const barcode = this.getBarcode(lineDefId, model);
    if (!!barcode) {
      // Empty the barcode field in preparation for the next scan
      this.setBarcode(lineDefId, model, null);

      // Calculate some basics
      const lineDef = this.lineDefinition(lineDefId);
      const barcodeColumnCollection = this.barcodeColumnCollection(lineDefId); // The column index of the barcoded column;
      const barcodeColumnIndex = this.barcodeColumnIndex(lineDefId); // The column index of the barcoded column;
      const barcodeProperty = lineDef.BarcodeProperty; // The column index of the barcoded column;

      // This function handles the situation when a line is found that already has a record with the scanned barcode
      const handleExistingLine = (existingLine: LineForSave, existingLineIndex: number) => {
        if (lineDef.BarcodeExistingItemHandling === 'IncrementQuantity') {
          // Increment Quantity column by 1
          const quantityColumnIndex = this.barcodeQuantityColumnIndex(lineDefId);
          const quantity = this.getFieldValue(lineDefId, quantityColumnIndex, existingLine, model) || 0;
          this.setFieldValue(lineDefId, quantityColumnIndex, existingLine, model, quantity + 1);

          this.drawAttentionToLine(existingLine, existingLineIndex, table);
          this.onBarcodeSuccess(lineDef);

        } else if (lineDef.BarcodeExistingItemHandling === 'AddNewLine') {
          // Add a new line with the same record Id
          const newLine = this.onAddNewLine(lineDefId, model);
          const barcodeRecordId = this.getFieldValue(lineDefId, barcodeColumnIndex, existingLine, model);
          this.setFieldValue(lineDefId, barcodeColumnIndex, newLine, model, barcodeRecordId);

          this.drawAttentionToLine(newLine, null, table);
          this.onBarcodeSuccess(lineDef);

        } else if (lineDef.BarcodeExistingItemHandling === 'ThrowError') {
          this.drawAttentionToLine(existingLine, existingLineIndex, table);

          const errorFunc = () => this.translate.instant('Error_Barcode0AlreadyAdded', { 0: barcode });
          this.onBarcodeError(lineDef, errorFunc);
        } else if (lineDef.BarcodeExistingItemHandling === 'DoNothing') {
          this.drawAttentionToLine(existingLine, existingLineIndex, table);
          this.onBarcodeSuccess(lineDef);

        } else {
          // Future proofing
          console.error(`Unknown value '${lineDef.BarcodeExistingItemHandling}' for BarcodeExistingItemHandling`);
        }
      };

      // (1) Try to find a line that already has a record with that barcode
      const lines = this.lines(lineDefId, model);
      const matchingLineIndex = lines.findIndex(line => {
        const barcodeRecordId = this.getFieldValue(lineDefId, barcodeColumnIndex, line, model);
        const barcodeRecord = this.ws.get(barcodeColumnCollection, barcodeRecordId);
        return !!barcodeRecord && barcodeRecord[barcodeProperty] === barcode;
      });

      const matchingLine = lines[matchingLineIndex];
      if (!!matchingLine) {
        handleExistingLine(matchingLine, matchingLineIndex);
      } else if (this.getBarcodeScanCount(lineDefId, model, barcode) > 0) {
        this.incrementBarcodeScanCount(lineDefId, model, barcode);
      } else {
        // Calculate select
        let defIds: number[];
        let defId: number; // Single or default
        let desc: EntityDescriptor;
        let select: string;

        const colDef = lineDef.Columns[lineDef.BarcodeColumnIndex];
        switch (colDef.ColumnName) {
          case 'AgentId':
            defIds = this.definitionIdsAgent_Smart(lineDefId, barcodeColumnIndex);
            defId = !!defIds && defIds.length === 1 ? defIds[0] : null;
            desc = metadata_Agent(this.workspace, this.translate, defId);
            select = desc.select + ',DefinitionId,' + this.additionalSelectAgent_Smart(lineDefId);
            break;
          case 'NotedAgentId':
            defIds = this.definitionIdsNotedAgent_Smart(lineDefId, barcodeColumnIndex);
            defId = !!defIds && defIds.length === 1 ? defIds[0] : null;
            desc = metadata_Agent(this.workspace, this.translate, defId);
            select = desc.select + ',DefinitionId,' + this.additionalSelectNotedAgent_Smart(lineDefId);
            break;
          case 'ResourceId':
            defIds = this.definitionIdsResource_Smart(lineDefId, barcodeColumnIndex);
            defId = !!defIds && defIds.length === 1 ? defIds[0] : null;
            desc = metadata_Resource(this.workspace, this.translate, defId);
            select = desc.select + ',DefinitionId,' + this.additionalSelectResource_Smart(lineDefId);
            break;
          case 'NotedResourceId':
            defIds = this.definitionIdsNotedResource_Smart(lineDefId, barcodeColumnIndex);
            defId = !!defIds && defIds.length === 1 ? defIds[0] : null;
            desc = metadata_Resource(this.workspace, this.translate, defId);
            select = desc.select + ',DefinitionId,' + this.additionalSelectNotedResource_Smart(lineDefId);
            break;
        }

        // Calculate filter
        const barcodePropDescriptor = desc.properties[barcodeProperty];
        if (!barcodePropDescriptor) {
          // Server validation should prevent this
          console.error(`Barcode Property '${barcodeProperty}' could not be found`);
          return;
        }

        const barcodeForFilter = isText(barcodePropDescriptor) ? `'${barcode.replace(`'`, `''`)}'` : barcode.toString();
        const barcodeFilter = `${barcodeProperty} eq ${barcodeForFilter}`;
        const activeFilter = ' and IsActive eq true';
        const baseFilter = this.getFilter(lineDefId, barcodeColumnIndex);
        const defFilter = defIds.length > 1 ? defIds.map(e => `DefinitionId eq ${e}`).reduce((a, b) => `${a} or ${b}`) : null;
        const filter = barcodeFilter + activeFilter +
          (!!baseFilter ? ` and (${baseFilter})` : '') +
          (!!defFilter ? ` and (${defFilter})` : '');

        // Prepare arguments
        const args: GetArguments = {
          select,
          filter,
          top: 2,
          skip: 0,
        };

        this.onBarcodeStartLoading(lineDefId, model, barcode);
        this.api.crudFactory(desc.apiEndpoint, this.notifyDestruct$).getEntities(args).pipe(
          tap(results => {
            // Hide the spinner and get the number of same scans that happaned while this barcode was loading
            const scanCount = this.onBarcodeFinishedLoading(lineDefId, model, barcode);
            if (!!results && !!results.Result && results.Result.length > 0) {
              if (results.Result.length > 1) {
                // The barcode is not uniquegetBarcodeError
                const errorFunc = () => this.translate.instant('Error_MoreThanOneRecordWithBarcode0', { 0: barcode });
                this.onBarcodeError(lineDef, errorFunc);
              } else {
                const recordId = addToWorkspace(results, this.workspace)[0];

                // SQL queries have a more lose sense of string equality, for example N'a' == N'A'
                // Ao we check for barcode equality again to make sure the strings match exactly
                const record = this.ws.get(results.CollectionName, recordId);
                if (record[barcodeProperty] !== barcode) {
                  const errorFunc = () => this.translate.instant('Error_CouldNotFindBarcode0', { 0: barcode });
                  this.onBarcodeError(lineDef, errorFunc);
                } else {
                  // If the user scans an item's barcode, changes its barcode from another browser tab and scans the new barcode
                  // Here we handle this edge case
                  const lines2 = this.lines(lineDefId, model);
                  const matchingLine2Index = lines2.findIndex(line =>
                    recordId === this.getFieldValue(lineDefId, barcodeColumnIndex, line, model));
                  const matchingLine2 = lines2[matchingLine2Index];
                  if (!!matchingLine2) {
                    handleExistingLine(matchingLine2, matchingLine2Index);
                  } else {
                    // All is good: add a new line
                    const newLine = this.onAddNewLine(lineDefId, model);
                    this.setFieldValue(lineDefId, barcodeColumnIndex, newLine, model, recordId);

                    if (lineDef.BarcodeExistingItemHandling === 'IncrementQuantity') {
                      // Set the quantity to 1 if the column is visible
                      const quantityColumnIndex = this.barcodeQuantityColumnIndex(lineDefId);
                      this.setFieldValue(lineDefId, quantityColumnIndex, newLine, model, 1);
                    }

                    // Highlight it
                    this.drawAttentionToLine(newLine, null, table);
                    this.onBarcodeSuccess(lineDef);

                    // If the user scanned the same barcode multiple times during the server call, we handle them here
                    if (scanCount > 1) {
                      // Handle as if it were an existing line
                      for (let i = 1; i < scanCount; i++) {
                        const gapInMs = 160; // Gap between successive handlings, so that successive beeps are heard
                        timer(gapInMs * i).pipe(
                          tap(() => handleExistingLine(newLine, null)),
                          takeUntil(this.notifyDestruct$)
                        ).subscribe();
                      }
                    }
                  }
                }
              }
            } else {
              const errorFunc = () => this.translate.instant('Error_CouldNotFindBarcode0', { 0: barcode });
              this.onBarcodeError(lineDef, errorFunc);
            }
          }),
          catchError(friendlyError => {
            const errorFunc = () => friendlyError.error;
            this.onBarcodeError(lineDef, errorFunc);
            this.onBarcodeFinishedLoading(lineDefId, model, barcode);
            return of(null);
          }),
          takeUntil(this.notifyDestruct$)
        ).subscribe();
      }
    }
  }

  private selectDefinition: DocumentDefinitionForClient;
  private selectResult: string;

  public get select(): string {
    const def = this.definition;
    if (this.selectDefinition !== def) {
      this.selectDefinition = def;

      const tracker = {};

      for (const lineDefId of def.LineDefinitions.map(e => e.LineDefinitionId)) {
        const lineDef = this.ws.definitions.Lines[lineDefId];
        if (isSpecified(lineDef.BarcodeColumnIndex) && !!lineDef.BarcodeProperty) {
          const colDef = lineDef.Columns[lineDef.BarcodeColumnIndex];
          if (!!colDef) {
            switch (colDef.ColumnName) {
              case 'AgentId':
                tracker[`Lines.Entries.Agent.${lineDef.BarcodeProperty}`] = true;
                break;
              case 'NotedAgentId':
                tracker[`Lines.Entries.NotedAgent.${lineDef.BarcodeProperty}`] = true;
                break;
              case 'ResourceId':
                tracker[`Lines.Entries.Resource.${lineDef.BarcodeProperty}`] = true;
                break;
              case 'NotedResourceId':
                tracker[`Lines.Entries.NotedResource.${lineDef.BarcodeProperty}`] = true;
                break;
            }
          }
        }
      }

      // Construct the select result
      let result = this.selectBase;
      for (const s of Object.keys(tracker)) {
        result += ',' + s;
      }

      this.selectResult = result;
    }

    return this.selectResult;
  }

  public additionalSelectAgent_Smart(lineDefId: number): string {
    const lineDef = this.lineDefinition(lineDefId);
    return `CurrencyId,${lineDef.BarcodeProperty || ''}`;
  }

  public additionalSelectNotedAgent_Smart(lineDefId: number): string {
    const lineDef = this.lineDefinition(lineDefId);
    return lineDef.BarcodeProperty || ''; // May need review
  }

  public additionalSelectResource_Smart(lineDefId: number): string {
    const lineDef = this.lineDefinition(lineDefId);
    return `CurrencyId,UnitId,${lineDef.BarcodeProperty || ''}`;
  }

  public additionalSelectNotedResource_Smart(lineDefId: number): string {
    const lineDef = this.lineDefinition(lineDefId);
    return lineDef.BarcodeProperty || ''; // May need review
  }

  public additionalSelectReferenceSource_Smart(lineDefId: number): string {
    return ``;
  }

  // Edit Definitions
  public onEditDefinition = (_: Document) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../document-definitions', this.definitionId], { relativeTo: this.route })
      .then((success: boolean) => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public onEditLineDefinition = (defId: number) => {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../../line-definitions', defId], { relativeTo: this.route })
      .then((success: boolean) => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  public showEditDefinition = (_: Document) => this.ws.canDo('document-definitions', 'Update', null);

  public showEditLineDefinition = () => this.ws.canDo('line-definitions', 'Update', null);

  // Messages

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
        .filter(e => e.Collection === 'Document' && e.DefinitionId === defId && (e.Usage === 'FromDetails' || e.Usage === 'FromSearchAndDetails'));
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
          this.documentsApi.messageCommandPreviewEntities(template.MessageTemplateId, { i: [id] }) :
          this.documentsApi.messageCommandPreviewEntity(id, template.MessageTemplateId, {});
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
          this.documentsApi.messageEntities(template.MessageTemplateId, { i: [id] }, version) :
          this.documentsApi.messageEntity(id, template.MessageTemplateId, {}, version);
      };
    }

    return this._sendMessage;
  }

  // Emails

  private _emailTemplatesDefinitions: DefinitionsForClient;
  private _emailTemplatesDefinitionId: number;
  private _emailTemplatesResult: EmailTemplateForClient[];

  public get emailTemplates(): EmailTemplateForClient[] {
    const defs = this.workspace.currentTenant.definitions;
    const defId = this.definitionId;
    if (this._emailTemplatesDefinitions !== defs ||
      this._emailTemplatesDefinitionId !== defId) {
      this._emailTemplatesDefinitions = defs;
      this._emailTemplatesDefinitionId = defId;

      this._emailTemplatesResult = Object.values(defs.EmailTemplates || {})
        .filter(e => e.Collection === 'Document' && e.DefinitionId === defId && (e.Usage === 'FromDetails' || e.Usage === 'FromSearchAndDetails'));
    }

    return this._emailTemplatesResult;
  }

  private _emailCommandPreviewId: string | number;
  private _emailCommandPreview: (t: EmailTemplateForClient) => Observable<EmailCommandPreview>;

  public emailCommandPreviewFactory(id: number) {
    if (!id) {
      delete this._emailCommandPreviewId;
      delete this._emailCommandPreview;
    } else if (this._emailCommandPreviewId !== id) {
      this._emailCommandPreviewId = id;
      this._emailCommandPreview = (template: EmailTemplateForClient) => {
        return template.Usage === 'FromSearchAndDetails' ?
          this.documentsApi.emailCommandPreviewEntities(template.EmailTemplateId, { i: [id] }) :
          this.documentsApi.emailCommandPreviewEntity(id, template.EmailTemplateId, {});
      };
    }

    return this._emailCommandPreview;
  }

  private _emailPreviewId: string | number;
  private _emailPreview: (t: EmailTemplateForClient, i: number) => Observable<EmailPreview>;

  public emailPreviewFactory(id: number) {
    if (!id) {
      delete this._emailPreviewId;
      delete this._emailPreview;
    } else if (this._emailPreviewId !== id) {
      this._emailPreviewId = id;
      this._emailPreview = (template: EmailTemplateForClient, index: number, version?: string) => {
        return template.Usage === 'FromSearchAndDetails' ?
          this.documentsApi.emailPreviewEntities(template.EmailTemplateId, index, { i: [id], version }) :
          this.documentsApi.emailPreviewEntity(id, template.EmailTemplateId, index, { version });
      };
    }

    return this._emailPreview;
  }

  private _sendEmailId: string | number;
  private _sendEmail: (t: EmailTemplateForClient, v?: EmailCommandVersions) => Observable<IdResult>;

  public sendEmailFactory(id: number) {
    if (!id) {
      delete this._sendEmailId;
      delete this._sendEmail;
    } else if (this._sendEmailId !== id) {
      this._sendEmailId = id;

      this._sendEmail = (template: EmailTemplateForClient, version?: EmailCommandVersions) => {
        return template.Usage === 'FromSearchAndDetails' ?
          this.documentsApi.emailEntities(template.EmailTemplateId, { i: [id] }, version) :
          this.documentsApi.emailEntity(id, template.EmailTemplateId, {}, version);
      };
    }

    return this._sendEmail;
  }

  // Smart grid search
  private _isSearchResultFactoryDoc: DocumentForSave;
  private _isSearchResultFactoryResult: (line: LineForSave, term: string) => boolean;
  public isSearchResultFactory: (doc: DocumentForSave) => (line: LineForSave, term: string) => boolean = (doc: DocumentForSave) => {
    if (this._isSearchResultFactoryDoc !== doc) {
      this._isSearchResultFactoryDoc = doc;
      this._isSearchResultFactoryResult = (line: LineForSave, term: string) => {
        if (!line || !term) {
          return false;
        }

        term = term.toLowerCase();
        const lineDef = this.lineDefinition(line.DefinitionId);

        return !!lineDef && lineDef.Columns.some(e => {
          // Do the non strings first

          // (1) If the column inheits from the header, don't include it in the search since it's not visible
          if (e.InheritsFromHeader >= 1) {
            const isCommonPropName = this.isCommonPropertyName(e.ColumnName);
            const tabEntry = this.tabEntry(line.DefinitionId, e, doc);
            const tabIsCommon = !!tabEntry ? tabEntry[isCommonPropName] : true;
            if (tabIsCommon) {
              return false;
            }
            if (e.InheritsFromHeader >= 2) {
              const docIsCommon = doc[isCommonPropName];
              if (docIsCommon) {
                return false;
              }
            }
          }

          // (2) Non-stringifiable columns
          // TODO

          // (3) Stringifiable columns
          let textToSearch: string;
          const lineOrEntry = this.entity(e, line);
          switch (e.ColumnName) {
            case 'AccountId':
            case 'AgentId':
            case 'CenterId':
            case 'EntryTypeId':
            case 'NotedAgentId':
            case 'NotedResourceId':
            case 'ReferenceSourceId':
            case 'ResourceId': {
              const navEntityId = !!lineOrEntry ? lineOrEntry[e.ColumnName] : null;
              let navEntity: any;
              switch (e.ColumnName) {
                case 'AccountId': navEntity = this.ws.Account[navEntityId]; break;
                case 'AgentId': navEntity = this.ws.Agent[navEntityId]; break;
                case 'CenterId': navEntity = this.ws.Center[navEntityId]; break;
                case 'EntryTypeId': navEntity = this.ws.EntryType[navEntityId]; break;
                case 'NotedAgentId': navEntity = this.ws.Agent[navEntityId]; break;
                case 'NotedResourceId': navEntity = this.ws.Resource[navEntityId]; break;
                case 'ReferenceSourceId': navEntity = this.ws.Agent[navEntityId]; break;
                case 'ResourceId': navEntity = this.ws.Resource[navEntityId]; break;
              }

              if (!!navEntity) {
                textToSearch = this.ws.localize(navEntity.Name, navEntity.Name2, navEntity.Name3);
              }

              break;
            }

            case 'ExternalReference':
            case 'InternalReference':
            case 'Memo':
            case 'NotedAgentName':
            case 'Text1':
            case 'Text2': {
              textToSearch = lineOrEntry[e.ColumnName];
            }
          }

          return (textToSearch || '').toLowerCase().includes(term);
        });
      };
    }

    return this._isSearchResultFactoryResult;
  }

  ////////////// Import lines

  public onParseLines(input: HTMLInputElement, lineDefId: number, doc: DocumentForSave, isEdit: boolean): void {
    if (!isEdit || !doc) {
      return; // Can't call the API unless all required params are set
    }

    const files = input.files;
    if (files.length === 0) {
      return;
    }

    const file = files[0];
    input.value = '';

    // Call the API and retrieve the generated lines
    this.documentsApi.parseLines(lineDefId, file).pipe(
      tap((res: EntitiesResponse<LineForSave>) => {

        // Add related entities to workspace
        mergeEntitiesInWorkspace(res.RelatedEntities, this.workspace);
        this.workspace.notifyStateChanged();

        for (const line of res.Result) {
          line._flags = { isModified: true };
        }

        // Add the new lines to the doc and refresh the grids
        doc.Lines.push(...res.Result);
        this._computeEntriesModel = null;
        this._linesModel = null;
      }),
    ).subscribe({ error: this.details.handleActionError });
  }

  public onExportTemplateForLines(lineDefId: number): void {
    const lineDef = this.lineDefinition(lineDefId);
    const title = this.ws.localize(lineDef.TitlePlural, lineDef.TitlePlural2, lineDef.TitlePlural3);
    this.documentsApi.templateForLines(lineDefId)
      .subscribe(blob => downloadBlob(blob, title + '.csv'));
  }
}

interface InputComponent {
  /**
   * Focuses the input
   */
  focus: () => void;

  /**
   * Selects all the text in the input
   */
  select: () => void;
}

interface TableComponent {
  /**
   * Scrolls to the given index
   */
  scrollTo: (index: number) => void;

  /**
   * Scrolls to the end of the table
   */
  scrollToEnd: () => void;
}

interface AttachmentWrapper {
  attachment: Attachment;
  file?: File;
  downloading?: boolean;
  previewing?: boolean;
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
