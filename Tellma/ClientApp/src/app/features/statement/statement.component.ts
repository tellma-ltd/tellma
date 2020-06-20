// tslint:disable:member-ordering
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, ReportStore } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Resource, metadata_Resource } from '~/app/data/entities/resource';
import { Account } from '~/app/data/entities/account';
import { metadata_Contract } from '~/app/data/entities/contract';
import { AccountType } from '~/app/data/entities/account-type';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';

@Component({
  selector: 't-statement',
  templateUrl: './statement.component.html',
  styles: []
})
export class StatementComponent implements OnInit, OnDestroy {

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();

  private numericKeys = ['account_id', 'segment_id', 'contract_id', 'resource_id', 'entry_type_id', 'center_id'];
  private stringKeys = ['from_date', 'to_date', 'currency_id'];

  @Input()
  type: 'account' | 'contract';

  constructor(
    private route: ActivatedRoute, private router: Router, private customUserSettings: CustomUserSettingsService,
    private translate: TranslateService, private workspace: WorkspaceService) { }

  ngOnInit(): void {

    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {

      // Copy all report arguments from URL

      let fetchIsNeeded = false;
      const s = this.state;
      const args = s.arguments;

      for (const key of this.stringKeys) {
        const paramValue = params.get(key) || undefined;
        if (args[key] !== paramValue) {
          args[key] = params.get(key);
          fetchIsNeeded = true;
        }
      }

      for (const key of this.numericKeys) {
        const paramValue = (+params.get(key)) || undefined;
        if (args[key] !== paramValue) {
          args[key] = params.get(key);
          fetchIsNeeded = true;
        }
      }

      // Other screen parameters
      const skipParam = +params.get('skip') || 0;
      if (s.skip !== skipParam) {
        s.skip = skipParam;
      }

      if (fetchIsNeeded) {
        this.fetch();
      }

      if (this.isAccount) {
      }

      if (this.isContract) {
        // TODO
      }
    }));

    this._subscriptions.add(this.notifyFetch$.pipe().subscribe(() => this.doFetch()));
  }

  ngOnDestroy(): void {
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
    return null; // TODO
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
    // TODO
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

}
