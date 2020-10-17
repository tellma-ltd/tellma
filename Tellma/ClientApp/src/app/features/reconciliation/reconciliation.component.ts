// tslint:disable:member-ordering
import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { WorkspaceService, ReconciliationStore, ReportStatus } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { ApiService } from '~/app/data/api.service';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { TranslateService } from '@ngx-translate/core';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Account } from '~/app/data/entities/account';
import { FriendlyError, isSpecified } from '~/app/data/util';
import {
  ReconciliationGetReconciledArguments,
  ReconciliationGetReconciledResponse,
  ReconciliationGetUnreconciledArguments,
  ReconciliationGetUnreconciledResponse,
  ReconciliationSavePayload
} from '~/app/data/dto/reconciliation';
import { ExternalEntry, ExternalEntryForSave } from '~/app/data/entities/external-entry';
import { Custody, metadata_Custody } from '~/app/data/entities/custody';
import { EntryForReconciliation } from '~/app/data/entities/entry-for-reconciliation';
import { Currency } from '~/app/data/entities/currency';
import { ReconciliationForSave } from '~/app/data/entities/reconciliation';
import { NgbModal, Placement } from '@ng-bootstrap/ng-bootstrap';

type View = 'unreconciled' | 'reconciled';

@Component({
  selector: 't-reconciliation',
  templateUrl: './reconciliation.component.html',
  styles: []
})
export class ReconciliationComponent implements OnInit, OnDestroy, ICanDeactivate {

  private DEFAULT_PAGE_SIZE = 200;

  private argumentsKey = 'reconciliation/arguments';
  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.reconciliationApi(this.notifyDestruct$); // Only for intellisense

  @ViewChild('errorModal', { static: true })
  errorModal: TemplateRef<any>;

  @ViewChild('unsavedChangesModal', { static: true })
  unsavedChangesModal: TemplateRef<any>;

  public viewChoices: SelectorChoice[] = [
    { value: 'reconciled', name: () => this.translate.instant('Reconciled') },
    { value: 'unreconciled', name: () => this.translate.instant('Unreconciled') },
  ];

  private numericKeys: { [key: string]: any } = {
    account_id: undefined,
    custody_id: undefined,
    entries_top: 200,
    entries_skip: 0,
    ex_entries_top: 200,
    ex_entries_skip: 0,
    top: 200,
    skip: 0,
    from_amount: undefined,
    to_amount: undefined,
  };

  private stringKeys: { [key: string]: any } = {
    view: 'unreconciled',
    from_date: undefined,
    to_date: undefined,
    ex_ref_contains: undefined
  };

  constructor(
    private workspace: WorkspaceService, private router: Router, private customUserSettings: CustomUserSettingsService,
    private route: ActivatedRoute, private apiService: ApiService, private translate: TranslateService, private modalService: NgbModal) { }

