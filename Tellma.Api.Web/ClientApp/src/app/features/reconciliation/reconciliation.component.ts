// tslint:disable:member-ordering
import { Component, OnInit, OnDestroy, TemplateRef, ViewChild, AfterViewInit, HostListener } from '@angular/core';
import { WorkspaceService, ReconciliationStore, ReportStatus } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable, of, timer } from 'rxjs';
import { ApiService } from '~/app/data/api.service';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { TranslateService } from '@ngx-translate/core';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Account, metadata_Account } from '~/app/data/entities/account';
import { csvPackage, FriendlyError, isSpecified, downloadBlob } from '~/app/data/util';
import { toLocalDateOnlyISOString } from '~/app/data/date-util';
import {
  ReconciliationGetReconciledArguments,
  ReconciliationGetReconciledResponse,
  ReconciliationGetUnreconciledArguments,
  ReconciliationGetUnreconciledResponse,
  ReconciliationSavePayload
} from '~/app/data/dto/reconciliation';
import { ExternalEntry, ExternalEntryForSave } from '~/app/data/entities/external-entry';
import { EntryForReconciliation } from '~/app/data/entities/entry-for-reconciliation';
import { Currency } from '~/app/data/entities/currency';
import { Reconciliation, ReconciliationForSave } from '~/app/data/entities/reconciliation';
import { NgbModal, Placement } from '@ng-bootstrap/ng-bootstrap';
import { formatSerialFromDefId } from '~/app/data/entities/document';
import { getEditDistance } from '~/app/data/edit-distance';
import { accountingFormat } from '~/app/shared/accounting/accounting-format';
import { metadata_Agent, Agent } from '~/app/data/entities/agent';

type View = 'unreconciled' | 'reconciled';

interface AutoReconcileArguments {
  postingDateTolerance: number;
  externalRefTolerance: number;
}

@Component({
  selector: 't-reconciliation',
  templateUrl: './reconciliation.component.html',
  styles: []
})
export class ReconciliationComponent implements OnInit, AfterViewInit, OnDestroy, ICanDeactivate {

  private MIN_PAGE_SIZE = 500;

  private argumentsKey = 'reconciliation/arguments';
  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.reconciliationApi(this.notifyDestruct$); // Only for intellisense

  @ViewChild('errorModal', { static: true })
  errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  @ViewChild('unsavedChangesModal', { static: true })
  unsavedChangesModal: TemplateRef<any>;

  public viewChoices: SelectorChoice[] = [
    { value: 'reconciled', name: () => this.translate.instant('Reconciled') },
    { value: 'unreconciled', name: () => this.translate.instant('Unreconciled') },
  ];

  private numericKeys: { [key: string]: any } = {
    account_id: undefined,
    agent_id: undefined,
    entries_top: this.MIN_PAGE_SIZE,
    entries_skip: 0,
    ex_entries_top: this.MIN_PAGE_SIZE,
    ex_entries_skip: 0,
    top: this.MIN_PAGE_SIZE,
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
    private route: ActivatedRoute, private apiService: ApiService, private translate: TranslateService, public modalService: NgbModal) { }

