import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService, TenantWorkspace, MasterDetailsStore } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap, Router, Params } from '@angular/router';
import { DocumentForSave, Document, serialNumber, DocumentClearance, metadata_Document, DocumentState } from '~/app/data/entities/document';
import {
  DocumentDefinitionForClient, ResourceDefinitionForClient,
  LineDefinitionColumnForClient, LineDefinitionEntryForClient
} from '~/app/data/dto/definitions-for-client';
import { LineForSave, Line, LineState } from '~/app/data/entities/line';
import { Entry, EntryForSave } from '~/app/data/entities/entry';
import { DocumentAssignment } from '~/app/data/entities/document-assignment';
import { addToWorkspace, getDataURL, downloadBlob, fileSizeDisplay, mergeEntitiesInWorkspace, toLocalDateISOString } from '~/app/data/util';
import { tap, catchError, finalize, takeUntil, skip } from 'rxjs/operators';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
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

type DocumentDetailsView = 'Managerial' | 'Accounting';
interface LineEntryPair { entry: EntryForSave; line: LineForSave; }

interface DocumentDetailsState {
  tab: string;
  view: DocumentDetailsView;
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

interface DocumentPostingEvent extends DocumentEventBase {
  type: 'posting';
}

interface DocumentCancellationEvent extends DocumentEventBase {
  type: 'cancellation';
}

type DocumentEvent = DocumentReassignmentEvent | DocumentCreationEvent | DocumentPostingEvent | DocumentCancellationEvent;

@Component({
  selector: 't-documents-details',
  templateUrl: './documents-details.component.html',
  styles: []
})
export class DocumentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;
  private _currentDoc: Document;
  private _sortedHistory: { date: string, events: DocumentEvent[] }[] = [];
  private _maxAttachmentSize = 20 * 1024 * 1024;
  private _pristineDocJson: string;
  private localState = new MasterDetailsStore();  // Used in popup mode

  // Caching for required signature functions
  private _requiredSignaturesDetailed: RequiredSignature[];
  private _requiredSignaturesLineIds: number[];
  private _requiredSignaturesSummary: RequiredSignature[];
  private _requiredSignaturesLineIdsHash: HashTable;
  private _requiredSignatureProps = [
    'ToState', 'RuleType', 'RoleId', 'AgentId', 'UserId', 'SignedById', 'SignedAt', 'OnBehalfOfUserId',
    'LastUnsignedState', 'LastNegativeState', 'CanSign', 'ProxyRoleId', 'CanSignOnBehalf',
    'ReasonId', 'ReasonDetails'];

  private _requiredSignaturesForLineDefModel: Document;
  private _requiredSignaturesForLineDefLineDefId: string;
  private _requiredSignaturesForLineDefLineIds: number[];

  // Caching for other functions

  private _computeTabsLines: LineForSave[];
  private _computeTabsDefinitions: DocumentDefinitionForClient;
  private _visibleTabs: string[];
  private _invisibleTabs: string[];

  private _lines: { [key: string]: LineForSave[] };
  private _linesModel: DocumentForSave;

  private _onNewLineFactoryLineDefId: string;
  private _onNewLineFactoryResult: (item: LineForSave) => LineForSave;

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

