import { Component, OnInit, OnDestroy } from '@angular/core';
import { WorkspaceService, ReconciliationStore, ReportStatus } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { ApiService } from '~/app/data/api.service';
import { catchError, switchMap } from 'rxjs/operators';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { TranslateService } from '@ngx-translate/core';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Account } from '~/app/data/entities/account';

type View = 'unreconciled' | 'reconciled';

@Component({
  selector: 't-reconciliation',
  templateUrl: './reconciliation.component.html',
  styles: []
})
export class ReconciliationComponent implements OnInit, OnDestroy, ICanDeactivate {

  private argumentsKey = 'reconciliation/arguments';
  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.reconciliationApi(this.notifyDestruct$); // Only for intellisense

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
    private route: ActivatedRoute, private apiService: ApiService, private translate: TranslateService) { }

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

    // Update the URL
    this.urlStateChanged();

    // Save the arguments in user settings
    const argsString = JSON.stringify(this.state.arguments);
    this.customUserSettings.save(this.argumentsKey, argsString);

    // Refresh the results
    this.fetch();
  }

  public fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {
    // For robustness grab a reference to the state object, in case it changes later
    const s = this.state;

    // if (!this.requiredParametersAreSet) {
    //   s.reportStatus = ReportStatus.information;
    //   s.information = () => this.translate.instant('FillRequiredFields');
    //   return of();
    // } else if (this.loadingRequiredParameters) {
    //   // Wait until required parameters have loaded
    //   // They will call fetch again once they load
    //   s.reportStatus = ReportStatus.loading;
    //   s.result = [];
    //   return of();
    // } else {
    //   s.reportStatus = ReportStatus.loading;
    //   s.result = [];

    //   // Prepare the query params
    //   const args = this.computeStatementArguments();
    //   return this.api.statement(args).pipe(
    //     tap(response => {
    //       // Result is loaded
    //       s.reportStatus = ReportStatus.loaded;

    //       // Add the result to the state
    //       s.result = response.Result;
    //       s.top = response.Top;
    //       s.skip = response.Skip;
    //       s.total = response.TotalCount;
    //       s.extras = {
    //         opening: response.Opening,
    //         openingQuantity: response.OpeningQuantity,
    //         openingMonetaryValue: response.OpeningMonetaryValue,
    //         closing: response.Closing,
    //         closingQuantity: response.ClosingQuantity,
    //         closingMonetaryValue: response.ClosingMonetaryValue
    //       };

    //       // Merge the related entities and Notify everyone
    //       mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);
    //       this.workspace.notifyStateChanged();
    //     }),
    //     catchError((friendlyError: FriendlyError) => {
    //       s.reportStatus = ReportStatus.error;
    //       s.errorMessage = friendlyError.error;
    //       return of(null);
    //     })
    //   );
    // }

    return null;
  }

  private get requiredParametersAreSet(): boolean {
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
    return true;
    // if (this.isDirty) {

    //   // IF there are unsaved changes, prompt the user asking if they would like them discarded
    //   const modal = this.modalService.open(this.unsavedChangesModal);

    //   // capture the user's decision in a subject:
    //   // first action when the user presses one of the two buttons
    //   // second func is when the user dismisses the modal with x or ESC or clicking the background
    //   const decision$ = new Subject<boolean>();
    //   modal.result.then(
    //     v => { decision$.next(v); decision$.complete(); },
    //     _ => { decision$.next(false); decision$.complete(); }
    //   );

    //   // return the subject that will eventually emit the user's decision
    //   return decision$;

    // } else {

    //   // IF there are no unsaved changes, the navigation can happily proceed
    //   return true;
    // }
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

  public get readonlyCustody_Manual(): boolean {
    const account = this.account();
    return !!account && !!account.CustodyId;
  }
}
