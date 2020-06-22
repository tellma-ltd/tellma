// tslint:disable:member-ordering
// tslint:disable:max-line-length
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, ReportStore, ReportStatus } from '~/app/data/workspace.service';
import { tap, catchError, switchMap } from 'rxjs/operators';
import { Resource, metadata_Resource } from '~/app/data/entities/resource';
import { Account } from '~/app/data/entities/account';
import { metadata_Contract } from '~/app/data/entities/contract';
import { AccountType } from '~/app/data/entities/account-type';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { Entity } from '~/app/data/entities/base/entity';
import { DetailsEntry } from '~/app/data/entities/details-entry';
import { formatDate, formatNumber } from '@angular/common';
import { LineForQuery } from '~/app/data/entities/line';
import { Document, metadata_Document } from '~/app/data/entities/document';
import { SerialPropDescriptor } from '~/app/data/entities/base/metadata';
import { ApiService } from '~/app/data/api.service';
import { FriendlyError, mergeEntitiesInWorkspace, isSpecified } from '~/app/data/util';
import { GetArguments } from '~/app/data/dto/get-arguments';
import { StatementArguments } from '~/app/data/dto/statement-arguments';

@Component({
  selector: 't-statement',
  templateUrl: './statement.component.html',
  styles: []
})
export class StatementComponent implements OnInit, OnDestroy {

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.detailsEntriesApi(this.notifyDestruct$); // Only for intellisense

  private numericKeys = ['account_id', 'segment_id', 'contract_id', 'resource_id', 'entry_type_id', 'center_id'];
  private stringKeys = ['from_date', 'to_date', 'currency_id'];

  @Input()
  type: 'account' | 'contract';

  constructor(
    private route: ActivatedRoute, private router: Router, private customUserSettings: CustomUserSettingsService,
    private translate: TranslateService, private workspace: WorkspaceService, private apiService: ApiService) { }

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

      if (this.isAccount) {
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
      }

      if (this.isContract) {
        // TODO
      }