    // Assignment history
    ['Assignee', 'CreatedBy']
      .map(prop => `AssignmentsHistory/${prop}`).join(',');

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute, private modalService: NgbModal) {
    super();
  }

  ngOnInit() {

    const handleFreshStateFromUrl = (params: ParamMap) => {

      if (this.isScreenMode) {
        // Definitoin Id, must set before retrieving the state
        this.definitionId = params.get('definitionId') || '';

        // When set to true, it means the url is out of step with the state
        let needsUrlStateChange = false;
        const s = this.state.detailsState as DocumentDetailsState;

        // Active tab
        const urlTab = params.get('tab');
        if (!!urlTab) {
          s.tab = urlTab;
        } else if (!!s.tab) { // Prevents infinite loop
          needsUrlStateChange = true;
        }

        const urlView = params.get('view') as DocumentDetailsView;
        if (urlView === 'Managerial' || urlView === 'Accounting') {
          s.view = urlView;
        } else {
          if (!s.view) {
            s.view = 'Managerial'; // Default
          }
          needsUrlStateChange = true;
        }

        // The URL is out of step with the state => sync the two
        // This happens when we navigate to the screen again 2nd time
        if (needsUrlStateChange) {
          this.urlStateChange();
        }
      }
    };

    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.pipe(skip(1)).subscribe(handleFreshStateFromUrl)); // future changes
    handleFreshStateFromUrl(this.route.snapshot.paramMap); // right now
  }

  private urlStateChange(): void {
    if (this.isScreenMode) {
      const params: Params = {
      };

      const s = this.state.detailsState as DocumentDetailsState;
      if (!!s.tab) {
        params.tab = s.tab;
      }

      if (!!s.view) {
        params.view = s.view;
      }

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  public setActiveTab(newTab: string) {
    (this.state.detailsState as DocumentDetailsState).tab = newTab;
    this.urlStateChange();
  }

  public getActiveTab(model: Document): string {
    // Special tabs, TODO: Remove
    const s = this.state.detailsState as DocumentDetailsState;
    if (s.tab === 'Attachment') {
      return 'Attachment';
    }

    if (this.isAccounting) {
      // Accounting view only has 1 tab
      return '_Entries';
    } else {
      // Managerial view can have multiple tabs, make sure the selected one is visible
      const visibleTabs = this.visibleTabs(model);
      if (visibleTabs.some(e => e === s.tab)) {
        return s.tab;
      } else {
        // Get the first visible tab
        return visibleTabs[0];
      }
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
      result.MemoIsCommon = true;
      result.Memo = this.initialText;
      result.PostingDate = toLocalDateISOString(new Date());
    } else {
      const def = this.definition;
      if (!def.HasWorkflow) {
        result.PostingDate = toLocalDateISOString(new Date());
      }

      result.MemoIsCommon = false; // TODO
      if (result.MemoIsCommon) {
        result.Memo = this.initialText;
      }
      result.AgentIsCommon = false;
      result.InvestmentCenterIsCommon = false;
      result.Time1IsCommon = false;
      result.Time2IsCommon = false;
      result.QuantityIsCommon = false;
      result.UnitIsCommon = false;
      result.CurrencyIsCommon = false;

      // result.AgentIsCommon = !!def.AgentDefinitionId;
      // result.InvestmentCenterIsCommon = !!def.InvestmentCenterVisibility;
      // result.Time1IsCommon = !!def.Time1Visibility;
      // result.Time2IsCommon = !!def.Time2Visibility;
      // result.QuantityIsCommon = !!def.QuantityVisibility;
      // result.UnitIsCommon = !!def.UnitVisibility;
      // result.CurrencyIsCommon = !!def.CurrencyVisibility;
    }

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

      clone.Attachments = [];
      clone.AssignmentsHistory = [];

      delete clone.AssigneeId;
      delete clone.CreatedById;
      delete clone.ModifiedById;
      delete clone.SerialNumber;
      delete clone.State;
      delete clone.PostingState;
      delete clone.PostingStateAt;

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
    return serialNumber(serial, def.Prefix, def.CodeWidth || 4);
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

    if (model.PostingState === 1) {
      return 'Error_UnpostDocumentBeforeEdit';
    }

    if (model.PostingState === -1) {
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

      // if (!!model.PostingStateById && !!model.PostingStateAt) {
      //   if (model.PostingState === 1) {
      //     mappedHistory.push({ type: 'posting', userId: model.PostingStateById, time: model.PostingStateAt });
      //   }
      //   if (model.PostingState === -1) {
      //     mappedHistory.push({ type: 'cancellation', userId: model.PostingStateById, time: model.PostingStateAt });
      //   }
      // }

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
    return this.workspace.ws.isRtl ? 'mr-auto' : 'ml-auto';
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

  // AgentId

  public showDocumentAgent(_: DocumentForSave): boolean {
    return !!this.definition.AgentDefinitionId;
  }

  public requireDocumentAgent(_: DocumentForSave): boolean {
    return true;
  }

  public labelDocumentAgent(_: DocumentForSave): string {
    let label = this.ws.getMultilingualValueImmediate(this.definition, 'AgentLabel');
    if (!label) {
      const agentDefId = this.definition.AgentDefinitionId;
      const agentDef = this.ws.definitions.Agents[agentDefId];
      if (!!agentDef) {
        label = this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular');
      } else {
        label = this.translate.instant('Agent');
      }
    }

    return label;
  }

  public documentAgentDefinitionIds(_: DocumentForSave): string[] {
    return [this.definition.AgentDefinitionId];
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
    return !!account && !!account.AgentDefinitionId;
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
    const agentDefinitionId = !!account ? account.AgentDefinitionId : null;

    return metadata_Agent(this.workspace, this.translate, agentDefinitionId).titleSingular();
  }

  // ResourceId

  public showResource(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account && account.HasResource;
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
    return !!account ? account.HasExternalReference : false;
  }

  // Additional Reference

  public showAdditionalReference(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasAdditionalReference : false;
  }

  // Noted Agent Id

  public showNotedAgent(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAgentId : false;
  }

  // Noted Agent Name

  public showNotedAgentName(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAgentName : false;
  }

  // Noted Amount
  public showNotedAmount(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedAmount : false;
  }

  // Noted Date
  public showNotedDate(entry: Entry): boolean {
    const account = this.account(entry);
    return !!account ? account.HasNotedDate : false;
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

  public get extraParams(): { [key: string]: any } {
    return { includeRequiredSignatures: true };
  }

  public handleFreshExtras(extras: { [key: string]: any }) {
    if (!!extras) {
      const relatedEntities = extras.RequiredSignaturesRelatedEntities as ({ [key: string]: EntityWithKey[] });
      if (!!relatedEntities) {
        mergeEntitiesInWorkspace(relatedEntities, this.workspace);
      }
    }
  }
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
    return !!model ? !!model.PostingState : true;
  }

  public unsignTooltip(_: RequiredSignature, model: Document) {
    if (!model) {
      return null;
    } else if (model.PostingState === 1) {
      return this.translate.instant('Error_UnpostDocumentBeforeEdit');
    } else if (model.PostingState === -1) {
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
      return this.translate.instant('Document_State_' + toState);
    } else {
      return this.translate.instant('Document_State_minus_' + (-toState));
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

    return model.PostingState === -1 ||
      model.PostingState === 1 ||
      this.isTooEarlyForThisSignature(signature) ||
      this.areNegativeLines(signature);
  }

  public signTooltip(signature: RequiredSignature, lineDefId: string, model: Document) {
    if (!model) {
      return null;
    } else if (model.PostingState === 1) {
      return this.translate.instant('Error_UnpostDocumentBeforeEdit');
    } else if (model.PostingState === -1) {
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
      const lineDefinitions = this.ws.definitions.Documents[this.definitionId].LineDefinitions;

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

  public lines(lineDefId: string, model: Document): LineForSave[] {
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

  // WIP


  public onInsertManualEntry(pair: LineEntryPair, model: Document): void {
    // Called when the user inserts a new entry
    model.Lines.push(pair.line);
  }

  public onDeleteManualEntry(pair: LineEntryPair, model: Document): void {
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
      Entries: [pair.entry]
    };

    return pair;
  }

  // tslint:disable:member-ordering
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

  public smartColumnPaths(lineDefId: string, model: DocumentForSave): string[] {
    // All line definitions other than 'ManualLine'
    const lineDef = this.ws.definitions.Lines[lineDefId];
    const isMultiRS = this.ws.settings.IsMultiCenter;
    return !!lineDef && !!lineDef.Columns ? lineDef.Columns
      .filter(e => isMultiRS || e.ColumnName !== 'CenterId') // Hide Center columns when there is only one
      .map((_, index) => index + '') : [];
  }

  public manualColumnPaths(model: DocumentForSave): string[] {
    const paths = ['AccountId', 'Debit', 'Credit'];

    if (this.ws.settings.IsMultiCenter) {
      paths.splice(1, 0, 'Center');
    }

    if (!model.MemoIsCommon || this.definitionId !== 'manual-journal-vouchers') {
      paths.push('Memo');
    }

    return paths;
  }

  public columnTemplates(lineDefId: string, model: DocumentForSave, header: TemplateRef<any>, row: TemplateRef<any>): {
    [index: string]: {
      headerTemplate: TemplateRef<any>,
      rowTemplate: TemplateRef<any>,
      weight: number,
      argument: number
    }
  } {

    const templates: {
      [index: string]: {
        headerTemplate: TemplateRef<any>,
        rowTemplate: TemplateRef<any>,
        weight: number,
        argument: number
      }
    } = {};

    // Add as many templates as there are columns
    const columnCount = this.smartColumnPaths(lineDefId, model).length;
    for (let colIndex = 0; colIndex < columnCount; colIndex++) {
      templates[colIndex + ''] = {
        headerTemplate: header,
        rowTemplate: row,
        weight: 1,
        argument: colIndex
      };
    }

    return templates;
  }

  private lineDefinition(lineDefId: string) {
    const lineDef = !!lineDefId ? this.ws.definitions.Lines[lineDefId] : null;
    return lineDef;
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
    return (line.State || 0) >= colDef.ReadOnlyState;
  }

  public isRequired(lineDefId: string, columnIndex: number, line: Line) {
    const colDef = this.columnDefinition(lineDefId, columnIndex);
    return (line.State || 0) >= colDef.RequiredState;
  }

  public onNewSmartLineFactory(lineDefId: string): (item: LineForSave) => LineForSave {
    if (this._onNewLineFactoryLineDefId !== lineDefId) {
      this._onNewLineFactoryLineDefId = lineDefId;
      this._onNewLineFactoryResult = (item) => {
        // set the definition Id
        item.DefinitionId = lineDefId;
        // Add the specified number of entries
        item.Entries = [];
        const lineDef = this.lineDefinition(lineDefId);
        if (!!lineDef) {
          if (lineDef.Entries) {
            for (let i = 0; i < lineDef.Entries.length; i++) {
              const entryDef = lineDef.Entries[i];
              item.Entries[i] = { Direction: entryDef.Direction };
            }
          } else {
            console.error(`Line definition ${lineDefId} is missing its Entries`);
          }
        } else {
          console.error(`Missing line definition ${lineDefId}`);
        }

        return item;
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

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  // Serial Number
  public get isOriginalDocument(): boolean {
    return this.definition.IsOriginalDocument;
  }

  // To work around a bug in Angular compiler
  public isNegativeStateActive = (state: LineState, model: Document) =>
    this.isStateActive(-state as LineState, model)

  // To work around a bug in Angular compiler
  public isNegativePostingStateActive = (state: DocumentState, model: Document) =>
    this.isPostingStateActive(-state as DocumentState, model)

  // The state chart
  public isStateActive(state: LineState, model: Document): boolean {
    if (!model) {
      return false;
    }

    const def = this.definition;
    if (!def) {
      return false;
    }

    return !model.PostingState && def.HasWorkflow && (model.State || 0) === state;
  }

  public isPostingStateActive(state: DocumentState, model: Document): boolean {
    if (!model) {
      return false;
    }

    const def = this.definition;
    if (!def) {
      return false;
    }

    if (state === 0) { // Current
      return !model.PostingState && !def.HasWorkflow;
    } else { // Posted + Canceled
      return model.PostingState === state;
    }
  }

  public isStateVisible(state: LineState, model: Document): boolean {
    // Returns if a positive state is visible on the wide screen flow chart
    if (!!model && (model.PostingState < 0 || model.State < 0)) { // <-- Review
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

  public isPostingStateVisible(state: DocumentState, _: Document): boolean {
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
    const postingStates: DocumentState[] = [0, 1];

    return states.some(state => this.isStateActive(state, model)) ||
      postingStates.some(state => this.isPostingStateActive(state, model));

    //// !!model && ((model.State < 0 && model.PostingState <= 0) || model.PostingState === -1);
  }

  ////////////// Posting State

  public onPostingState(
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
    this.onPostingState(doc, this.documentsApi.post);
  }

  public onUnpost(doc: Document): void {
    this.onPostingState(doc, this.documentsApi.unpost);
  }

  public onCancel(doc: Document): void {
    this.onPostingState(doc, this.documentsApi.cancel);
  }

  public onUncancel(doc: Document): void {
    this.onPostingState(doc, this.documentsApi.uncancel);
  }

  public hasPermissionToUpdateState(doc: Document): boolean {
    return this.ws.canDo(this.view, 'PostingState', !!doc ? doc.CreatedById : null);
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
    return !!doc && !doc.PostingState;
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
    return !!doc && !doc.PostingState;
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
    return !!doc && !!doc.Id && doc.PostingState === 1;
  }

  public showUncancel(doc: Document, _: RequiredSignature[]): boolean {
    return !!doc && !!doc.Id && doc.PostingState === -1;
  }

  public postingStateTooltip(doc: Document): string {
    return this.hasPermissionToUpdateState(doc) ? null : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  ////////////// Accounting vs. Managerial

  public get isManagerial(): boolean {
    return (this.state.detailsState as DocumentDetailsState).view === 'Managerial';
  }

  public get isAccounting(): boolean {
    return (this.state.detailsState as DocumentDetailsState).view === 'Accounting';
  }

  public onManagerialView() {
    const s = this.state.detailsState as DocumentDetailsState;
    s.view = 'Managerial';
    this.urlStateChange();
  }

  public onAccountingView() {
    const s = this.state.detailsState as DocumentDetailsState;
    s.view = 'Accounting';
    this.urlStateChange();
  }

  // Accounting View

  public entriesCount(doc: DocumentForSave) {
    return this.smartEntries(doc).length + this.manualEntries(doc).length;
  }

  public get hasManualLines(): boolean {
    return this.ws.definitions.Documents[this.definitionId].LineDefinitions
      .some(e => e.LineDefinitionId === 'ManualLine');
  }
}

/* Rules for showing and hiding chart states

-------- IsActive
[-4] !PostingState and !!CanReachState4 and State === -4
[-3] !PostingState and !!CanReachState4 and State === -3
[-2] !PostingState and !!CanReachState4 and State === -2
[-1] !PostingState and !!CanReachState4 and State === -1
[0] !PostingState and !!CanReachState4 and State === 0
[1] !PostingState and !!CanReachState4 and State === 1
[2] !PostingState and !!CanReachState4 and State === 2
[3] !PostingState and !!CanReachState4 and State === 3
[4] !PostingState and !!CanReachState4 and State === 4
[Current] !PostingState and !CanReachState4
[Posted] PostingState === 1
[Canceled] PostingState === -1

--------- IsVisible (In +ve state and wide screen)
[0] !!CanReachState4
[1] (!!CanReachState4 && CanReachState1) || isActive(1)
[2] (!!CanReachState4 && CanReachState2) || isActive(2)
[3] (!!CanReachState4 && CanReachState3) || isActive(3)
[4] !!CanReachState4 || isActive(4)
[Current] !CanReachState4
[Posted] Always

--------- IsVisible (In -ve state or narrow screen)
IsVisible = IsActive

*/