  ngOnInit(): void {

    // Initialize the api service
    this.api = this.apiService.reconciliationApi(this.notifyDestruct$);

    // Set up all the subscriptions
    this._subscriptions = new Subscription();

    // Subscribe to fetch requests
    this._subscriptions.add(this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe());

    const autoReconcileParams = this.customUserSettings.get<AutoReconcileArguments>(this._autoReconcileParamsKey);
    if (!!autoReconcileParams) {
      this.postingDateTolerance = +autoReconcileParams.postingDateTolerance || 0;
      this.externalRefTolerance = +autoReconcileParams.externalRefTolerance || 0;
    } else {
      this.postingDateTolerance = 0;
      this.externalRefTolerance = 0;
    }

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

      // Collapse, special case
      args.collapse = params.get('collapse') === 'true';

      if (fetchIsNeeded) {
        this.fetch();
      }
    }));
  }

  public initialized = false;

  ngAfterViewInit() {
    if (!this.initialized) {
      timer(1).subscribe(() => this.initialized = true);
    }
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

    // Save the arguments in user settings
    const argsString = JSON.stringify(args);
    this.customUserSettings.save(this.argumentsKey, argsString);
  }

  private parametersChanged(): void {

    // Just in case
    this.resetEdits();

    // Update the URL
    this.urlStateChanged();

    // Refresh the results
    this.fetch();
  }

  private get UnreconciledArgs(): ReconciliationGetUnreconciledArguments {
    return {
      accountId: this.accountId,
      agentId: this.visibleAgentId,
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
      agentId: this.visibleAgentId,
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
      delete s.reconciled_response;
      delete s.unreconciled_response;
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
            s.unreconciled_entries_count = response.UnreconciledEntriesCount;
            s.unreconciled_ex_entries_count = response.UnreconciledExternalEntriesCount;
          })
        );
      } else if (this.isReconciled) {
        delete s.reconciled_response;
        obs = this.api.getReconciled(this.ReconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.reconciled_response = response;
            s.reconciled_count = response.ReconciledCount;
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
    return !!args.account_id && (!this.showAgentParameter || !!this.visibleAgentId);
  }

  private get loadingRequiredParameters(): boolean {
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
    return this.state.arguments.view || 'unreconciled';
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

  // Success message
  public get modalSuccessMessage(): string {
    return this._modalSuccessMessage;
  }

  // Information
  public get showInformation(): boolean {
    return this.state.reportStatus === ReportStatus.information;
  }

  public information(): string {
    return this.state.information();
  }

  // Items not found

  public get showNoItemsFound(): boolean {
    return this.state.reportStatus === ReportStatus.loaded && this.rows.length === 0 && !this.showBuiltInCreateRow;
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

  // agentId
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

  public get showAgentParameter(): boolean {
    const account = this.account();
    return !!account && !!account.AgentDefinitionId;
  }

  public get labelAgent_Manual(): string {
    const account = this.account();
    const defId = !!account ? account.AgentDefinitionId : null;

    return metadata_Agent(this.workspace, this.translate, defId).titleSingular();
  }

  public get readonlyAgent_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.AgentId;
  }

  public get readonlyValueAgentId_Manual(): number {
    const account = this.account();
    return !!account ? account.AgentId : null;
  }

  public get definitionIdsAgent_Manual(): number[] {
    const account = this.account();
    return [account.AgentDefinitionId];
  }

  public get visibleAgentId(): number {
    if (this.showAgentParameter) {
      if (this.readonlyAgent_Manual) {
        return this.readonlyValueAgentId_Manual;
      } else {
        return this.state.arguments.agent_id;
      }
    }
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
    return Math.max(this.MIN_PAGE_SIZE, this.state.arguments.top);
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
    return Math.max(this.MIN_PAGE_SIZE, this.state.arguments.entries_top);
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
    return Math.max(this.MIN_PAGE_SIZE, this.state.arguments.ex_entries_top);
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

  public get showReport(): boolean {
    return this.isUnreconciled;
  }

  public get disableReport(): boolean {
    if (!this.isLoaded) {
      return this.translate.instant('Error_NotLoadedYet');
    }

    return null;
  }

  // This
  public get entriesBalance(): number {
    const res = this.state.unreconciled_response;
    return !!res ? res.EntriesBalance : 0;
  }

  // ... Minus This
  public get unreconciledEntriesBalance(): number {
    const res = this.state.unreconciled_response;
    return !!res ? res.UnreconciledEntriesBalance : 0;
  }

  // ... Plus This
  public get unreconciledExEntriesBalance(): number {
    const res = this.state.unreconciled_response;
    return !!res ? res.UnreconciledExternalEntriesBalance : 0;
  }

  // ... Equals This
  public get exEntriesBalance(): number {
    const res = this.state.unreconciled_response;
    return !!res ? (res.EntriesBalance - res.UnreconciledEntriesBalance + res.UnreconciledExternalEntriesBalance) : 0;
  }

  private get canEditPermissions(): boolean {
    return this.ws.canUpdate('reconciliation', null);
  }

  private get canReadPermissions(): boolean {
    return this.ws.canRead('reconciliation');
  }

  ////////////// Import
  public onSelectFileToImport(input: HTMLInputElement) {
    if (!!this.disableImport) {
      return;
    }

    const files = input.files;
    if (files.length === 0) {
      return;
    }

    const file = files[0];
    input.value = '';

    this.api.import(file).subscribe(
      (exEntries: ExternalEntryForSave[]) => {

        // Just in case
        if (!this.isLoaded) {
          return;
        }

        // Find the index in _rows where the new external entry can be inserted
        let index = this.findIndexForCreate();

        // Add the entries one by one
        const countBefore = this._rows.length;
        for (const exEntry of exEntries) {
          this.addCreatedExEntry(exEntry, index);

          index++;
        }
        const countAfter = this._rows.length;

        if (countBefore !== countAfter) {
          this._rows = this._rows.slice(); // To refresh the virtual scroll
        }

        this.fixCreateExEntryRow(index);
      },
      (friendlyError: any) => {
        this.displayErrorModal(friendlyError.error);
      }
    );
  }

  public get showImport() {
    return this.isUnreconciled;
  }

  public get disableImport(): string {
    if (!this.isLoaded) {
      return this.translate.instant('Error_NotLoadedYet');
    } else if (!this.canEditPermissions) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    }

    return null;
  }

  // Download template
  public onDownloadTemplate(): void {
    // The template is just one row containing 3 headers
    const templateArray = [[
      this.translate.instant('Line_PostingDate'),
      this.translate.instant('Reconciliation_Reference'),
      this.translate.instant('Entry_MonetaryValue'),
    ]];

    const blob = csvPackage(templateArray);
    const filename = this.translate.instant('ImportTemplate') + '.csv';
    downloadBlob(blob, filename);
  }

  public get showDownloadTemplate(): boolean {
    return this.showImport;
  }

  public showExportSpinner: boolean;

  ////////////// Export
  public onExport() {
    if (this.disableExport) {
      return;
    }

    if (this.isUnreconciled) {
      const args = this.UnreconciledArgs;
      const agentLabel = this.labelAgent_Manual;
      const format = this.amountsFormat;

      const obs$: Observable<ReconciliationGetUnreconciledResponse> = this.api.getUnreconciled(args);
      this.showExportSpinner = true;
      obs$.pipe(
        tap((res: ReconciliationGetUnreconciledResponse) => {
          this.showExportSpinner = false;
          const fileName = `${this.translate.instant('Unreconciled')}.csv`;
          const data: string[][] = [
            [
              this.translate.instant('Entry_Account'),
              metadata_Account(this.workspace, this.translate).format(this.ws.get('Account', args.accountId)),
              '', '', '', '', ''
            ],
            [
              agentLabel,
              this.ws.getMultilingualValue('Agent', args.agentId, 'Name'),
              '', '', '', '', ''
            ],
            [
              this.translate.instant('AsOfDate'),
              !!args.asOfDate ? toLocalDateOnlyISOString(new Date(args.asOfDate)) : '',
              '', '', '', '', ''
            ],
            ['', '', '', '', '', '', ''], // Margin
            [
              this.translate.instant('InternalBalance'),
              accountingFormat(res.EntriesBalance, format),
              '', '', '', '', ''
            ],
            [
              this.translate.instant('InternalUnreconciledBalance'),
              accountingFormat(res.UnreconciledEntriesBalance, format),
              '-', '', '', '', ''
            ],
            [
              this.translate.instant('ExternalUnreconciledBalance'),
              accountingFormat(res.UnreconciledExternalEntriesBalance, format),
              '+', '', '', '', ''
            ],
            [
              this.translate.instant('ExternalBalance'),
              accountingFormat(res.EntriesBalance - res.UnreconciledEntriesBalance + res.UnreconciledExternalEntriesBalance, format),
              '=', '', '', '', ''
            ],
            ['', '', '', '', '', '', ''], // Margin
            [
              this.translate.instant('InternalEntries'),
              '',
              '',
              '',
              this.translate.instant('ExternalEntries'),
              '',
              '',
            ], [
              this.translate.instant('Line_Document'),
              this.translate.instant('Line_PostingDate'),
              this.translate.instant('Reconciliation_Reference'),
              this.translate.instant('Entry_MonetaryValue'),
              this.translate.instant('Line_PostingDate'),
              this.translate.instant('Reconciliation_Reference'),
              this.translate.instant('Entry_MonetaryValue'),
            ]];


          const length = Math.max(res.Entries.length, res.ExternalEntries.length);
          for (let i = 0; i < length; i++) {
            const entry = res.Entries[i];
            const exEntry = res.ExternalEntries[i];

            const dataRow = [];
            if (!!entry) {
              dataRow.push(this.formatSerialNumber(entry.DocumentSerialNumber, entry.DocumentDefinitionId));
              dataRow.push(toLocalDateOnlyISOString(new Date(entry.PostingDate)));
              dataRow.push(entry.ExternalReference);
              dataRow.push(accountingFormat(entry.MonetaryValue * entry.Direction, format));
            } else {
              dataRow.push(...['', '', '', '']);
            }

            if (!!exEntry) {
              dataRow.push(toLocalDateOnlyISOString(new Date(exEntry.PostingDate)));
              dataRow.push(exEntry.ExternalReference);
              dataRow.push(accountingFormat(exEntry.MonetaryValue * exEntry.Direction, format));
            } else {
              dataRow.push(...['', '', '']);
            }

            data.push(dataRow);
          }

          const blob = csvPackage(data);
          downloadBlob(blob, fileName);
        }),
        catchError(friendlyError => {
          this.showExportSpinner = false;
          this.displayErrorModal(friendlyError.error);
          return of();
        }),
        finalize(() => {
          this.showExportSpinner = false;
        })
      ).subscribe();

    } else if (this.isReconciled) {
      // TODO
    }
  }

  public get disableExport(): string {
    if (!this.isLoaded) {
      return this.translate.instant('Error_NotLoadedYet');
    } else if (!this.canReadPermissions) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    }

    return null;
  }

  public showExport() {
    return this.isUnreconciled;
  }

  private _postingDateTolerance = 0;
  private _externalRefTolerance = 0;

  private _postingDateToleranceChoices: SelectorChoice[] = [
    { value: 0, name: () => this.translate.instant('ExactMatch') },
    { value: 1, name: () => '1' },
    { value: 2, name: () => '2' },
    { value: 3, name: () => '3' },
    { value: 4, name: () => '4' },
    { value: 5, name: () => '5' },
  ];

  private _externalRefToleranceChoices: SelectorChoice[] = [
    { value: 0, name: () => this.translate.instant('ExactMatch') },
    { value: 1, name: () => this.translate.instant('StringDistance_1', { 0: 1 }) },
    { value: 2, name: () => this.translate.instant('StringDistance_2', { 0: 2 }) },
    { value: 3, name: () => this.translate.instant('StringDistance_3', { 0: 3 }) },
    { value: 1000000, name: () => this.translate.instant('IgnoreMatch') },
  ];

  public get postingDateTolerance(): number {
    return this._postingDateTolerance;
  }

  public set postingDateTolerance(v: number) {
    this._postingDateTolerance = v;
  }

  public get postingDateToleranceChoices(): SelectorChoice[] {
    return this._postingDateToleranceChoices;
  }

  public get externalRefTolerance(): number {
    return this._externalRefTolerance;
  }

  public set externalRefTolerance(v: number) {
    this._externalRefTolerance = v;
  }

  public get externalRefToleranceChoices(): SelectorChoice[] {
    return this._externalRefToleranceChoices;
  }

  private _autoReconcileParamsKey = 'reconciliation/auto-reconcile-params';

  ////////////// Auto-Reconcile
  public onAutoReconcile() {
    if (!this.disableAutoReconcile) {
      // (1) Save the params to user settings, if they have changed
      const postingDateTolerance = this.postingDateTolerance;
      const externalRefTolerance = this.externalRefTolerance;

      const oldArgs = this.customUserSettings.get<AutoReconcileArguments>(this._autoReconcileParamsKey);
      if (!oldArgs || oldArgs.postingDateTolerance !== postingDateTolerance
        || oldArgs.externalRefTolerance !== externalRefTolerance) {
        const args: AutoReconcileArguments = { postingDateTolerance, externalRefTolerance };
        this.customUserSettings.save(this._autoReconcileParamsKey, JSON.stringify(args));
      }

      // (2) Define inline utility functions
      function dateDifference(a: ReconciliationRow, b: ReconciliationRow) {
        const datesDiff = Math.abs(a.entryDays - b.exEntryDays);
        return datesDiff > postingDateTolerance ? Infinity : datesDiff;
      }

      function externalRefDifference(a: ReconciliationRow, b: ReconciliationRow) {
        const exRefsDiff = getEditDistance(
          a.entry.ExternalReference,
          b.exEntry.ExternalReference,
          externalRefTolerance + 1);

        return exRefsDiff > externalRefTolerance ? Infinity : exRefsDiff;
      }

      function difference(a: ReconciliationRow, b: ReconciliationRow): number {
        return dateDifference(a, b) + 1000 * externalRefDifference(a, b);
      }

      function dateMatches(a: ReconciliationRow, b: ReconciliationRow): boolean {
        return dateDifference(a, b) !== Infinity;
      }

      function exRefMatches(a: ReconciliationRow, b: ReconciliationRow): boolean {
        return externalRefDifference(a, b) !== Infinity;
      }

      // (3) Auto-Reconcile (e = entry, ex = external entry)
      // Get the rows that contain unreconciled entries, and unreconciled external entries
      const entryRows = this.rows
        .filter(r => !!r.entry && !r.entry.IsReconciledLater &&
          isSpecified(r.entry.MonetaryValue) && r.entry.PostingDate && !r.entryReconciliation);

      const exEntryRows = this.rows
        .filter(r => !!r.exEntry &&
          isSpecified(r.exEntry.MonetaryValue) && r.exEntry.PostingDate && !r.exEntryReconciliation);

      // Hash the amounts of entries rows
      const eAmountsHash: { [amount: number]: ReconciliationRow[] } = {};
      for (const eRow of entryRows) {
        // Calculate the number of days since 1st Jan 1970, the earliest JS date
        eRow.entryDays = new Date(eRow.entry.PostingDate).getTime() / 86400000; // milliseconds per day

        // Hash the amounts
        const amount = eRow.entry.MonetaryValue * eRow.entry.Direction;
        eAmountsHash[amount] = eAmountsHash[amount] || [];
        eAmountsHash[amount].push(eRow);
      }

      // Hash the amounts of external entries rows
      const exAmountsHash: { [amount: number]: ReconciliationRow[] } = {};
      for (const exRow of exEntryRows) {
        // Calculate the number of days since 1st Jan 1970, the earliest JS date
        exRow.exEntryDays = new Date(exRow.exEntry.PostingDate).getTime() / 86400000; // milliseconds per day

        // Hash the amounts
        const amount = exRow.exEntry.MonetaryValue * exRow.exEntry.Direction;
        exAmountsHash[amount] = exAmountsHash[amount] || [];
        exAmountsHash[amount].push(exRow);
      }

      let reconciliationsCount = 0; // To report once done
      for (const amountKey of Object.keys(eAmountsHash)) {

        // Those two collections contain rows with matching amounts
        const amountERows: ReconciliationRow[] = eAmountsHash[amountKey];
        const amountExRows: ReconciliationRow[] = exAmountsHash[amountKey];

        if (!!amountERows && amountERows.length > 0 && !!amountExRows && amountExRows.length > 0) {
          if (amountERows.length === 1 && amountExRows.length === 1) {
            // Optimization for the common scenario where there is one row on each side
            const eRow = amountERows[0];
            const exRow = amountExRows[0];

            if (dateMatches(eRow, exRow) && exRefMatches(eRow, exRow)) {
              this.addReconciliation([eRow], [exRow]);
              reconciliationsCount++;
            }
          } else {
            // One or both sides have more than one row, match them as best as you can

            // First sort both collections by days, this makes it efficient to weed out non-matches
            amountERows.sort((a, b) => a.entryDays - b.entryDays);
            amountExRows.sort((a, b) => a.exEntryDays - b.exEntryDays);

            // Here we collect all compatible entries, external entries and the difference scores between each pair
            const compatibles: { eIndex: number, exIndex: number, diff: number }[] = [];

            let startingIndex = 0; // The first external entry with a compatible date
            let endingIndexPlusOne = 0; // 1 + The last external entry with a compatible date
            let prevEntryDays: number; // The days of the current entry row
            for (let i = 0; i < amountERows.length; i++) {

              const eRow = amountERows[i];

              // Adjust the indices if it's a new date
              if (eRow.entryDays !== prevEntryDays) {
                prevEntryDays = eRow.entryDays;
                const earliestDays = eRow.entryDays - postingDateTolerance;
                while (startingIndex < amountExRows.length && amountExRows[startingIndex].exEntryDays < earliestDays) {
                  startingIndex++;
                }
                const latestDays = eRow.entryDays + postingDateTolerance;
                while (endingIndexPlusOne < amountExRows.length && amountExRows[endingIndexPlusOne].exEntryDays <= latestDays) {
                  endingIndexPlusOne++;
                }
              }

              // Go over all external entries with compatible dates and calculate the difference
              // Push the results into a large array "compatibles"
              for (let j = startingIndex; j < endingIndexPlusOne; j++) {
                const exRow = amountExRows[j];
                const diff = difference(eRow, exRow);
                if (diff < Infinity) {
                  compatibles.push({ eIndex: i, exIndex: j, diff });
                }
              }
            }

            compatibles.sort((a, b) => a.diff - b.diff); // Best matches will come first
            const eReconciledTracker: { [amount: number]: true } = {}; // entries that were already reconciled
            const exReconciledTracker: { [amount: number]: true } = {}; // external entries that were already reconciled

            for (const match of compatibles) {
              if (!eReconciledTracker[match.eIndex] && !exReconciledTracker[match.exIndex]) {
                eReconciledTracker[match.eIndex] = true;
                exReconciledTracker[match.exIndex] = true;

                const eRow = amountERows[match.eIndex];
                const exRow = amountExRows[match.exIndex];

                this.addReconciliation([eRow], [exRow]);
                reconciliationsCount++;
              }
            }
          }
        }
      }

      // (4) Cleanup
      for (const row of this.rows) {
        delete row.entryDays;
        delete row.exEntryDays;
      }

      const msg = this.translate.instant('AutoReconcileMessage', { 0: reconciliationsCount });
      this.displaySuccessModal(msg);
    }
  }

  public get showAutoReconcile(): boolean {
    return this.isUnreconciled;
  }

  public get disableAutoReconcile(): string {
    if (!this.isLoaded) {
      return this.translate.instant('Error_NotLoadedYet');
    } else if (!this.canEditPermissions) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    }

    return null;
  }

  ////////////// Placements and orientations

  public get actionsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get commandsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public get errorPopoverPlacement(): Placement {
    return 'bottom';
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  ////////////// Refresh

  public onRefresh(): void {
    // The if statement to deal with incessant button clickers (Users who hit refresh repeatedly)
    if (this.state.reportStatus !== ReportStatus.loading) {
      this.fetch();
    }
  }

  public get disableRefresh(): boolean {
    return !this.requiredParametersAreSet;
  }

  // Paging for reconciled entities

  get from(): number {
    const skip = this.state.arguments.skip;
    return Math.min(skip + 1, this.total);
  }

  get to(): number {
    const res = this.state.reconciled_response;
    const page = !!res ? res.Reconciliations.length : this.top;
    return Math.min(this.state.arguments.skip + page, this.total);
  }

  get total(): number {
    return this.state.reconciled_count || 0;
  }

  onPreviousPage() {
    const args = this.state.arguments;
    args.skip = Math.max(args.skip - this.top, 0);

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canPreviousPage(): boolean {
    return this.state.arguments.skip > 0;
  }

  onNextPage() {
    const args = this.state.arguments;
    args.skip = args.skip + this.top;

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  // Paging for Internal Entries

  get entries_from(): number {
    const entries_skip = this.state.arguments.entries_skip;
    return Math.min(entries_skip + 1, this.entries_total);
  }

  get entries_to(): number {
    const res = this.state.unreconciled_response;
    const page = !!res ? res.Entries.length : this.entriesTop;
    return Math.min(this.state.arguments.entries_skip + page, this.entries_total);
  }

  get entries_total(): number {
    return this.state.unreconciled_entries_count || 0;
  }

  entries_onPreviousPage() {
    const args = this.state.arguments;
    args.entries_skip = Math.max(args.entries_skip - this.entriesTop, 0);

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get entries_canPreviousPage(): boolean {
    return this.state.arguments.entries_skip > 0;
  }

  entries_onNextPage() {
    const args = this.state.arguments;
    args.entries_skip = args.entries_skip + this.entriesTop;

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get entries_canNextPage(): boolean {
    return this.entries_to < this.entries_total;
  }

  // Paging for External Entries

  get ex_entries_from(): number {
    const ex_entries_skip = this.state.arguments.ex_entries_skip;
    return Math.min(ex_entries_skip + 1, this.ex_entries_total);
  }

  get ex_entries_to(): number {
    const res = this.state.unreconciled_response;
    const page = !!res ? res.ExternalEntries.length : this.externalEntriesTop;
    return Math.min(this.state.arguments.ex_entries_skip + page, this.ex_entries_total);
  }

  get ex_entries_total(): number {
    return this.state.unreconciled_ex_entries_count || 0;
  }

  ex_entries_onPreviousPage() {
    const args = this.state.arguments;
    args.ex_entries_skip = Math.max(args.ex_entries_skip - this.externalEntriesTop, 0);

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get ex_entries_canPreviousPage(): boolean {
    return this.state.arguments.ex_entries_skip > 0;
  }

  ex_entries_onNextPage() {
    const args = this.state.arguments;
    args.ex_entries_skip = args.ex_entries_skip + this.externalEntriesTop;

    this.urlStateChanged(); // to update the URL state
    this.fetch();
  }

  get ex_entries_canNextPage(): boolean {
    return this.ex_entries_to < this.ex_entries_total;
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
                const row: ReconciliationRow = { reconciliation };

                // The first row will have a button to unreconcile
                // And all cells below the button will have no top border to make it look like one big cell
                if (i === 0) {
                  row.firstOne = length;
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
            for (let i = 0; i < length; i++) {
              const row: ReconciliationRow = {};

              // The entry
              row.entry = res.Entries[i];
              row.exEntry = res.ExternalEntries[i];

              this._rows.push(row);
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

  public onDeleteReconciliation(row: ReconciliationRow) {
    const reconciliation = row.reconciliation;
    if (!!reconciliation) {
      if (!!reconciliation.Id) {
        this.deletedReconciliationIds.push(+reconciliation.Id);
      }

      this._rows = this._rows.filter(r => r.reconciliation !== reconciliation);
    }
  }

  private _showBuiltInCreateRow = false;

  public get showBuiltInCreateRow(): boolean {
    return this._showBuiltInCreateRow &&
      this.isUnreconciled &&
      this.requiredParametersAreSet &&
      this.state.reportStatus === ReportStatus.loaded;
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
      if (row.reconciliation || !!row.exEntry) {
        break;
      }
    }

    return index + 1;
  }

  public onRowDoubleClick(row: ReconciliationRow) {
    if (this.isUnreconciled) {
      if (row.exEntryIsCreate) {
        this.onCreateExEntry();
      } else {
        this.onEditExEntry(row);
      }
    }
  }

  /**
   * Returns the index of the row where the ex entry was created
   */
  public onCreateExEntryOld(): number {
    // Just in case
    if (!this.isLoaded) {
      return;
    }

    // (1) Find the index in _rows where the new external entry can be inserted
    const index = this.findIndexForCreate();

    // (2) Create the external entry and add it to entities for save
    const exEntry: ExternalEntryForSave = { Direction: 1 };
    this.externalEntriesForSave.push(exEntry);
    const exEntryIndex = this.externalEntriesForSave.length - 1;

    // (3) Add the external entry to the view and make it editable
    if (this._rows.length <= index) {
      this._rows.push({ exEntryIsEdit: true, exEntry, exEntryIndex });
      this._rows = this._rows.slice(); // To refresh the virtual scroll
    } else {
      const row = this._rows[index];
      row.exEntry = exEntry;
      row.exEntryIsEdit = true;
      row.exEntryIndex = exEntryIndex;
    }

    this.fixCreateExEntryRow(index + 1);

    return index;
  }

  /**
   * Returns the index of the row where the ex entry was created
   */
  public onCreateExEntry(): number {
    // Just in case
    if (!this.isLoaded) {
      return;
    }

    // (1) Find the index in _rows where the new external entry can be inserted
    const index = this.findIndexForCreate();

    // (2) Create the external entry and add it to entities for save
    const exEntry: ExternalEntryForSave = { Direction: 1 };

    const countBefore = this._rows.length;
    this.addCreatedExEntry(exEntry, index);
    const countAfter = this._rows.length;

    if (countBefore !== countAfter) {
      this._rows = this._rows.slice(); // To refresh the virtual scroll
    }

    this.fixCreateExEntryRow(index + 1);
    return index;
  }

  /**
   * Creates the entry at the specified index without refreshing the scroll or fixing create ex entry row.
   * Used when adding batch ex entries for performance
   */
  private addCreatedExEntry(exEntry: ExternalEntryForSave, index: number): number {
    // (2) Create the external entry and add it to entities for save
    this.externalEntriesForSave.push(exEntry);
    const exEntryIndex = this.externalEntriesForSave.length - 1;

    // (3) Add the external entry to the view and make it editable
    if (this._rows.length <= index) {
      this._rows.push({ exEntryIsEdit: true, exEntry, exEntryIndex });
    } else {
      const row = this._rows[index];
      row.exEntry = exEntry;
      row.exEntryIsEdit = true;
      row.exEntryIndex = exEntryIndex;
    }

    return index;
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
      prevRow.exEntryReconciliation = nextRow.exEntryReconciliation;
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
      delete lastRow.exEntryReconciliation;
    }
  }

  public onEditExEntry(row: ReconciliationRow): void {
    if (!row.exEntryIsEdit && !!row.exEntry) {
      row.exEntryIsEdit = true;
      row.exEntryOriginal = row.exEntry;
      row.exEntry = JSON.parse(JSON.stringify(row.exEntryOriginal)); // Store the original safely to support cancellation
    }
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

      // Clear all errors
      for (const exEntry of this.externalEntriesForSave) {
        if (!!exEntry) {
          delete exEntry.serverErrors;
        }
      }
      for (const reconciliation of this.reconciliationsForSave) {
        delete reconciliation.serverErrors;
        for (const e of reconciliation.Entries) {
          delete e.serverErrors;
        }
        for (const e of reconciliation.ExternalEntries) {
          delete e.serverErrors;
        }
      }

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
            s.unreconciled_entries_count = response.UnreconciledEntriesCount;
            s.unreconciled_ex_entries_count = response.UnreconciledExternalEntriesCount;
          })
        );
      } else if (this.isReconciled) {
        obs$ = this.api.saveAndGetReconciled(payload, this.ReconciledArgs).pipe(
          map(response => {
            // Add the result to the state
            s.reconciled_response = response;
            s.reconciled_count = response.ReconciledCount;
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
            const errorsDic = friendlyError.error as { [key: string]: string[] };
            const keys = Object.keys(errorsDic);
            const unboundErrors: string[] = [];
            for (const key of keys) {
              const errors = errorsDic[key];
              const steps = key.split('.');

              const step1 = steps[0];
              if (step1.startsWith('ExternalEntries') && steps.length > 1) {
                const parts1 = step1.substring(0, step1.length - 1).split('[');
                const exEntryIndex = +parts1[1];
                if (isNaN(exEntryIndex)) {
                  unboundErrors.push(...errors);
                } else {
                  const step2 = steps[1];
                  switch (step2) {
                    case 'PostingDate':
                    case 'ExternalReference':
                    case 'MonetaryValue':
                      const exEntry = payload.ExternalEntries[exEntryIndex];
                      exEntry.serverErrors = exEntry.serverErrors || {};
                      exEntry.serverErrors[step2] = errors;
                      break;
                    default:
                      unboundErrors.push(...errors);
                  }
                }
              } else if (step1.startsWith('Reconciliations') && steps.length > 2) {
                const parts1 = step1.substring(0, step1.length - 1).split('[');
                const reconciliationIndex = +parts1[1];
                if (isNaN(reconciliationIndex)) {
                  unboundErrors.push(...errors);
                } else {
                  const reconciliation = payload.Reconciliations[reconciliationIndex];
                  reconciliation.serverErrors = reconciliation.serverErrors || {};
                  const step2 = steps[1];
                  if (step2.startsWith('Entries')) {
                    reconciliation.serverErrors.Entries = reconciliation.serverErrors.Entries || [];
                    reconciliation.serverErrors.Entries.push(...errors);

                    // const parts2 = step2.substring(0, step2.length - 1).split('[');
                    // const entryIndex = +parts2[1];
                    // if (isNaN(entryIndex)) {
                    //   unboundErrors.push(...errors);
                    // } else {
                    //   const step3 = steps[2];
                    //   if (step3 === 'EntryId') {
                    //     const entry = reconciliation.Entries[entryIndex];
                    //     entry.serverErrors = entry.serverErrors || {};
                    //     entry.serverErrors[step2] = errors;
                    //   } else {
                    //     unboundErrors.push(...errors);
                    //   }
                    // }
                  } else if (step2.startsWith('ExternalEntries')) {
                    reconciliation.serverErrors.ExternalEntries = reconciliation.serverErrors.ExternalEntries || [];
                    reconciliation.serverErrors.ExternalEntries.push(...errors);

                    // const parts2 = step2.substring(0, step2.length - 1).split('[');
                    // const exEntryIndex = +parts2[1];
                    // if (isNaN(exEntryIndex)) {
                    //   unboundErrors.push(...errors);
                    // } else {
                    //   const step3 = steps[2];
                    //   if (step3 === 'ExternalEntryId') {
                    //     const exEntry = reconciliation.ExternalEntries[exEntryIndex];
                    //     exEntry.serverErrors = exEntry.serverErrors || {};
                    //     exEntry.serverErrors[step2] = errors;
                    //   } else {
                    //     unboundErrors.push(...errors);
                    //   }
                    // }
                  } else {
                    unboundErrors.push(...errors);
                  }
                }
              } else {
                unboundErrors.push(...errors);
              }
            }

            // If there are unbound errors, show them in a modal
            if (unboundErrors.length > 0) {
              const tracker: { [key: string]: true } = {};
              for (const error of unboundErrors) {
                tracker[error] = true;
              }

              const distinctUnboundErrors = Object.keys(tracker);

              const newline = `
`;
              const top = 10;
              let errorMessage = distinctUnboundErrors.slice(0, top).map(e => ` - ${e}`).join(newline);
              if (distinctUnboundErrors.length > top) {
                errorMessage += '...'; // To show that's not all
              }

              this.displayErrorModal(errorMessage);
            }
          } else {
            this.displayErrorModal(friendlyError.error);
          }

          return of(null);
        })
      ).subscribe();
    }
  }

  private _modalErrorMessage: string; // in the modal

  public displayErrorModal(errorMessage: string) {
    // shows the error message in a dismissable modal
    this._modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  private _modalSuccessMessage: string; // in the modal

  public displaySuccessModal(successMsg: string) {
    // shows the error message in a dismissable modal
    this._modalSuccessMessage = successMsg;
    this.modalService.open(this.successModal);
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
    return this.checkedExEntries.map(e => e.Direction * (e.MonetaryValue || 0)).reduce((a, b) => a + b, 0);
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

  public onCloneAndReconcile(entryRow: ReconciliationRow) {
    const entry = entryRow.entry;
    if (!!entry) {
      // Create a new entry and copy all the values across.
      const exEntryRowIndex = this.onCreateExEntry();
      const exEntryRow = this.rows[exEntryRowIndex];
      const exEntry = exEntryRow.exEntry;

      exEntry.PostingDate = entry.PostingDate;
      exEntry.ExternalReference = entry.ExternalReference;
      exEntry.Direction = entry.Direction;
      exEntry.MonetaryValue = entry.MonetaryValue;

      // Reconcile
      entryRow.entryIsChecked = true;
      exEntryRow.exEntryIsChecked = true;
      // this.addReconciliation([entryRow], [exEntryRow]);
    }
  }

  public onUndoCloneAndReconcile(row: ReconciliationRow) {
    const reconciliation = row.entryReconciliation;

    // Find all external entries and delete them
    const exEntryRowPairs = this._rows
      .map((r, i) => ({ row: r, index: i })) // capture the indices
      .filter(pair => pair.row.exEntryReconciliation === reconciliation);

    for (const pair of exEntryRowPairs) {
      this.onDeleteExEntry(pair.row, pair.index);
    }

    // Remove the reconciliation from the list for save
    this.removeReconciliation(reconciliation);
  }

  public onReconcileChecked() {
    if (!this.disableReconcileChecked) {

      // Search for checked entries and external entries
      const entryRows: ReconciliationRow[] = [];
      const exEntryRows: ReconciliationRow[] = [];

      for (const row of this._rows) {
        if (row.entryIsChecked && !!row.entry) {
          entryRows.push(row);
        }

        if (row.exEntryIsChecked && !!row.exEntry) {
          exEntryRows.push(row);
        }
      }

      // Reconcile them
      this.addReconciliation(entryRows, exEntryRows);
    }
  }

  public get disableReconcileChecked(): boolean {
    if (!this.canEditPermissions) {
      return this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
    } else if (this.checkedEntries.length > 1 && this.checkedExEntries.length > 1) {
      return this.translate.instant('Error_ManyToManyReconciliation');
    } else if (Math.round(this.checkedEntriesTotal * 10000) !== Math.round(this.checkedExEntriesTotal * 10000)) {
      // Rounding is key, since JS addition produces slightly different results when you change the order due to floating precision
      return this.translate.instant('Error_TotalsImbalance');
    }

    return null;
  }

  /**
   * Creates a reconciliation for the supplied entries and external entries, and flags the rows as reconciled
   */
  private addReconciliation(entryRows: ReconciliationRow[], exEntryRows: ReconciliationRow[]): void {
    const reconciliation: ReconciliationForSave = {
      Entries: [],
      ExternalEntries: []
    };

    for (const row of entryRows) {
      delete row.entryIsChecked;
      row.entryReconciliation = reconciliation;
      const entry = row.entry;
      reconciliation.Entries.push({ EntryId: +entry.Id });
    }

    for (const row of exEntryRows) {
      delete row.exEntryIsChecked;
      row.exEntryReconciliation = reconciliation;
      const exEntry = row.exEntry;
      reconciliation.ExternalEntries.push({ ExternalEntryId: +exEntry.Id || undefined, ExternalEntryIndex: row.exEntryIndex });
    }

    this.reconciliationsForSave.push(reconciliation);
  }

  public onUndoReconcileCheckedEntry(row: ReconciliationRow) {
    this.removeReconciliation(row.entryReconciliation);
  }

  public onUndoReconciledCheckedExEntry(row: ReconciliationRow) {
    this.removeReconciliation(row.exEntryReconciliation);
  }

  private removeReconciliation(reconciliation: ReconciliationForSave) {
    if (!reconciliation) {
      return;
    }

    // Remove it from the reconciliations for save
    this.reconciliationsForSave = this.reconciliationsForSave.filter(e => e !== reconciliation);

    // deletes it from all the rows
    for (const row of this._rows) {
      if (row.entryReconciliation === reconciliation) {
        delete row.entryReconciliation;
      }

      if (row.exEntryReconciliation === reconciliation) {
        delete row.exEntryReconciliation;
      }
    }
  }

  public onCancelReconcileChecked() {
    this.uncheckAll();
  }

  private amountsDecimalsAccount: Account;
  private amountsDecimalsAgent: Agent;
  private amountsDecimalsResult: number;
  public get amountsDecimals(): number {
    const agent = this.ws.get('Agent', this.visibleAgentId);
    const account = this.ws.get('Account', this.accountId);
    if (this.amountsDecimalsAgent !== agent && this.amountsDecimalsAccount !== account) {
      this.amountsDecimalsAgent = agent;
      this.amountsDecimalsAccount = account;

      const currencyId = (!!account ? account.CurrencyId : null) || (!!agent ? agent.CurrencyId : null);
      const currency = this.ws.get('Currency', currencyId) as Currency;
      this.amountsDecimalsResult = !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
    }

    return this.amountsDecimalsResult;
  }

  private amountsFormatAccount: Account;
  private amountsFormatAgent: Agent;
  private amountsFormatResult: string;
  public get amountsFormat(): string {
    const agent = this.ws.get('Agent', this.visibleAgentId);
    const account = this.ws.get('Account', this.accountId);
    if (this.amountsFormatAgent !== agent && this.amountsFormatAccount !== account) {
      this.amountsFormatAccount = agent;
      this.amountsFormatAgent = account;

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

  public formatSerialNumber(serial: number, docDefId: number) {
    return formatSerialFromDefId(serial, this.ws, docDefId);
  }

  // Collapse parameters
  public get collapseParameters(): boolean {
    return this.state.collapseParams;
  }

  public set collapseParameters(v: boolean) {
    const s = this.state;
    if (s.collapseParams !== v) {
      s.collapseParams = v;
    }
  }

  // Hovering over a reconciled entry/external entry highlights all entries/external entries with yellow marker

  public onToggleCollapseParameters() {
    this.collapseParameters = !this.collapseParameters;
  }

  private _hoveredReconciliation: Reconciliation;

  public onReconciledEntryMouseEnter(row: ReconciliationRow) {
    this._hoveredReconciliation = row.entryReconciliation;
  }

  public onReconciledEntryMouseLeave(_: ReconciliationRow) {
    delete this._hoveredReconciliation;
  }

  public onReconciledExEntryMouseEnter(row: ReconciliationRow) {
    this._hoveredReconciliation = row.exEntryReconciliation;
  }

  public onReconciledExEntryMouseLeave(_: ReconciliationRow) {
    delete this._hoveredReconciliation;
  }

  public isHoveredEntryReconciliation(row: ReconciliationRow) {
    return !!this._hoveredReconciliation && this._hoveredReconciliation === row.entryReconciliation;
  }

  public isHoveredExEntryReconciliation(row: ReconciliationRow) {
    return !!this._hoveredReconciliation && this._hoveredReconciliation === row.exEntryReconciliation;
  }

  // ALT+R to reconcile

  // this captures all keydown events from the root document
  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (this.workspace.ignoreKeyDownEvents) {
      return;
    }

    if (event.altKey && event.code === 'KeyR') {
      if (this.showCheckedEntriesToolbar) {
        this.onReconcileChecked();
      }
    }
  }
}

interface ReconciliationRow {
  // Reconciled Stuff
  reconciliation?: Reconciliation;
  firstOne?: number;
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

  /**
   * The entry is reconciled in memory ready to be saved
   */
  entryReconciliation?: ReconciliationForSave;

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
   * The external entry is reconciled in memory ready to be saved
   */
  exEntryReconciliation?: ReconciliationForSave;

  /**
   * Inserted or updates external entries will have their indices stored here
   */
  exEntryIndex?: number;

  /**
   * Indicates that the external entry in this row is in edit mode
   */
  exEntryIsEdit?: boolean;

  /**
   * Indicates that you can create an external entry in this row (No need to copy this one when shifting up or down)
   */
  exEntryIsCreate?: boolean;

  ///////////////// Auto-Reconciliation

  /**
   * Number of days of entry posting date since 1st Jan 1970
   */
  entryDays?: number;

  /**
   * Number of days of external entry posting date since 1st Jan 1970
   */
  exEntryDays?: number;
}