  ngOnInit(): void {

    // Initialize the api service
    this.api = this.apiService.reconciliationApi(this.notifyDestruct$);

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

      for (const key of Object.keys(this.stringKeys)) {
        const paramValue = params.get(key) || this.stringKeys[key];
        if (args[key] !== paramValue) {
          args[key] = paramValue;
          fetchIsNeeded = true;
        }
      }

      for (const key of Object.keys(this.numericKeys)) {
        const paramValue = (+params.get(key)) || this.numericKeys[key];
        if (args[key] !== paramValue) {
          args[key] = paramValue;
          fetchIsNeeded = true;
        }
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

    // Add the string arguments
    for (const key of Object.keys(this.stringKeys)) {
      const value = args[key] || this.stringKeys[key];
      if (!!value) {
        params[key] = value;
      }
    }

    // Add the numeric arguments
    for (const key of Object.keys(this.numericKeys)) {
      const value = args[key] || this.numericKeys[key];
      if (!!value) {
        params[key] = value;
      }
    }

    // navigate to the new url
    this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
  }

  private parametersChanged(): void {

    // Just in case
    this.resetEdits();

    // Update the URL
    this.urlStateChanged();

    // Save the arguments in user settings
    const argsString = JSON.stringify(this.state.arguments);
    this.customUserSettings.save(this.argumentsKey, argsString);

    // Refresh the results
    this.fetch();
  }

  private get UnreconciledArgs(): ReconciliationGetUnreconciledArguments {
    return {
      accountId: this.accountId,
      custodyId: this.custodyId,
      asOfDate: this.toDate,
      entriesTop: this.entriesTop,
      entriesSkip: this.entriesSkip,
      externalEntriesTop: this.externalEntriesTop,
      externalEntriesSkip: this.externalEntriesSkip,
    };
  }

  private get ReconciledArgs(): ReconciliationGetReconciledArguments {
    return {
      accountId: this.accountId,
      custodyId: this.custodyId,
      fromDate: this.fromDate,
      toDate: this.toDate,
      fromAmount: this.fromAmount,
      toAmount: this.toAmount,
      externalReferenceContains: this.externalReferenceContains,
      top: this.top,
      skip: this.skip,
    };
  }

  public fetch() {
    this.notifyFetch$.next();
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
      delete s.reconciled_response;
      delete s.unreconciled_response;
      return of();
    } else {
      s.reportStatus = ReportStatus.loading;
      let obs: Observable<void>;
      if (this.isUnreconciled) {
        delete s.unreconciled_response;
        obs = this.api.getUnreconciled(this.UnreconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.unreconciled_response = response;
          })
        );
      } else if (this.isReconciled) {
        delete s.reconciled_response;
        obs = this.api.getReconciled(this.ReconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.reconciled_response = response;
          })
        );
      } else {
        console.error(`Unknown view ${this.view}`); // Future proofing
      }

      obs = obs.pipe(
        tap(_ => {
          // Result is loaded
          s.reportStatus = ReportStatus.loaded;

          // Edits cannot stay after a refresh
          this.resetEdits();
        }),
        catchError((friendlyError: FriendlyError) => {
          s.reportStatus = ReportStatus.error;
          s.errorMessage = friendlyError.error;
          return of(null);
        })
      );

      return obs;
    }
  }

  public get requiredParametersAreSet(): boolean {
    const args = this.state.arguments;
    return !!args.account_id && !!args.custody_id;
  }

  private get loadingRequiredParameters(): boolean {
    // Some times the account Id or resource Id from the Url refer to entities that are not loaded
    // Given that computing the statement query requires knowledge of these entities (not just their Ids)
    // We have to wait until the details pickers have loaded the entities for us, until then this
    // property returns true, and the statement query is not executed
    if (!!this.accountId && !this.account()) {
      return true;
    }

    if (this.showCustodyParameter && !this.readonlyCustody_Manual && !!this.custodyId && !this.ws.get('Custody', this.custodyId)) {
      return true;
    }

    return false;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get state(): ReconciliationStore {

    const ws = this.ws;
    return ws.reconciliationState = ws.reconciliationState || new ReconciliationStore();
  }

  public canDeactivate(): boolean | Observable<boolean> {
    if (this.isDirty) {

      // IF there are unsaved changes, prompt the user asking if they would like them discarded
      const modal = this.modalService.open(this.unsavedChangesModal);

      // capture the user's decision in a subject:
      // first action when the user presses one of the two buttons
      // second func is when the user dismisses the modal with x or ESC or clicking the background
      const decision$ = new Subject<boolean>();
      modal.result.then(
        v => { decision$.next(v); decision$.complete(); },
        _ => { decision$.next(false); decision$.complete(); }
      );

      // return the subject that will eventually emit the user's decision
      return decision$;

    } else {

      // IF there are no unsaved changes, the navigation can happily proceed
      return true;
    }
  }

  public get canExport() {
    return this.isLoaded;
  }

  private account(id?: number): Account {
    id = id || this.accountId;
    return this.ws.get('Account', id);
  }

  // UI Bindings

  public onParameterLoaded(): void {
    if (this.state.reportStatus === ReportStatus.loading) {
      this.fetch();
    }
  }

  // view
  public get view(): View {
    return this.state.arguments.view;
  }

  public set view(v: View) {
    const args = this.state.arguments;
    if (args.view !== v) {
      args.view = v;
      this.parametersChanged();
    }
  }

  /**
   * Returns false if the unreconciled entries view is selected
   */
  public get isUnreconciled(): boolean {
    return this.view === 'unreconciled';
  }

  /**
   * Returns true if the reconciled entries view is selected
   */
  public get isReconciled(): boolean {
    return this.view === 'reconciled';
  }

  // Error Message
  public get showErrorMessage(): boolean {
    return this.state.reportStatus === ReportStatus.error;
  }

  public get errorMessage(): string {
    return this.state.errorMessage;
  }

  public get modalErrorMessage(): string {
    return this._modalErrorMessage;
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

  // accountId
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

  // custodyId
  public get custodyId(): number {
    return this.state.arguments.custody_id;
  }

  public set custodyId(v: number) {
    const args = this.state.arguments;
    if (args.custody_id !== v) {
      args.custody_id = v;
      this.parametersChanged();
    }
  }

  public get showCustodyParameter(): boolean {
    const account = this.account();
    return !!account && !!account.CustodyDefinitionId;
  }

  public get labelCustody_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.CustodyDefinitionId : null;

    return metadata_Custody(this.workspace, this.translate, defId).titleSingular();
  }

  public get readonlyCustody_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.CustodyId;
  }

  public get readonlyValueCustodyId_Manual(): number {
    const account = this.account();
    return !!account ? account.CustodyId : null;
  }

  public get definitionIdsCustody_Manual(): number[] {
    const account = this.account();
    return [account.CustodyDefinitionId];
  }

  // toDate
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

  // fromDate
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

  // top
  public get top(): number {
    return this.state.arguments.top;
  }

  public set top(v: number) {
    const args = this.state.arguments;
    if (args.top !== v) {
      args.top = v;
      this.parametersChanged();
    }
  }

  // skip
  public get skip(): number {
    return this.state.arguments.skip;
  }

  public set skip(v: number) {
    const args = this.state.arguments;
    if (args.skip !== v) {
      args.skip = v;
      this.parametersChanged();
    }
  }

  // entries top
  public get entriesTop(): number {
    return this.state.arguments.entries_top;
  }

  public set entriesTop(v: number) {
    const args = this.state.arguments;
    if (args.entries_top !== v) {
      args.entries_top = v;
      this.parametersChanged();
    }
  }

  // entries skip
  public get entriesSkip(): number {
    return this.state.arguments.entries_skip;
  }

  public set entriesSkip(v: number) {
    const args = this.state.arguments;
    if (args.entries_skip !== v) {
      args.entries_skip = v;
      this.parametersChanged();
    }
  }

  // external entries top
  public get externalEntriesTop(): number {
    return this.state.arguments.ex_entries_top;
  }

  public set externalEntriesTop(v: number) {
    const args = this.state.arguments;
    if (args.ex_entries_top !== v) {
      args.ex_entries_top = v;
      this.parametersChanged();
    }
  }

  // external entries skip
  public get externalEntriesSkip(): number {
    return this.state.arguments.ex_entries_skip;
  }

  public set externalEntriesSkip(v: number) {
    const args = this.state.arguments;
    if (args.ex_entries_skip !== v) {
      args.ex_entries_skip = v;
      this.parametersChanged();
    }
  }

  // from amount
  public get fromAmount(): number {
    return this.state.arguments.from_amount;
  }

  public set fromAmount(v: number) {
    const args = this.state.arguments;
    if (args.from_amount !== v) {
      args.from_amount = v;
      this.parametersChanged();
    }
  }

  // to amount
  public get toAmount(): number {
    return this.state.arguments.to_amount;
  }

  public set toAmount(v: number) {
    const args = this.state.arguments;
    if (args.to_amount !== v) {
      args.to_amount = v;
      this.parametersChanged();
    }
  }

  // external reference contains
  public get externalReferenceContains(): string {
    return this.state.arguments.ex_ref_contains;
  }

  public set externalReferenceContains(v: string) {
    const args = this.state.arguments;
    if (args.ex_ref_contains !== v) {
      args.ex_ref_contains = v;
      this.parametersChanged();
    }
  }

  private get isLoaded(): boolean {
    return this.state.reportStatus === ReportStatus.loaded;
  }

  public onExport() {
    alert('TODO');
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get commandsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get disableRefresh(): boolean {
    return !this.requiredParametersAreSet;
  }

  public onRefresh(): void {
    // The if statement to deal with incessant button clickers (Users who hit refresh repeatedly)
    if (this.state.reportStatus !== ReportStatus.loading) {
      this.fetch();
    }
  }

  // Paging for reconciled entities

  get from(): number {
    const skip = this.state.arguments.skip;
    return Math.min(skip + 1, this.total);
  }

  get to(): number {
    return Math.min(this.state.arguments.skip + this.DEFAULT_PAGE_SIZE, this.total);
  }

  get total(): number {
    return !!this.state.reconciled_response ?
      this.state.reconciled_response.Reconciliations.length : 0;
  }

  onPreviousPage() {
    const args = this.state.arguments;
    args.skip = Math.max(args.skip - this.DEFAULT_PAGE_SIZE, 0);

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canPreviousPage(): boolean {
    return this.state.arguments.skip > 0;
  }

  onNextPage() {
    const s = this.state.arguments;
    s.skip = s.skip + this.DEFAULT_PAGE_SIZE;

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  // results

  private _rowsResponse: ReconciliationGetUnreconciledResponse | ReconciliationGetReconciledResponse;
  private _rows: ReconciliationRow[] = [];
  public get rows(): ReconciliationRow[] {
    switch (this.view) {
      case 'reconciled': {
        const res = this.state.reconciled_response;
        if (this._rowsResponse !== res) {
          this._rowsResponse = res;
          this._rows = [];
          if (!!res) {
            for (const reconciliation of res.Reconciliations) {
              const length = Math.max(reconciliation.Entries.length, reconciliation.ExternalEntries.length);
              for (let i = 0; i < length; i++) {
                const row: ReconciliationRow = { isReconciled: true };

                // The first row will have a middle cell extending to the entire reconciliation, with a button to unreconcile
                if (i === 0) {
                  row.rowSpan = length;
                }
                // The last row will have a thick bottom border indicating that the
                // reconciliation is done, and shows the next reconciliation
                if (i === length - 1) {
                  row.lastOne = true;
                }

                // The entry
                const reconciliationEntry = reconciliation.Entries[i];
                if (!!reconciliationEntry) {
                  row.entry = reconciliationEntry.Entry;
                }

                // The external entry
                const reconciliationExEntry = reconciliation.ExternalEntries[i];
                if (!!reconciliationExEntry && !!reconciliationExEntry.ExternalEntryId) {
                  row.exEntry = reconciliationExEntry.ExternalEntry;
                }

                this._rows.push(row);
              }
            }
          }
        }
        break;
      }
      case 'unreconciled': {
        const res = this.state.unreconciled_response;
        if (this._rowsResponse !== res) {
          this._rowsResponse = res;
          this._rows = [];
          if (!!res) {
            const length = Math.max(res.Entries.length, res.ExternalEntries.length);
            let lastExEntryIndex = 0; // Keeps track of the last row that isn't reconciled and doesn't have an external entry
            for (let i = 0; i < length; i++) {
              const row: ReconciliationRow = { isReconciled: false };

              // The entry
              row.entry = res.Entries[i];
              row.exEntry = res.ExternalEntries[i];

              this._rows.push(row);

              if (row.isReconciled || !!row.exEntry) {
                lastExEntryIndex = i;
              }
            }

            this.fixCreateExEntryRow();
          }
        }
        break;
      }

      default: {
        console.error(`Unknown view ${this.view}`); // future proofing
        this._rows = [];
      }
    }

    return this._rows;
  }

  private _showBuiltInCreateRow = false;

  public get showBuiltInCreateRow(): boolean {
    return this._showBuiltInCreateRow;
  }

  private fixCreateExEntryRow(i = -1): void {
    this._rows.forEach(r => delete r.exEntryIsCreate);
    this._showBuiltInCreateRow = false;

    // Find the index of the last occupied row
    const index = i >= 0 ? i : this.findIndexForCreate();

    if (index >= this._rows.length) {
      this._showBuiltInCreateRow = true; // All rows are occupied show the Create command on a dedicated row
    } else {
      this._rows[index].exEntryIsCreate = true; // Show the Create command at this row index
    }
  }

  private findIndexForCreate(): number {

    // Find the index of the last occupied row
    const rowsLength = this._rows.length;
    let index = rowsLength - 1;
    for (; index >= 0; index--) {
      const row = this._rows[index];
      if (row.isReconciled || !!row.exEntry) {
        break;
      }
    }

    return index + 1;
  }

  public onCreateExEntry(): void {
    // (1) Find the index in _rows where the new external entry can be inserted
    const index = this.findIndexForCreate();

    // (2) Create the external entry and add it to entities for save
    const exEntry: ExternalEntryForSave = {};
    this.externalEntriesForSave.push(exEntry);
    const exEntryIndex = this.externalEntriesForSave.length - 1;

    // (3) Add the external entry to the view and make it editable
    if (this._rows.length <= index) {
      this._rows.push({ isReconciled: false, exEntryIsEdit: true, exEntry, exEntryIndex });
      this._rows = this._rows.slice(); // To refresh the virtual scroll
    } else {
      const row = this._rows[index];
      row.exEntry = exEntry;
      row.exEntryIsEdit = true;
      row.exEntryIndex = exEntryIndex;
    }

    this.fixCreateExEntryRow(index + 1);
  }

  private removeExternalEntryForSave(exEntryIndex: number) {
    const forSave = this.externalEntriesForSave;
    forSave[exEntryIndex] = null; // In order not to mess up other indices

    // Cleanup: if the last N externalEntriesForSave are null, remove them
    const forSaveLength = forSave.length;
    for (let i = forSaveLength - 1; i >= 0; i--) {
      if (!forSave[i]) {
        forSave.pop();
      } else {
        break;
      }
    }
  }

  public onDeleteExEntry(row: ReconciliationRow, index: number): void {
    const exEntryIndex = row.exEntryIndex;
    if (isSpecified(exEntryIndex)) {
      this.removeExternalEntryForSave(exEntryIndex);
    }

    // If it has an Id, add it to the deleted Ids
    if (!!row.exEntry.Id) {
      this.deletedExternalEntryIds.push(+row.exEntry.Id);
    }

    // Delete the external entry
    delete row.exEntry;
    delete row.exEntryOriginal;
    delete row.exEntryIsEdit;
    delete row.exEntryIndex;
    delete row.exEntryIsChecked;

    // Shift all subsequent entries one row up
    this.shiftAllExternalEntriesUp(index);

    // Adjust the row that allows for creating a new external entry
    this.fixCreateExEntryRow();
  }

  private shiftAllExternalEntriesUp(index: number) {
    for (let i = index + 1; i < this._rows.length; i++) {
      const prevRow = this._rows[i - 1];
      const nextRow = this._rows[i];

      prevRow.exEntry = nextRow.exEntry;
      prevRow.exEntryOriginal = nextRow.exEntryOriginal;
      prevRow.exEntryIsEdit = nextRow.exEntryIsEdit;
      prevRow.exEntryIndex = nextRow.exEntryIndex;
      prevRow.exEntryIsChecked = nextRow.exEntryIsChecked;
    }

    const lastRow = this._rows[this._rows.length - 1];
    if (!lastRow.entry) {
      this._rows.pop();
      this._rows = this._rows.slice(); // To refresh the virtual scroll
    } else {
      delete lastRow.exEntry;
      delete lastRow.exEntryOriginal;
      delete lastRow.exEntryIsEdit;
      delete lastRow.exEntryIndex;
      delete lastRow.exEntryIsChecked;
    }
  }

  public onEditExEntry(row: ReconciliationRow): void {
    row.exEntryIsEdit = true;
    row.exEntryOriginal = row.exEntry;
    row.exEntry = JSON.parse(JSON.stringify(row.exEntryOriginal)); // Store the original safely to support cancellation
  }

  public onCancelEditExEntry(row: ReconciliationRow, index: number): void {
    if (!row.exEntry.Id) { // New entry
      this.onDeleteExEntry(row, index);
    } else { // Modified entry
      if (isSpecified(row.exEntryIndex)) {
        this.removeExternalEntryForSave(row.exEntryIndex);
      }

      row.exEntryIsEdit = false;
      row.exEntry = row.exEntryOriginal;
      delete row.exEntryOriginal;
    }
  }

  public onChangeExEntry(row: ReconciliationRow, _: number): void {
    if (!isSpecified(row.exEntryIndex)) { // Freshly edited
      this.externalEntriesForSave.push(row.exEntry);
      row.exEntryIndex = this.externalEntriesForSave.length - 1;
    }
  }

  public onChangeExternalReference(row: ReconciliationRow, rowIndex: number, v: string): void {
    this.onChangeExEntry(row, rowIndex);
    row.exEntry.ExternalReference = v;
  }

  public onChangePostingDate(row: ReconciliationRow, rowIndex: number, v: string): void {
    this.onChangeExEntry(row, rowIndex);
    row.exEntry.PostingDate = v;
  }

  public onChangeMonetaryValue(row: ReconciliationRow, rowIndex: number, v: number): void {
    this.onChangeExEntry(row, rowIndex);
    this.setMonetaryValue(row.exEntry, v);
  }

  private externalEntriesForSave: ExternalEntryForSave[] = [];
  private reconciliationsForSave: ReconciliationForSave[] = [];
  private deletedExternalEntryIds: number[] = [];
  private deletedReconciliationIds: number[] = [];

  private resetEdits() {
    this.externalEntriesForSave = [];
    this.reconciliationsForSave = [];
    this.deletedExternalEntryIds = [];
    this.deletedReconciliationIds = [];
  }

  public get canSave(): boolean {
    return true;
  }

  public onSave(): void {
    // For robustness grab a reference to the state object, in case it changes later
    if (this.canSave && this.isDirty && this.requiredParametersAreSet && !this.loadingRequiredParameters) {

      // Prepare the payload
      const payload: ReconciliationSavePayload = {
        ExternalEntries: this.externalEntriesForSave,
        Reconciliations: this.reconciliationsForSave,
        DeletedExternalEntryIds: this.deletedExternalEntryIds,
        DeletedReconciliationIds: this.deletedReconciliationIds
      };

      const s = this.state;
      let obs$: Observable<void>;
      if (this.isUnreconciled) {
        obs$ = this.api.saveAndGetUnreconciled(payload, this.UnreconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.unreconciled_response = response;
          })
        );
      } else if (this.isReconciled) {
        delete s.reconciled_response;
        obs$ = this.api.saveAndGetReconciled(payload, this.ReconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.reconciled_response = response;
          })
        );
      } else {
        console.error(`Unknown view ${this.view}`); // Future proofing
      }

      obs$.pipe(
        tap(_ => {
          // Result is loaded
          s.reportStatus = ReportStatus.loaded;

          // Edits cannot stay after a refresh
          this.resetEdits();
        }),
        catchError((friendlyError: FriendlyError) => {

          if (friendlyError.status === 422) {
            alert('TODO: Validation Errors');
          } else {
            this.displayModalError(friendlyError.error);
          }

          return of(null);
        })
      ).subscribe();
    }
  }

  private _modalErrorMessage: string; // in the modal

  public displayModalError(errorMessage: string) {
    // shows the error message in a dismissable modal
    this._modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  public onCancel() {
    // prompt the user manually, since the Angular Router isn't involved
    const canCancel = this.canDeactivate();
    if (canCancel instanceof Observable) {
      canCancel.subscribe(can => {
        if (can) {
          this.doCancel();
        }
      });
    } else if (canCancel) {
      this.doCancel();
    }
  }

  public doCancel() {
    this.resetEdits();
    delete this._rowsResponse; // Causes rows() to re-render
  }

  public onSelectRow(row: ReconciliationRow) {
    alert('TODO');
  }

  public get showCheckedEntriesToolbar(): boolean {
    // Always appears when there are checkboxes selected
    return !!this.rows && this.rows.some(e => e.entryIsChecked || e.exEntryIsChecked);
  }

  public get showEditToolbar(): boolean {
    // Appears when there are dirty changes
    return this.isDirty && !this.showCheckedEntriesToolbar;
  }

  public get showViewToolbar(): boolean {
    return !this.isDirty && !this.showCheckedEntriesToolbar;
  }

  private get checkedEntries(): EntryForReconciliation[] {
    if (!this.rows) {
      return [];
    }

    return this.rows.filter(e => !!e.entry && e.entryIsChecked).map(e => e.entry);
  }

  private get checkedExEntries(): ExternalEntry[] {
    if (!this.rows) {
      return [];
    }

    return this.rows.filter(e => !!e.exEntry && e.exEntryIsChecked).map(e => e.exEntry);
  }

  public get checkedEntriesTotal(): number {
    return this.checkedEntries.map(e => e.Direction * e.MonetaryValue).reduce((a, b) => a + b, 0);
  }

  public get checkedExEntriesTotal(): number {
    return this.checkedExEntries.map(e => e.Direction * e.MonetaryValue).reduce((a, b) => a + b, 0);
  }

  public get isDirty(): boolean {
    return this.externalEntriesForSave.length > 0 ||
      this.reconciliationsForSave.length > 0 ||
      this.deletedExternalEntryIds.length > 0 ||
      this.deletedReconciliationIds.length > 0;
  }

  /**
   * Unchecks all checked internal entries and external entries
   */
  private uncheckAll() {
    if (!!this.rows) {
      for (const row of this.rows) {
        delete row.entryIsChecked;
        delete row.exEntryIsChecked;
      }
    }
  }

  public onReconcileChecked() {
    if (this.canReconcileChecked) {
      const reconciliation: ReconciliationForSave = {
        Entries: [],
        ExternalEntries: []
      };

      for (const row of this._rows) {
        if (row.entryIsChecked && !!row.entry) {
          const entry = row.entry;
          reconciliation.Entries.push({ EntryId: +entry.Id });
        }

        if (row.exEntryIsChecked && !!row.exEntry) {
          const exEntry = row.exEntry;
          reconciliation.ExternalEntries.push({ ExternalEntryId: +exEntry.Id, ExternalEntryIndex: row.exEntryIndex });
        }
      }

      this.reconciliationsForSave.push(reconciliation);
      this.uncheckAll(); // Clear the selection
    }
  }

  public get canReconcileChecked(): boolean {
    return this.checkedEntriesTotal === this.checkedExEntriesTotal;
  }

  public onCancelReconcileChecked() {
    this.uncheckAll();
  }

  private amountsDecimalsAccount: Account;
  private amountsDecimalsCustody: Custody;
  private amountsDecimalsResult: number;
  public get amountsDecimals(): number {
    const custody = this.ws.get('Custody', this.custodyId);
    const account = this.ws.get('Account', this.accountId);
    if (this.amountsDecimalsCustody !== custody && this.amountsDecimalsAccount !== account) {
      this.amountsDecimalsCustody = custody;
      this.amountsDecimalsAccount = account;

      const currencyId = (!!account ? account.CurrencyId : null) || (!!custody ? custody.CurrencyId : null);
      const currency = this.ws.get('Currency', currencyId) as Currency;
      this.amountsDecimalsResult = !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
    }

    return this.amountsDecimalsResult;
  }

  private amountsFormatAccount: Account;
  private amountsFormatCustody: Custody;
  private amountsFormatResult: string;
  public get amountsFormat(): string {
    const custody = this.ws.get('Custody', this.custodyId);
    const account = this.ws.get('Account', this.accountId);
    if (this.amountsFormatCustody !== custody && this.amountsFormatAccount !== account) {
      this.amountsFormatAccount = custody;
      this.amountsFormatCustody = account;

      const decimals = this.amountsDecimals;
      this.amountsFormatResult = `1.${decimals}-${decimals}`;
    }

    return this.amountsFormatResult;
  }

  public getMonetaryValue(exEntry: ExternalEntry): number {
    if (!exEntry) {
      return null;
    }

    return exEntry.Direction * exEntry.MonetaryValue;
  }

  public setMonetaryValue(exEntry: ExternalEntry, v: number): void {
    if (!exEntry) {
      return;
    }

    exEntry.Direction = v < 0 ? -1 : 1;
    exEntry.MonetaryValue = Math.abs(v);
  }
}

interface ReconciliationRow {
  // Reconciled Stuff
  isReconciled: boolean;
  rowSpan?: number;
  lastOne?: boolean;

  ///////////////// Entry

  /**
   * Entry
   */
  entry?: EntryForReconciliation;

  /**
   * Checkbox for external entry
   */
  entryIsChecked?: boolean;

  ///////////////// External Entry ()

  /**
   * External Entry
   */
  exEntry?: ExternalEntry;

  /**
   * External Entry
   */
  exEntryOriginal?: ExternalEntry;

  /**
   * Checkbox for external entry
   */
  exEntryIsChecked?: boolean;

  /**
   * Inserted or updates external entries will have their indices stored here
   */
  exEntryIndex?: number;

  /**
   * Indicates that the external entry in this row is in edit mode
   */
  exEntryIsEdit?: boolean;

  /**
   * Indicates that you can create an external entry in this row
   */
  exEntryIsCreate?: boolean;
}
