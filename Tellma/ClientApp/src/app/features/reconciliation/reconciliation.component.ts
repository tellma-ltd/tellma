import { Component, OnInit, OnDestroy } from '@angular/core';
import { WorkspaceService, ReconciliationStore } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { Subscription, Subject, Observable } from 'rxjs';
import { ApiService } from '~/app/data/api.service';
import { switchMap } from 'rxjs/operators';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';

type View = 'unreconciled' | 'reconciled' | 'report';

@Component({
  selector: 't-reconciliation',
  templateUrl: './reconciliation.component.html',
  styles: []
})
export class ReconciliationComponent implements OnInit, OnDestroy, ICanDeactivate {

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();
  private notifyDestruct$ = new Subject<void>();
  private api = this.apiService.reconciliationApi(this.notifyDestruct$); // Only for intellisense

  constructor(
    private workspace: WorkspaceService, private router: Router,
    private route: ActivatedRoute, private apiService: ApiService) { }

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

      const view: View = (params.get('view') || 'unreconciled') as View;
      if (args.view !== view) {
        args.view = view;
        fetchIsNeeded = true;
      }

      const accountId = +params.get('account_id') || undefined;
      if (args.account_id !== accountId) {
        args.account_id = accountId;
        fetchIsNeeded = true;
      }

      const custodyId = +params.get('custody_id') || undefined;
      if (args.custody_id !== custodyId) {
        args.custody_id = custodyId;
        fetchIsNeeded = true;
      }

      // Remaining params
      // if ()

      if (fetchIsNeeded) {
        this.fetch();
      }
    }));
  }

  ngOnDestroy(): void {
    this.notifyDestruct$.next();
    this._subscriptions.unsubscribe();
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

  public get state(): ReconciliationStore {

    const ws = this.workspace.currentTenant;
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
}
