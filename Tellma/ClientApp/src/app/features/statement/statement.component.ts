// tslint:disable:member-ordering
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, ParamMap, Params } from '@angular/router';
import { Subscription, Subject, Observable } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, ReportStore } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 't-statement',
  templateUrl: './statement.component.html',
  styles: []
})
export class StatementComponent implements OnInit, OnDestroy {

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject<void>();

  @Input()
  type: 'account' | 'contract';

  constructor(
    private route: ActivatedRoute, private router: Router,
    private translate: TranslateService, private workspace: WorkspaceService) { }

  ngOnInit(): void {

    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {
      this._subscriptions.add(this.notifyFetch$.pipe().subscribe(() => this.doFetch()));

      if (this.isAccount) {

      }

      if (this.isContract) {
        // TODO
      }
    }));
  }

  ngOnDestroy(): void {
    this._subscriptions.unsubscribe();
  }

  private urlStateChange(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    if (this.isAccount) {
      const params: Params = {};

      // TODO
      // if (!!this.definition && this.definition.Type === 'Summary') {
      //   params.view = this.view;
      // }

      // if (!!this.definition && this.definition.Type === 'Details' && !!this.state.skip) {
      //   params.skip = this.state.skip;
      // }

      // this.parameters.forEach(p => {
      //   const value = this.arguments[p.key];
      //   if (isSpecified(value)) {
      //     params[p.key] = value + '';
      //   }
      // });

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    } else {

    }
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

    this.urlStateChange(); // to update the URL state
    this.notifyFetch$.next();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + this.DEFAULT_PAGE_SIZE;

    this.urlStateChange(); // to update the URL state
    this.notifyFetch$.next();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }
}