      // Other screen parameters
      const skipParam = +params.get('skip') || 0;
      if (s.skip !== skipParam) {
        s.skip = skipParam;
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

  private urlStateChanged(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    const s = this.state;
    const args = s.arguments;
    const params: Params = {};

    // Add the arguments
    for (const key of this.stringKeys.concat(this.numericKeys)) {
      const value = args[key] || undefined;
      if (!!value) {
        params[key] = value;
      }
    }

    // Add skip
    if (!!s.skip) {
      params.skip = s.skip;
    }

    // navigate to the new url
    this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
  }

  private parametersChanged(): void {

    // Update the URL
    this.urlStateChanged();

    // Save the arguments in user settings
    const argsString = JSON.stringify(this.state.arguments);
    this.customUserSettings.save('account-statement/arguments', argsString);

    // Refresh the results
    this.fetch();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  private get isAccount() {
    return this.type === 'account';
  }

  private get isContract() {
    return this.type === 'contract';
  }

  public fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {
    const s = this.state;

    if (this.missingRequiredParameters) {
      s.reportStatus = ReportStatus.information;
      s.information = () => this.translate.instant('FillRequiredFields');
      return of();
    } else {
      // For robustness grab a reference to the state object, in case it changes later
      s.reportStatus = ReportStatus.loading;
      s.result = [];

      // Prepare the query params
      const select = this.columns.map(arr => arr.select.join(',')).join(',');
      const top = this.DEFAULT_PAGE_SIZE;
      const skip = s.skip;

      // Prepare the query filter
      const filter = null; // = 'Line/State eq 4'; // TODO
      const args: StatementArguments = {
        select, top, skip,
        fromDate: formatDate(this.fromDate, 'yyyy-MM-dd', 'en-GB'),
        toDate: formatDate(this.toDate, 'yyyy-MM-dd', 'en-GB'),
        accountId: this.accountId
      };

      if (!!this.segmentId) {
        args.segmentId = this.segmentId;
      }

      if (!!this.contractId) {
        args.contractId = this.contractId;
      }

      if (!!this.resourceId) {
        args.resourceId = this.resourceId;
      }

      if (!!this.entryTypeId) {
        args.entryTypeId = this.entryTypeId;
      }

      if (!!this.centerId) {
        args.centerId = this.centerId;
      }

      if (!!this.currencyId) {
        args.currencyId = this.currencyId;
      }

      return this.api.statement(args).pipe(
        tap(response => {
          // Result is loaded
          s.reportStatus = ReportStatus.loaded;

          // Add the result to the state
          s.filter = filter;
          s.result = response.Result;
          s.extras = { opening: response.Opening, closing: response.Closing };

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

  private get missingRequiredParameters(): boolean {
    const args = this.state.arguments;
    if (this.isAccount) {
      return !args.from_date || !args.to_date || !args.account_id || (this.showSegmentParameter && !args.segment_id);
    }

    if (this.isContract) {
      // TODO
    }
  }

  // UI Bindings

  public get title() {
    if (this.isAccount) {
      return this.translate.instant('AccountStatement');
    } else if (this.isContract) {
      // TODO
    } else {
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

  public onExport(): void {
    // TODO
  }

  public get canExport(): boolean {
    return true; // TODO
  }

  public onRefresh(): void {
    // The if statement to deal with incessant button clickers (Users who hit refresh repeatedly)
    if (this.state.reportStatus !== ReportStatus.loading) {
      this.fetch();
    }
  }

  public get canRefresh(): boolean {
    return true; // TODO
  }

  DEFAULT_PAGE_SIZE = 60;

  public get stateKey(): string {
    if (this.isAccount) {
      return 'account-statement';
    } else {
      // TODO
    }
  }

  public get state(): ReportStore {

    if (!this.workspace.currentTenant.reportState[this.stateKey]) {
      this.workspace.currentTenant.reportState[this.stateKey] = new ReportStore();
    }

    return this.workspace.currentTenant.reportState[this.stateKey];
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
    if (this.isAccount) {
      return `../${finalSegment}`;
    }

    if (this.isContract) {
      return `../../${finalSegment}`;
    }
  }

  // From Date
  public get fromDate(): string {
    return this.state.arguments.from_date;
  }

  public set fromDate(v: string) {
    this.state.arguments.from_date = v;
    this.parametersChanged();
  }

  // To Date
  public get toDate(): string {
    return this.state.arguments.to_date;
  }

  public set toDate(v: string) {
    this.state.arguments.to_date = v;
    this.parametersChanged();
  }

  // Account
  public accountAdditionalSelect = '$DocumentDetails';

  public get accountId(): number {
    return this.state.arguments.account_id;
  }

  public set accountId(v: number) {
    this.state.arguments.account_id = v;
    this.parametersChanged();
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

  // Segment
  public get segmentId(): number {
    return this.state.arguments.segment_id;
  }

  public set segmentId(v: number) {
    this.state.arguments.segment_id = v;
    this.parametersChanged();
  }

  /**
   * Whether or not to show th segment parameter
   */
  public get showSegmentParameter(): boolean {
    return this.ws.settings.IsMultiSegment;
  }

  // Currency
  public get currencyId(): string {
    return this.state.arguments.currency_id;
  }

  public set currencyId(v: string) {
    this.state.arguments.currency_id = v;
    this.parametersChanged();
  }

  /**
   * Returns the currency Id from the selected account or from the selected resource if any
   */
  private getAccountResourceCurrencyId(): string {
    // IMOPORTANT: Keep consistent with documents-details.component.ts
    const account = this.account();
    const resource = this.resource();

    const accountCurrencyId = !!account ? account.CurrencyId : null;
    const resourceCurrencyId = !!resource ? resource.CurrencyId : null;

    return accountCurrencyId || resourceCurrencyId;
  }

  /**
   * Whether or not to show the currency parameter
   */
  public get showCurrencyParameter(): boolean {
    // Show the editable currency parameter
    const account = this.account();
    return !!account && !this.getAccountResourceCurrencyId();
  }

  /**
   * Whether or not to show the currency parameter
   */
  public get showCurrencyColumn(): boolean {
    return this.showCurrencyParameter && !this.currencyId;
  }

  /**
   * Returns the Id of the currency to show as a postfix to the monetary value column header
   */
  public get readonlyValueCurrencyId(): string {
    const accountResourceCurrencyId = this.getAccountResourceCurrencyId();
    return accountResourceCurrencyId || this.currencyId;
  }

  /**
   * Syntactic sugar to get the functional currency Id
   */
  public get functionalId(): string {
    return this.ws.settings.FunctionalCurrencyId;
  }

  // Contract
  public get contractId(): number {
    return this.state.arguments.contract_id;
  }

  public set contractId(v: number) {
    this.state.arguments.contract_id = v;
    this.parametersChanged();
  }

  public get showContract_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.ContractDefinitionId;
  }

  public get readonlyContract_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.ContractId;
  }

  public get readonlyValueContractId_Manual(): number {
    const account = this.account();
    return !!account ? account.ContractId : null;
  }

  public get labelContract_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.ContractDefinitionId : null;

    return metadata_Contract(this.workspace, this.translate, defId).titleSingular();
  }

  public get definitionIdsContract_Manual(): number[] {
    const account = this.account();
    return [account.ContractDefinitionId];
    // return !!account && !!account.ContractDefinitions ? account.ContractDefinitions.map(e => e.ContractDefinitionId) : [];
  }

  // Resource

  public get resourceId(): number {
    return this.state.arguments.resource_id;
  }

  public set resourceId(v: number) {
    this.state.arguments.resource_id = v;
    this.parametersChanged();
  }

  private resource(id?: number): Resource {
    id = id || this.resourceId;
    return this.ws.get('Resource', id);
  }

  public get showResource_Manual(): boolean {
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

  // Entry Type

  public get entryTypeId(): number {
    return this.state.arguments.entry_type_id;
  }

  public set entryTypeId(v: number) {
    this.state.arguments.entry_type_id = v;
    this.parametersChanged();
  }

  public get showEntryType_Manual(): boolean {
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
    return `IsAssignable eq true and Node descof ${accountType.EntryTypeParentId}`;
  }

  // Center

  public get centerId(): number {
    return this.state.arguments.center_id;
  }

  public set centerId(v: number) {
    this.state.arguments.center_id = v;
    this.parametersChanged();
  }

  public get showCenter_Manual(): boolean {
    const account = this.account();
    return !!account && this.ws.settings.IsMultiCenter;
  }

  public get readonlyCenter_Manual(): boolean {
    const at = this.account();
    return !!at && !!at.CenterId;
  }

  public get readonlyValueCenterId_Manual(): number {
    const account = this.account();
    return !!account ? account.CenterId : null;
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
    return this.isLoaded && this.from === 0;
  }
  public get showClosingBalance(): boolean {
    return this.isLoaded && this.to === this.total;
  }

  public get openingDisplay(): string {
    const s = this.state;
    if (s.extras) {
      const opening = s.extras.opening || 0;
      return formatNumber(opening, 'en-GB', this.functionalDigitsInfo);
    }

    return '';
  }

  public get closingDisplay(): string {
    const s = this.state;
    if (s.extras) {
      const closing = s.extras.closing || 0;
      return formatNumber(closing, 'en-GB', this.functionalDigitsInfo);
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

  private _columns: ColumnInfo[];
  public get columns(): ColumnInfo[] {

    if (!this._columns) {
      const settings = this.ws.settings;
      const locale = 'en-GB';

      this._columns = [
        // Posting Date
        {
          select: ['Line/PostingDate'],
          label: () => this.translate.instant('Line_PostingDate'),
          display: (entry: DetailsEntry) => {
            const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
            return formatDate(line.PostingDate, 'yyyy-MM-dd', locale);
          },
          weight: 1
        },

        // Serial Number
        {
          select: ['Line/Document/SerialNumber', 'Line/Document/DefinitionId'],
          label: () => this.translate.instant('Document_SerialNumber'),
          display: (entry: DetailsEntry) => {
            const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
            const doc = this.ws.get('Document', line.DocumentId) as Document;
            const desc = metadata_Document(this.workspace, this.translate, doc.DefinitionId);
            const prop = desc.properties.SerialNumber as SerialPropDescriptor;
            return prop.format(doc.SerialNumber);
          },
          weight: 1
        }];

      // Memo
      this._columns.push(
        {
          select: ['Line/Memo'],
          label: () => this.translate.instant('Memo'),
          display: (entry: DetailsEntry) => {
            const line = this.ws.get('LineForQuery', entry.LineId) as LineForQuery;
            return line.Memo;
          },
          weight: 2
        });

      // Debit
      this._columns.push(
        {
          select: ['Value', 'Direction'],
          label: () => `${this.translate.instant('Debit')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
          display: (entry: DetailsEntry) => {
            if (entry.Direction > 0 && isSpecified(entry.Value)) {
              return formatNumber(entry.Value, locale, this.functionalDigitsInfo);
            } else {
              return '';
            }
          },
          isRightAligned: true,
          weight: 1
        });

      // Credit
      this._columns.push(
        {
          select: ['Value', 'Direction'],
          label: () => `${this.translate.instant('Credit')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
          display: (entry: DetailsEntry) => {
            if (entry.Direction < 0 && isSpecified(entry.Value)) {
              return formatNumber(entry.Value, locale, this.functionalDigitsInfo);
            } else {
              return '';
            }
          },
          isRightAligned: true,
          weight: 1
        });

      // Acc.
      this._columns.push(
        {
          select: ['Value', 'Direction'],
          label: () => `${this.translate.instant('Accumulation')} (${this.ws.getMultilingualValueImmediate(settings, 'FunctionalCurrencyName')})`,
          display: (entry: DetailsEntry) => {
            if (isSpecified(entry.Accumulation)) {
              return formatNumber(entry.Accumulation, locale, this.functionalDigitsInfo);
            } else {
              return '';
            }
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
}

interface ColumnInfo {
  select: string[];
  label: () => string;
  display: (entry: DetailsEntry) => string;
  isRightAligned?: boolean;
  weight?: number;
}
