// tslint:disable:member-ordering
// tslint:disable:max-line-length
import { Component, OnInit, Input, OnDestroy, ViewChild, TemplateRef, OnChanges, SimpleChanges } from '@angular/core';
import { ActivatedRoute, Router, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, ReportStatus, MAXIMUM_COUNT, StatementStore } from '~/app/data/workspace.service';
import { tap, catchError, switchMap } from 'rxjs/operators';
import { Resource, metadata_Resource } from '~/app/data/entities/resource';
import { Account } from '~/app/data/entities/account';
import { metadata_Agent, Agent } from '~/app/data/entities/agent';
import { AccountType } from '~/app/data/entities/account-type';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { Entity } from '~/app/data/entities/base/entity';
import { DetailsEntry } from '~/app/data/entities/details-entry';
import { formatNumber } from '@angular/common';
import { LineForQuery } from '~/app/data/entities/line';
import { Document, formatSerial, metadata_Document } from '~/app/data/entities/document';
import { SerialPropDescriptor } from '~/app/data/entities/base/metadata';
import { ApiService } from '~/app/data/api.service';
import { FriendlyError, mergeEntitiesInWorkspace, csvPackage, downloadBlob } from '~/app/data/util';
import { toLocalDateTimeISOString } from '~/app/data/date-util';
import { StatementArguments } from '~/app/data/dto/statement-arguments';
import { Currency } from '~/app/data/entities/currency';
import { StatementResponse } from '~/app/data/dto/statement-response';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { ResourceDefinitionForClient, AgentDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { dateFormat } from '~/app/shared/date-format/date-time-format';
import { accountingFormat } from '~/app/shared/accounting/accounting-format';

@Component({
  selector: 't-statement',
  templateUrl: './statement.component.html',
  styles: []
})
export class StatementComponent implements OnInit, OnChanges, OnDestroy {

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.detailsEntriesApi(this.notifyDestruct$); // Only for intellisense

  private get numericKeys(): string[] {
    switch (this.type) {
      case 'account':
        return ['account_id', 'agent_id', 'resource_id', 'noted_agent_id', 'noted_resource_id', 'entry_type_id', 'center_id'];
      case 'agent':
        return ['agent_id', 'account_id', 'resource_id'];
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  private get stringKeys(): string[] {
    switch (this.type) {
      case 'account':
        return ['from_date', 'to_date', 'currency_id'];
      case 'agent':
        return ['from_date', 'to_date'];
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  private get booleanKeys(): string[] {
    switch (this.type) {
      case 'account':
        return ['include_completed'];
      case 'agent':
        return ['include_completed'];
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  public actionErrorMessage: string;

  @Input()
  type: 'account' | 'agent';

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  /**
   * For agent statement
   */
  private definitionId: number;

  private get agentDefinition(): AgentDefinitionForClient {
    return this.ws.definitions.Agents[this.definitionId];
  }

  constructor(
    private route: ActivatedRoute, private router: Router, private customUserSettings: CustomUserSettingsService,
    private translate: TranslateService, private workspace: WorkspaceService, private apiService: ApiService,
    private modalService: NgbModal) { }

  ngOnInit(): void {

    // Initialize the api service
    this.api = this.apiService.detailsEntriesApi(this.notifyDestruct$);

    // Set up all the subscriptions
    this._subscriptions = new Subscription();

    // Subscribe to fetch requests
    this._subscriptions.add(this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe());

    // Subscribe to changing URL param
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {

      // Copy all report arguments from URL

      let fetchIsNeeded = false;
      const s = this.state;
      const args = s.arguments;

      for (const key of this.stringKeys) {
        const paramValue = params.get(key) || undefined;
        if (args[key] !== paramValue) {
          args[key] = paramValue;
          fetchIsNeeded = true;
        }
      }

      for (const key of this.numericKeys) {
        const paramValue = (+params.get(key)) || undefined;
        if (args[key] !== paramValue) {
          args[key] = paramValue;
          fetchIsNeeded = true;
        }
      }

      for (const key of this.booleanKeys) {
        const paramValue: boolean = (params.get(key) || false).toString() === 'true';
        if (args[key] !== paramValue) {
          args[key] = paramValue;
          fetchIsNeeded = true;
        }
      }

      switch (this.type) {
        case 'account':
          break;
        case 'agent':
          const defId = +params.get('definitionId');
          if (!!defId && !!this.ws.definitions.Agents[defId]) {
            this.definitionId = defId;
          }
          break;
        default:
          console.error(`Unhandled report type ${this.type}`); // Future proofing
          break;
      }

      // Other screen parameters
      // Skip
      const skipParam = +params.get('skip') || 0;
      if (s.skip !== skipParam) {
        s.skip = skipParam;
        fetchIsNeeded = true;
      }

      // Collapse Parameters
      const collapseParamsValue = (params.get('collapse_params') || false).toString() === 'true';
      if (this._collapseParameters !== collapseParamsValue) {
        this._collapseParameters = collapseParamsValue;
      }

      if (fetchIsNeeded) {
        this.fetch();
      }
    }));
  }

  ngOnDestroy(): void {
    this.notifyDestruct$.next();
    this._subscriptions.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges) {
    // Using our famous pattern
    const screenDefProperties = [changes.type];
    const screenDefChanges = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());
    if (screenDefChanges) {
      this.ngOnDestroy();
      this.ngOnInit();
    }
  }

  private urlStateChanged(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    const s = this.state;
    const args = s.arguments;
    const params: Params = {};

    // Add the arguments
    for (const key of this.stringKeys.concat(this.numericKeys).concat(this.booleanKeys)) {
      const value = args[key] || undefined;
      if (!!value) {
        params[key] = value;
      }
    }

    // Add skip
    if (!!s.skip) {
      params.skip = s.skip;
    }

    if (!!this._collapseParameters) {
      params.collapse_params = true;
    }

    // navigate to the new url
    this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
  }

  private get argumentsKey(): string {
    switch (this.type) {
      case 'account':
        return 'account-statement/arguments';
      case 'agent':
        return `agent-statement/${this.definitionId}/arguments`;
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  private parametersChanged(): void {

    // Update the URL
    this.urlStateChanged();

    // Force refresh the columns
    this._columnsParametersHaveChanged = true;

    // Save the arguments in user settings
    const argsString = JSON.stringify(this.state.arguments);
    this.customUserSettings.save(this.argumentsKey, argsString);

    // Refresh the results
    this.fetch();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // private get isAccount() {
  //   return this.type === 'account';
  // }

  // private get isAgent() {
  //   return this.type === 'agent';
  // }

  public fetch() {
    this.notifyFetch$.next();
  }

  /**
   * Computes select from all columns, without repetition of select atoms
   */
  private computeSelect(): string {
    const resultHash: { [key: string]: true } = {};
    const resultArray: string[] = [];
    for (const column of this.columns) {
      for (const atom of column.select) {
        if (!resultHash[atom]) {
          resultHash[atom] = true;
          resultArray.push(atom);
        }
      }
    }

    return resultArray.join(',');
  }

  private computeStatementArguments(s?: StatementStore): StatementArguments {
    s = s || this.state;
    const select = this.computeSelect();
    const top = this.DEFAULT_PAGE_SIZE;
    const skip = s.skip;

    // Prepare the query filter
    const args: StatementArguments = {
      select, top, skip,
      fromDate: toLocalDateTimeISOString(new Date(this.fromDate)),
      toDate: toLocalDateTimeISOString(new Date(this.toDate)),
      accountId: this.accountId
    };

    if (this.showAgentParameter) {
      const agentId = this.readonlyAgent_Manual ? this.readonlyValueAgentId_Manual : this.agentId;
      if (!!agentId) {
        args.agentId = agentId;
      }
    }

    if (this.showResourceParameter) {
      const resourceId = this.readonlyResource_Manual ? this.readonlyValueResourceId_Manual : this.resourceId;
      if (!!resourceId) {
        args.resourceId = resourceId;
      }
    }

    if (this.showNotedAgentParameter) {
      const notedAgentId = this.readonlyNotedAgent_Manual ? this.readonlyValueNotedAgentId_Manual : this.notedAgentId;
      if (!!notedAgentId) {
        args.notedAgentId = notedAgentId;
      }
    }

    if (this.showNotedResourceParameter) {
      const notedResourceId = this.readonlyNotedResource_Manual ? this.readonlyValueNotedResourceId_Manual : this.notedResourceId;
      if (!!notedResourceId) {
        args.notedResourceId = notedResourceId;
      }
    }

    if (this.showEntryTypeParameter) {
      const entryTypeId = this.readonlyEntryType_Manual ? this.readonlyValueEntryTypeId_Manual : this.entryTypeId;
      if (!!entryTypeId) {
        args.entryTypeId = entryTypeId;
      }
    }

    if (this.showCenterParameter) {
      const centerId = this.readonlyCenter_Manual ? this.readonlyValueCenterId_Manual : this.centerId;
      if (!!centerId) {
        args.centerId = centerId;
      }
    }

    if (!!this.currencyId && this.showCurrencyParameter) {
      args.currencyId = this.currencyId;
    }

    if (!!this.includeCompleted) {
      args.includeCompleted = true;
    }

    return args;
  }

  private doFetch(): Observable<void> {
    // For robustness grab a reference to the state object, in case it changes later
    const s = this.state;

    if (!this.requiredParametersAreSet) {
      s.reportStatus = ReportStatus.information;
      s.information = () => this.translate.instant('FillRequiredFields');
      return of();
    } else if (this.loadingRequiredParameters) {
      // Wait until required parameters have loaded
      // They will call fetch again once they load
      s.reportStatus = ReportStatus.loading;
      s.result = [];
      return of();
    } else {
      s.reportStatus = ReportStatus.loading;
      s.result = [];

      // Prepare the query params
      const args = this.computeStatementArguments();
      return this.api.statement(args).pipe(
        tap(response => {
          // Result is loaded
          s.reportStatus = ReportStatus.loaded;

          // Add the result to the state
          s.result = response.Result;
          s.top = response.Top;
          s.skip = response.Skip;
          s.total = response.TotalCount;
          s.extras = {
            opening: response.Opening,
            openingQuantity: response.OpeningQuantity,
            openingMonetaryValue: response.OpeningMonetaryValue,
            closing: response.Closing,
            closingQuantity: response.ClosingQuantity,
            closingMonetaryValue: response.ClosingMonetaryValue
          };

          // Merge the related entities and Notify everyone
          mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);
          this.workspace.notifyStateChanged();
        }),
        catchError((friendlyError: FriendlyError) => {
          s.reportStatus = ReportStatus.error;
          s.errorMessage = friendlyError.error;
          return of(null);
        })
      );
    }
  }

  private get requiredParametersAreSet(): boolean {
    const args = this.state.arguments;

    switch (this.type) {
      case 'account':
        return !!args.from_date && !!args.to_date && !!args.account_id;
      case 'agent':
        return !!args.from_date && !!args.to_date && !!args.agent_id && !!args.account_id;
      default:
        console.error(`Unknown report type ${this.type}`); // Future proofing
    }
  }

  private get loadingRequiredParameters(): boolean {
    switch (this.type) {
      case 'account':
        // Some times the account Id or resource Id from the Url refer to entities that are not loaded
        // Given that computing the statement query requires knowledge of these entities (not just their Ids)
        // We have to wait until the details pickers have loaded the entities for us, until then this
        // property returns true, and the statement query is not executed
        if (!!this.accountId && !this.account()) {
          return true;
        }

        if (this.showAgentParameter && !this.readonlyAgent_Manual && !!this.agentId && !this.ws.get('Agent', this.agentId)) {
          return true;
        }

        if (this.showResourceParameter && !this.readonlyResource_Manual && !!this.resourceId && !this.ws.get('Resource', this.resourceId)) {
          return true;
        }

        if (this.showNotedAgentParameter && !this.readonlyNotedAgent_Manual && !!this.notedAgentId && !this.ws.get('Agent', this.notedAgentId)) {
          return true;
        }

        if (this.showNotedResourceParameter && !this.readonlyNotedResource_Manual && !!this.notedResourceId && !this.ws.get('Resource', this.notedResourceId)) {
          return true;
        }

        return false;
      case 'agent':
      // TODO


      default:
        console.error(`Unknown report type ${this.type}`); // Future proofing
    }
  }

  public onParameterLoaded(): void {

    console.log('onParameterLoaded!');
    if (this.state.reportStatus === ReportStatus.loading) {
      this._columnsParametersHaveChanged = true;
      this.fetch(); // ???
    }
  }

  // UI Bindings

  public get found(): boolean {
    switch (this.type) {
      case 'account':
        return true;
      case 'agent':
        return !!this.agentDefinition;
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
        return false;
    }
  }

  public get title() {
    switch (this.type) {
      case 'account':
        return this.translate.instant('AccountStatement');
      case 'agent':
        return this.translate.instant('StatementOf0', { 0: this.ws.getMultilingualValueImmediate(this.agentDefinition, 'TitleSingular') });
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
        return '???';
    }
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public showExportSpinner = false;

  private normalize(arr: string[], length: number): string[] {
    while (arr.length < length) {
      arr.push('');
    }

    return arr;
  }

  private displayErrorModal(error: string) {
    this.actionErrorMessage = error;
    this.modalService.open(this.errorModal);
  }

  public onExport(): void {

    const columns = this.columns;
    const args = this.computeStatementArguments();

    // we export everything
    args.skip = 0;
    args.top = 2147483647 - args.skip; // subtract skip to avoid integer overflow problems on the server

    this.showExportSpinner = true;

    this.api.statement(args).pipe(
      tap((response: StatementResponse) => {
        this.showExportSpinner = false;

        if (response.Result.length !== response.TotalCount) {
          this.displayErrorModal('Too many rows');
          return;
        }

        // Merge the related entities and Notify everyone
        mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);
        this.workspace.notifyStateChanged();

        const data: string[][] = [];

        // (1) Add the parameters
        // data.push(this.normalize([this.translate.instant('FromDate'), toLocalDateOnlyISOString(new Date(args.fromDate))], columns.length));
        // data.push(this.normalize([this.translate.instant('ToDate'), toLocalDateOnlyISOString(new Date(args.toDate))], columns.length));
        data.push(this.normalize([this.translate.instant('FromDate'), dateFormat(args.fromDate, this.workspace, this.translate)], columns.length));
        data.push(this.normalize([this.translate.instant('ToDate'), dateFormat(args.toDate, this.workspace, this.translate)], columns.length));
        data.push(this.normalize([this.translate.instant('Entry_Account'), this.ws.getMultilingualValue('Account', args.accountId, 'Name')], columns.length));
        if (!!args.currencyId) {
          data.push(this.normalize([this.translate.instant('Entry_Currency'), this.ws.getMultilingualValue('Currency', args.currencyId, 'Name')], columns.length));
        }
        if (!!args.agentId) {
          data.push(this.normalize([this.labelAgent_Manual, this.ws.getMultilingualValue('Agent', args.agentId, 'Name')], columns.length));
        }
        if (!!args.resourceId) {
          data.push(this.normalize([this.labelResource_Manual, this.ws.getMultilingualValue('Resource', args.resourceId, 'Name')], columns.length));
        }
        if (!!args.notedAgentId) {
          data.push(this.normalize([this.labelNotedAgent_Manual, this.ws.getMultilingualValue('Agent', args.notedAgentId, 'Name')], columns.length));
        }
        if (!!args.notedResourceId) {
          data.push(this.normalize([this.labelNotedResource_Manual, this.ws.getMultilingualValue('Resource', args.notedResourceId, 'Name')], columns.length));
        }
        if (!!args.entryTypeId) {
          data.push(this.normalize([this.translate.instant('Entry_EntryType'), this.ws.getMultilingualValue('EntryType', args.entryTypeId, 'Name')], columns.length));
        }
        if (!!args.centerId) {
          data.push(this.normalize([this.translate.instant('Entry_Center'), this.ws.getMultilingualValue('Center', args.centerId, 'Name')], columns.length));
        }
        if (!!args.includeCompleted) {
          data.push(this.normalize([this.translate.instant('IncludeCompleted'), this.translate.instant('Yes')], columns.length));
        }

        // Gap between parameters and grid
        data.push(this.normalize([], columns.length));

        // (2) Add column headers
        const headersRow: string[] = [];
        for (const col of columns) {
          headersRow.push(col.label());
        }
        data.push(headersRow);

        // (3) Add the opening row and prepare the closing row
        const openingRow: string[] = [];
        const closingRow: string[] = [];
        for (const col of columns) {
          switch (col.id) {
            case 'PostingDate':
              // openingRow.push(toLocalDateOnlyISOString(new Date(args.fromDate)));
              // closingRow.push(toLocalDateOnlyISOString(new Date(args.toDate)));
              openingRow.push(dateFormat(args.fromDate, this.workspace, this.translate));
              closingRow.push(dateFormat(args.toDate, this.workspace, this.translate));
              break;
            case 'SerialNumber':
              openingRow.push(this.translate.instant('OpeningBalance'));
              closingRow.push(this.translate.instant('ClosingBalance'));
              break;
            case 'QuantityAccumulation':
              openingRow.push(accountingFormat(response.OpeningQuantity, '1.0-4'));
              closingRow.push(accountingFormat(response.ClosingQuantity, '1.0-4'));
              break;
            case 'MonetaryValueAccumulation':
              openingRow.push(accountingFormat(response.OpeningMonetaryValue, this.monetaryValueDigitsInfo));
              closingRow.push(accountingFormat(response.ClosingMonetaryValue, this.monetaryValueDigitsInfo));
              break;
            case 'Accumulation':
              openingRow.push(accountingFormat(response.Opening, this.functionalDigitsInfo));
              closingRow.push(accountingFormat(response.Closing, this.functionalDigitsInfo));
              break;
            default:
              openingRow.push('');
              closingRow.push('');
          }
        }
        data.push(openingRow);

        // (4) Add the movements
        for (const entry of response.Result) {
          const dataRow: string[] = [];
          for (const col of columns) {
            dataRow.push(col.exportDisplay ? col.exportDisplay(entry) : col.display(entry));
          }
          data.push(dataRow);
        }

        // (5) Add the closing row
        data.push(closingRow);

        // Prepare a friendly file name
        const reportName = this.translate.instant('AccountStatement');
        const fileName = `${reportName}.csv`;

        // Download the blob
        const blob = csvPackage(data);
        downloadBlob(blob, fileName);
      }),
      catchError((friendlyError: FriendlyError) => {
        this.showExportSpinner = false;
        this.displayErrorModal(friendlyError.error || friendlyError);
        return of();
      })
    ).subscribe();
  }

  public get canExport(): boolean {
    return this.isLoaded;
  }

  public onRefresh(): void {
    // The if statement to deal with incessant button clickers (Users who hit refresh repeatedly)
    if (this.state.reportStatus !== ReportStatus.loading) {
      this.fetch();
    }
  }

  DEFAULT_PAGE_SIZE = 60;

  public get stateKey(): string {
    switch (this.type) {
      case 'account':
        return 'account-statement';
      case 'agent':
        return `agent-statement/${this.definitionId}`;
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  public get state(): StatementStore {

    const key = this.stateKey;
    const ws = this.workspace.currentTenant;
    if (!ws.statementState[key]) {
      ws.statementState[key] = new StatementStore();
    }

    return ws.statementState[key];
  }

  get from(): number {
    return Math.min(this.state.skip + 1, this.total);
  }

  get to(): number {
    const s = this.state;
    return Math.min(s.skip + this.DEFAULT_PAGE_SIZE, s.total);
  }

  get total(): number {
    return this.state.total;
  }

  get totalDisplay(): string {
    const total = this.total;
    if (total >= MAXIMUM_COUNT) {
      return formatNumber(MAXIMUM_COUNT - 1, 'en-GB') + '+';
    } else {
      return formatNumber(total, 'en-GB');
    }
  }

  onPreviousPage() {
    const s = this.state;
    s.skip = Math.max(s.skip - this.DEFAULT_PAGE_SIZE, 0);

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + this.DEFAULT_PAGE_SIZE;

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  public link(finalSegment: string): string {
    switch (this.type) {
      case 'account':
        return `../${finalSegment}`;
      case 'agent':
        return `../../${finalSegment}`;
      default:
        console.error(`Unhandled report type ${this.type}`); // Future proofing
    }
  }

  // Include Completed
  public get includeCompleted(): boolean {
    return this.state.arguments.include_completed;
  }

  public set includeCompleted(v: boolean) {
    const args = this.state.arguments;
    if (args.include_completed !== v) {
      args.include_completed = v;
      this.parametersChanged();
    }
  }

  // From Date
  public get fromDate(): string {
    return this.state.arguments.from_date;
  }

  public set fromDate(v: string) {
    const args = this.state.arguments;
    if (args.from_date !== v) {
      args.from_date = v;
      this.parametersChanged();
    }
  }

  // To Date
  public get toDate(): string {
    return this.state.arguments.to_date;
  }

  public set toDate(v: string) {
    const args = this.state.arguments;
    if (args.to_date !== v) {
      args.to_date = v;
      this.parametersChanged();
    }
  }

  // Account
  public accountAdditionalSelect = '$DocumentDetails';

  public get accountId(): number {
    return this.state.arguments.account_id;
  }

  public set accountId(v: number) {
    const args = this.state.arguments;
    if (args.account_id !== v) {
      args.account_id = v;
      this.parametersChanged();
    }
  }

  private account(id?: number): Account {
    id = id || this.accountId;
    return this.ws.get('Account', id);
  }

  private accountType(): AccountType {
    const account = this.account();
    if (!!account && account.AccountTypeId) {
      return this.ws.get('AccountType', account.AccountTypeId) as AccountType;
    }

    return null;
  }

  // Currency
  public currencyAdditionalSelect = 'E';

  public get currencyId(): string {
    return this.state.arguments.currency_id;
  }

  public set currencyId(v: string) {
    const args = this.state.arguments;
    if (args.currency_id !== v) {
      args.currency_id = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns the currency Id from the selected account or from the selected resource if any
   */
  private getAccountResourceAgentCurrencyId(): string {
    const account = this.account();
    const resource = this.resource();
    const agent = this.agent();

    const accountCurrencyId = !!account ? account.CurrencyId : null;
    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;
    const agentCurrencyId = !!agent ? agent.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId || agentCurrencyId;
  }

  /**
   * Whether or not to show the currency parameter
   */
  public get showCurrencyParameter(): boolean {
    // Show the editable currency parameter
    const account = this.account();
    return !!account && !this.getAccountResourceAgentCurrencyId();
  }

  /**
   * Returns the Id of the currency to show as a postfix to the monetary value column header
   */
  public get readonlyValueCurrencyId(): string {
    const accountResourceCurrencyId = this.getAccountResourceAgentCurrencyId();
    return accountResourceCurrencyId || this.currencyId;
  }

  /**
   * Syntactic sugar to get the functional currency Id
   */
  public get functionalId(): string {
    return this.ws.settings.FunctionalCurrencyId;
  }

  //////////////////// Agent

  public agentAdditionalSelect = '$DocumentDetails';

  public get agentId(): number {
    return this.state.arguments.agent_id;
  }

  public set agentId(v: number) {
    const args = this.state.arguments;
    if (args.agent_id !== v) {
      args.agent_id = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns any uniquely identifiable agent from the parameters
   */
  private agent(): Agent {
    const account = this.account();
    const accountAgentId = !!account ? account.AgentId : null;
    const paramAgentId = !!account && !!account.AgentDefinitionId ? this.agentId : null;
    const agentId = accountAgentId || paramAgentId;

    return this.ws.get('Agent', agentId) as Agent;
  }

  public get showAgentParameter(): boolean {
    const account = this.account();
    return !!account && !!account.AgentDefinitionId;
  }

  public get readonlyAgent_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.AgentId;
  }

  public get readonlyValueAgentId_Manual(): number {
    const account = this.account();
    return !!account ? account.AgentId : null;
  }

  public get labelAgent_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.AgentDefinitionId : null;

    return metadata_Agent(this.workspace, this.translate, defId).titleSingular();
  }

  public get definitionIdsAgent_Manual(): number[] {
    const account = this.account();
    return [account.AgentDefinitionId];
  }

  public get labelAgent_Smart(): string {
    return this.ws.getMultilingualValueImmediate(this.agentDefinition, 'TitleSingular');
  }

  public get definitionIdsAgent_Smart(): number[] {
    return [this.definitionId];
  }

  //////////////////// Resource

  public resourceAdditionalSelect = '$DocumentDetails';

  public get resourceId(): number {
    return this.state.arguments.resource_id;
  }

  public set resourceId(v: number) {
    const args = this.state.arguments;
    if (args.resource_id !== v) {
      args.resource_id = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns any uniquely identifiable resource from the parameters
   */
  private resource(): Resource {
    const account = this.account();
    const accountResourceId = !!account ? account.ResourceId : null;
    const paramResourceId = !!account && !!account.ResourceDefinitionId ? this.resourceId : null;
    const resourceId = accountResourceId || paramResourceId;

    return this.ws.get('Resource', resourceId) as Resource;
  }

  private resourceDefinition(): ResourceDefinitionForClient {
    const resource = this.resource();
    if (!!resource && !!resource.DefinitionId) {
      return this.ws.definitions.Resources[resource.DefinitionId];
    }
  }

  public get showResourceParameter(): boolean {
    const account = this.account();
    return !!account && !!account.ResourceDefinitionId;
  }

  public get readonlyResource_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.ResourceId;
  }

  public get readonlyValueResourceId_Manual(): number {
    const account = this.account();
    return !!account ? account.ResourceId : null;
  }

  public get labelResource_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.ResourceDefinitionId : null;

    return metadata_Resource(this.workspace, this.translate, defId).titleSingular();
  }

  public get definitionIdsResource_Manual(): number[] {
    const account = this.account();
    return [account.ResourceDefinitionId];
  }

  //////////////////// NotedAgent

  public notedAgentAdditionalSelect = '$DocumentDetails';

  public get notedAgentId(): number {
    return this.state.arguments.noted_agent_id;
  }

  public set notedAgentId(v: number) {
    const args = this.state.arguments;
    if (args.noted_agent_id !== v) {
      args.noted_agent_id = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns any uniquely identifiable agent from the parameters
   */
  private notedAgent(): Agent {
    const account = this.account();
    const accountNotedAgentId = !!account ? account.NotedAgentId : null;
    const paramNotedAgentId = !!account && !!account.NotedAgentDefinitionId ? this.notedAgentId : null;
    const notedAgentId = accountNotedAgentId || paramNotedAgentId;

    return this.ws.get('Agent', notedAgentId) as Agent;
  }

  public get showNotedAgentParameter(): boolean {
    const account = this.account();
    return !!account && !!account.NotedAgentDefinitionId;
  }

  public get readonlyNotedAgent_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.NotedAgentId;
  }

  public get readonlyValueNotedAgentId_Manual(): number {
    const account = this.account();
    return !!account ? account.NotedAgentId : null;
  }

  public get labelNotedAgent_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.NotedAgentDefinitionId : null;

    return metadata_Agent(this.workspace, this.translate, defId).titleSingular();
  }

  public get definitionIdsNotedAgent_Manual(): number[] {
    const account = this.account();
    return [account.NotedAgentDefinitionId];
  }

  //////////////////// NotedResource

  public notedResourceAdditionalSelect = '$DocumentDetails';

  public get notedResourceId(): number {
    return this.state.arguments.noted_resource_id;
  }

  public set notedResourceId(v: number) {
    const args = this.state.arguments;
    if (args.noted_resource_id !== v) {
      args.noted_resource_id = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns any uniquely identifiable resource from the parameters
   */
  private notedResource(): Resource {
    const account = this.account();
    const accountNotedResourceId = !!account ? account.NotedResourceId : null;
    const paramNotedResourceId = !!account && !!account.NotedResourceDefinitionId ? this.notedResourceId : null;
    const notedResourceId = accountNotedResourceId || paramNotedResourceId;

    return this.ws.get('Resource', notedResourceId) as Resource;
  }

  public get showNotedResourceParameter(): boolean {
    const account = this.account();
    return !!account && !!account.NotedResourceDefinitionId;
  }

  public get readonlyNotedResource_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.NotedResourceId;
  }

  public get readonlyValueNotedResourceId_Manual(): number {
    const account = this.account();
    return !!account ? account.NotedResourceId : null;
  }

  public get labelNotedResource_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.NotedResourceDefinitionId : null;

    return metadata_Resource(this.workspace, this.translate, defId).titleSingular();
  }

  public get definitionIdsNotedResource_Manual(): number[] {
    const account = this.account();
    return [account.NotedResourceDefinitionId];
  }

  // Entry Type

  public get entryTypeId(): number {
    return this.state.arguments.entry_type_id;
  }

  public set entryTypeId(v: number) {
    const args = this.state.arguments;
    if (args.entry_type_id !== v) {
      args.entry_type_id = v;
      this.parametersChanged();
    }
  }

  public get showEntryTypeParameter(): boolean {
    // Show entry type when the account's type has an entry type parent Id
    const at = this.accountType();
    if (!!at) {
      const entryTypeParent = this.ws.get('EntryType', at.EntryTypeParentId);
      return !!entryTypeParent && entryTypeParent.IsActive;
    }

    return false;
  }

  public get readonlyEntryType_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.EntryTypeId;
  }

  public get readonlyValueEntryTypeId_Manual(): number {
    const account = this.account();
    return !!account ? account.EntryTypeId : null;
  }

  public get filterEntryType_Manual(): string {
    const accountType = this.accountType();
    return `IsAssignable eq true and Id descof ${accountType.EntryTypeParentId}`;
  }

  // Center

  public get centerId(): number {
    return this.state.arguments.center_id;
  }

  public set centerId(v: number) {
    const args = this.state.arguments;
    if (args.center_id !== v) {
      args.center_id = v;
      this.parametersChanged();
    }
  }

  public get showCenterParameter(): boolean {
    const account = this.account();
    return !!account;
  }

  public get readonlyCenter_Manual(): boolean {
    return !!this.getAccountAgentCenterId();
  }

  public get readonlyValueCenterId_Manual(): number {
    return this.getAccountAgentCenterId();
  }

  /**
   * Returns the center Id from the selected account or from the selected resource if any
   */
  private getAccountAgentCenterId(): number {
    const account = this.account();
    const agent = this.agent();

    const accountCenterId = !!account ? account.CenterId : null;
    const agentCenterId = !!agent ? agent.CenterId : null;

    return accountCenterId || agentCenterId;
  }

  // Error Message
  public get showErrorMessage(): boolean {
    return this.state.reportStatus === ReportStatus.error;
  }

  public get errorMessage(): string {
    return this.state.errorMessage;
  }

  // Information
  public get showInformation(): boolean {
    return this.state.reportStatus === ReportStatus.information;
  }

  public information(): string {
    return this.state.information();
  }

  // Spinner
  public get showSpinner(): boolean {
    return this.state.reportStatus === ReportStatus.loading;
  }

  // Result
  public get showNoItemsFound(): boolean {
    const s = this.state;
    return s.reportStatus === ReportStatus.loaded && (!s.result || s.result.length === 0);
  }

  private get isLoaded(): boolean {
    return this.state.reportStatus === ReportStatus.loaded;
  }

  public get showOpeningBalance(): boolean {
    return this.isLoaded && !this.state.skip;
  }

  public get showClosingBalance(): boolean {
    return this.isLoaded && this.to === this.total;
  }

  private get monetaryValueDigitsInfo(): string {
    const currencyId = this.getAccountResourceAgentCurrencyId() || this.currencyId || this.functionalId;
    const digitsInfo = this.digitsInfo(currencyId);

    return digitsInfo;
  }

  public get openingDisplay(): string {
    const extras = this.state.extras;
    if (extras) {
      const opening = extras.opening || 0;
      return accountingFormat(opening, this.functionalDigitsInfo);
    }

    return '';
  }

  public get openingQuantityDisplay(): string {
    const extras = this.state.extras;
    if (!!extras) {
      const opening = extras.openingQuantity || 0;
      return accountingFormat(opening, '1.0-4');
    }

    return '';
  }

  public get openingMonetaryValueDisplay(): string {
    const extras = this.state.extras;
    if (extras) {
      const opening = extras.openingMonetaryValue || 0;
      return accountingFormat(opening, this.monetaryValueDigitsInfo);
    }

    return '';
  }

  public get closingDisplay(): string {
    const extras = this.state.extras;
    if (extras) {
      const closing = extras.closing || 0;
      return accountingFormat(closing, this.functionalDigitsInfo);
    }

    return '';
  }

  public get closingQuantityDisplay(): string {
    const extras = this.state.extras;
    if (extras) {
      const closing = extras.closingQuantity || 0;
      return accountingFormat(closing, '1.0-4');
    }

    return '';
  }

  public get closingMonetaryValueDisplay(): string {
    const extras = this.state.extras;
    if (extras) {
      const closing = extras.closingMonetaryValue || 0;
      return accountingFormat(closing, this.monetaryValueDigitsInfo);
    }

    return '';
  }

  public get entities(): Entity[] {
    return this.state.result;
  }

  private get functionalDigitsInfo(): string {
    const settings = this.ws.settings;
    const functionalDecimals = settings.FunctionalCurrencyDecimals;
    return `1.${functionalDecimals}-${functionalDecimals}`;
  }

  private digitsInfo(currencyId: string): string {
    const currency = this.ws.get('Currency', currencyId) as Currency;
    if (!currency) {
      return this.functionalDigitsInfo;
    } else {
      const e = currency.E || 0;
      return `1.${e}-${e}`;
    }
  }

  private _columnsAccount: Account;
  private _columnsAccountType: AccountType;
  private _columnsResource: Resource;
  private _columnsSettings: SettingsForClient;
  private _columnsParametersHaveChanged = false;
  private _columns: ColumnInfo[];
  public get columns(): ColumnInfo[] {
    const account = this.account();
    const accountType = this.accountType();
    const resource = this.resource();
    const settings = this.ws.settings;
    if (this._columnsAccount !== account ||
      this._columnsAccountType !== accountType ||
      this._columnsResource !== resource ||
      this._columnsSettings !== settings ||
      this._columnsParametersHaveChanged) {

      this._columnsAccount = account;
      this._columnsAccountType = accountType;
      this._columnsResource = resource;
      this._columnsSettings = settings;
      this._columnsParametersHaveChanged = false;

      this._columns = [
        // PostingDate
        {
          id: 'PostingDate',
          select: ['Line.PostingDate'],
          label: () => this.translate.instant('Line_PostingDate'),
          display: (entry: DetailsEntry) => {
            const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
            return dateFormat(line.PostingDate, this.workspace, this.translate);
          },
          // exportDisplay: (entry: DetailsEntry) => {
          //   const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
          //   return toLocalDateOnlyISOString(new Date(line.PostingDate)); // For Excel to pick it up as a date
          // },
          weight: 1
        },

        // SerialNumber
        {
          id: 'SerialNumber',
          select: ['Line.Document.SerialNumber', 'Line.Document.DefinitionId'],
          label: () => this.translate.instant('Document_SerialNumber'),
          display: (entry: DetailsEntry) => {
            const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
            const doc = this.ws.get('Document', line.DocumentId) as Document;
            const desc = metadata_Document(this.workspace, this.translate, doc.DefinitionId);
            const prop = desc.properties.SerialNumber as SerialPropDescriptor;

            return formatSerial(doc.SerialNumber, prop.prefix, prop.codeWidth);
          },
          weight: 1
        }];

      // Memo
      this._columns.push({
        id: 'Memo',
        select: ['Line.Memo'],
        label: () => this.translate.instant('Memo'),
        display: (entry: DetailsEntry) => {
          const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
          return line.Memo;
        },
        weight: 1
      });

      // EntryType
      if (this.showEntryTypeParameter && !this.readonlyEntryType_Manual && !this.entryTypeId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['EntryType.Name,EntryType.Name2,EntryType.Name3'],
          label: () => this.translate.instant('Entry_EntryType'),
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('EntryType', entry.EntryTypeId, 'Name');
          },
          weight: 1
        });
      }

      // Center
      if (this.showCenterParameter && !this.readonlyCenter_Manual && !this.centerId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['Center.Name,Center.Name2,Center.Name3'],
          label: () => this.translate.instant('Entry_Center'),
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('Center', entry.CenterId, 'Name');
          },
          weight: 1
        });
      }

      // All dynamic properties from account type label
      if (!!accountType) {

        // Time1
        if (!!accountType.Time1Label) {
          this._columns.push({
            select: ['Time1'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'Time1Label'),
            display: (entry: DetailsEntry) => dateFormat(entry.Time1, this.workspace, this.translate),
            // exportDisplay: (entry: DetailsEntry) => !!entry.Time1 ? toLocalDateOnlyISOString(new Date(entry.Time1)) : '',
            weight: 1
          });
        }

        // if (!!accountType.Time1Label && !!accountType.Time2Label) {

        //   // Duration
        //   this._columns.push({
        //     select: ['Duration'],
        //     label: () => this.translate.instant('Entry_Duration'),
        //     display: (entry: DetailsEntry) => accountingFormat(entry.Duration, '1.0-4'),
        //     isRightAligned: true,
        //     weight: 1
        //   });

        //   // Duration Unit
        //   this._columns.push({
        //     select: ['DurationUnit.Name', 'DurationUnit.Name2', 'DurationUnit.Name3'],
        //     label: () => this.translate.instant('Entry_DurationUnit'),
        //     display: (entry: DetailsEntry) => this.ws.getMultilingualValue('Unit', entry.DurationUnitId, 'Name'),
        //     weight: 1
        //   });
        // }

        if (!!accountType.Time2Label) {
          // Time2
          this._columns.push({
            select: ['Time2'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'Time2Label'),
            display: (entry: DetailsEntry) => dateFormat(entry.Time2, this.workspace, this.translate),
            // exportDisplay: (entry: DetailsEntry) => !!entry.Time2 ? toLocalDateOnlyISOString(new Date(entry.Time2)) : '',
            weight: 1
          });
        }

        // ExternalReference
        if (!!accountType.ExternalReferenceLabel) {
          this._columns.push({
            select: ['ExternalReference'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'ExternalReferenceLabel'),
            display: (entry: DetailsEntry) => entry.ExternalReference,
            weight: 1
          });
        }

        // ReferenceSource
        if (!!accountType.ReferenceSourceLabel) {
          this._columns.push({
            select: ['ReferenceSource.Name', 'ReferenceSource.Name2', 'ReferenceSource.Name3'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'ReferenceSourceLabel'),
            display: (entry: DetailsEntry) => this.ws.getMultilingualValue('Agent', entry.ReferenceSourceId, 'Name'),
            weight: 1
          });
        }

        // InternalReference
        if (!!accountType.InternalReferenceLabel) {
          this._columns.push({
            select: ['InternalReference'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'InternalReferenceLabel'),
            display: (entry: DetailsEntry) => entry.InternalReference,
            weight: 1
          });
        }

        // NotedAgentName
        if (!!accountType.NotedAgentNameLabel) {
          this._columns.push({
            select: ['NotedAgentName'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'NotedAgentNameLabel'),
            display: (entry: DetailsEntry) => entry.NotedAgentName,
            weight: 1
          });
        }

        // NotedDate
        if (!!accountType.NotedDateLabel) {
          this._columns.push({
            select: ['NotedDate'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'NotedDateLabel'),
            display: (entry: DetailsEntry) => dateFormat(entry.NotedDate, this.workspace, this.translate),
            // exportDisplay: (entry: DetailsEntry) => !!entry.NotedDate ? toLocalDateOnlyISOString(new Date(entry.NotedDate)) : '',
            weight: 1
          });
        }

        // NotedAmount
        if (!!accountType.NotedAmountLabel) {
          this._columns.push({
            select: ['NotedAmount'],
            label: () => this.ws.getMultilingualValueImmediate(accountType, 'NotedAmountLabel'),
            display: (entry: DetailsEntry) => !!entry.NotedAmount ? accountingFormat(entry.NotedAmount, this.functionalDigitsInfo) : '',
            weight: 1
          });
        }
      }

      // Agent
      if (this.showAgentParameter && !this.readonlyAgent_Manual && !this.agentId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['Agent.Name,Agent.Name2,Agent.Name3'],
          label: () => this.labelAgent_Manual,
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('Agent', entry.AgentId, 'Name');
          },
          weight: 1
        });
      }

      // Resource
      if (this.showResourceParameter && !this.readonlyResource_Manual && !this.resourceId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['Resource.Name,Resource.Name2,Resource.Name3'],
          label: () => this.labelResource_Manual,
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('Resource', entry.ResourceId, 'Name');
          },
          weight: 1
        });
      }

      // NotedAgent
      if (this.showNotedAgentParameter && !this.readonlyNotedAgent_Manual && !this.notedAgentId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['NotedAgent.Name,NotedAgent.Name2,NotedAgent.Name3'],
          label: () => this.labelNotedAgent_Manual,
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('Agent', entry.NotedAgentId, 'Name');
          },
          weight: 1
        });
      }

      // NotedResource
      if (this.showNotedResourceParameter && !this.readonlyNotedResource_Manual && !this.notedResourceId) {
        // If a parameter is visible, editable and not selected yet, show it as a column below
        this._columns.push({
          select: ['NotedResource.Name,NotedResource.Name2,NotedResource.Name3'],
          label: () => this.labelNotedResource_Manual,
          display: (entry: DetailsEntry) => {
            return this.ws.getMultilingualValue('Resource', entry.NotedResourceId, 'Name');
          },
          weight: 1
        });
      }

      // Quantity + Unit
      if (this.showResourceParameter) {
        // Determine whether the resource specifies a single well defined unit
        const resourceDef = this.resourceDefinition();
        const singleUnitDefined = !!resourceDef && resourceDef.UnitCardinality === 'Single' && !!resource && !!resource.UnitId && !!accountType && !accountType.StandardAndPure;
        const singleUnitId = singleUnitDefined ? resource.UnitId : null;

        const baseUnitDefined = !!resourceDef && !!resourceDef.UnitCardinality && !!resource && !!resource.UnitId && !!accountType && !accountType.StandardAndPure;
        const baseUnitId = baseUnitDefined ? resource.UnitId : null;

        this._columns.push({
          select: ['Direction', 'Quantity'],
          label: () => {
            let label = this.translate.instant('Entry_Quantity');
            if (singleUnitDefined) {
              const unitId = singleUnitId;
              label = `${label} (${this.ws.getMultilingualValue('Unit', unitId, 'Name')})`;
            }
            return label;
          },
          display: (entry: DetailsEntry) => {
            return accountingFormat(entry.Direction * entry.Quantity, '1.0-4');
          },
          isRightAligned: true,
          weight: 1
        });

        // If a single unit is not defined, add a column to show the unit per row
        if (!singleUnitDefined) {
          this._columns.push({
            select: ['Unit.Name', 'Unit.Name2', 'Unit.Name3'],
            label: () => this.translate.instant('Entry_Unit'),
            display: (entry: DetailsEntry) => this.ws.getMultilingualValue('Unit', entry.UnitId, 'Name'),
            weight: 1
          });
        }

        // Quantity Acc.
        if (baseUnitDefined) {
          this._columns.push({
            id: 'QuantityAccumulation',
            select: ['Direction', 'BaseQuantity'],
            label: () => {
              return `${this.translate.instant('DetailsEntry_QuantityAccumulation')} (${this.ws.getMultilingualValue('Unit', baseUnitId, 'Name')})`;
            },
            display: (entry: DetailsEntry) => {
              return accountingFormat(entry.QuantityAccumulation, '1.0-4');
            },
            isRightAligned: true,
            weight: 1
          });
        }
      }

      const definedCurrencyId = this.getAccountResourceAgentCurrencyId() || this.currencyId;
      if (!!this.account() && definedCurrencyId !== this.functionalId) {

        // Monetary Value
        this._columns.push({
          select: ['MonetaryValue', 'Direction'],
          label: () => {
            let label = this.translate.instant('Entry_MonetaryValue');
            if (!!definedCurrencyId) {
              label = `${label} (${this.ws.getMultilingualValue('Currency', definedCurrencyId, 'Name')})`;
            }
            return label;
          },
          display: (entry: DetailsEntry) => {
            const currencyId = definedCurrencyId || entry.CurrencyId;
            return accountingFormat(entry.Direction * entry.MonetaryValue, this.digitsInfo(currencyId));
          },
          isRightAligned: true,
          weight: 1
        });

        if (!!definedCurrencyId) {
          // MonetaryValue Acc.
          this._columns.push({
            id: 'MonetaryValueAccumulation',
            select: ['MonetaryValue', 'Direction'],
            label: () => {
              return `${this.translate.instant('DetailsEntry_MonetaryValueAccumulation')} (${this.ws.getMultilingualValue('Currency', definedCurrencyId, 'Name')})`;
            },
            display: (entry: DetailsEntry) => {
              return accountingFormat(entry.MonetaryValueAccumulation, this.digitsInfo(definedCurrencyId));
            },
            isRightAligned: true,
            weight: 1
          });

        } else {
          // Currency
          this._columns.push({
            select: ['Currency.Name', 'Currency.Name2', 'Currency.Name3', 'Currency.E'], // The E is to format the values correctly
            label: () => this.translate.instant('Entry_Currency'),
            display: (entry: DetailsEntry) => this.ws.getMultilingualValue('Currency', entry.CurrencyId, 'Name'),
            weight: 1
          });
        }
      }

      // Debit
      this._columns.push({
        select: ['Value', 'Direction'],
        label: () => `${this.translate.instant('Debit')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
        display: (entry: DetailsEntry) => {
          if (entry.Direction > 0) {
            return accountingFormat(entry.Value, this.functionalDigitsInfo);
          } else {
            return '';
          }
        },
        isRightAligned: true,
        weight: 1
      });

      // Credit
      this._columns.push({
        select: ['Value', 'Direction'],
        label: () => `${this.translate.instant('Credit')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
        display: (entry: DetailsEntry) => {
          if (entry.Direction < 0) {
            return accountingFormat(entry.Value, this.functionalDigitsInfo);
          } else {
            return '';
          }
        },
        isRightAligned: true,
        weight: 1
      });

      // Acc.
      this._columns.push({
        id: 'Accumulation',
        select: ['Value', 'Direction'],
        label: () => `${this.translate.instant('DetailsEntry_Accumulation')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
        display: (entry: DetailsEntry) => {
          return accountingFormat(entry.Accumulation, this.functionalDigitsInfo);
        },
        isRightAligned: true,
        weight: 1
      });
    }

    return this._columns;
  }

  public fromWeight(weight: number) {
    const totalWeight = this.columns.map(e => e.weight).reduce((acc, v) => acc + v);
    if (totalWeight === 0) { // Impossible but added for robustness
      return '100%';
    }

    return ((weight / totalWeight) * 100) + '%';
  }

  public onSelectRow(entry: DetailsEntry) {
    const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
    const doc = this.ws.get('Document', line.DocumentId) as Document;
    const docId = doc.Id;
    const definitionId = doc.DefinitionId;
    const params = { state_key: 'from_statement', tab: -10 }; // fake state key to hide forward and backward navigation in details screen
    this.router.navigate(['../documents', definitionId, docId, params], { relativeTo: this.route });
  }

  // Collapse parameters
  private _collapseParameters = false;

  public get collapseParameters(): boolean {
    return this._collapseParameters;
  }

  public set collapseParameters(v: boolean) {
    if (this._collapseParameters !== v) {
      this._collapseParameters = v;
      this.urlStateChanged();
    }
  }

  onToggleCollapseParameters() {
    this.collapseParameters = !this.collapseParameters;
  }
}

interface ColumnInfo {
  id?: string;
  select: string[];
  label: () => string;
  display: (entry: DetailsEntry) => string;
  exportDisplay?: (entry: DetailsEntry) => string;
  weight: number;
  isRightAligned?: boolean;
}
